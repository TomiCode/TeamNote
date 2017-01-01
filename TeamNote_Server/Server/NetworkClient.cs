using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

using Google.Protobuf;

using TeamNote.Protocol;

namespace TeamNote.Server
{
  class NetworkClient
  {
    public const int LISTEN_MESSAGE_SIZE = 512;

    /* Client information fields class. */
    public class ClientProfile
    {
      public string Name { get; private set; }
      public string Surname { get; private set; }
      public string Status { get; set; }

      public void UpdateProfile(AuthorizationRequest clientRequest)
      {
        this.Name = clientRequest.Name;
        this.Surname = clientRequest.Surname;
        Debug.Log("Updating profile from client request. [{0} {1}]", this.Name, this.Surname);
      }
    }

    /* Delegates and events. */
    public delegate void ClientMessageHandler(NetworkClient client, NetworkPacket packet);
    public event ClientMessageHandler onClientMessage; 

    /* Client private class members. */
    private PgpPublicKey m_publicKey;
    private TcpClient m_networkClient;
    private Thread m_listenThread;

    /* Client profile informations. */
    private ClientProfile m_clientProfile;

    /* Public properties. */
    public bool IsConnected {
      get {
        return this.m_networkClient.Connected;
      }
    }

    public ClientProfile Profile {
      get {
        return this.m_clientProfile;
      }
    }

    /* Public methods. */
    public NetworkClient(TcpClient clientInstance)
    {
      this.m_networkClient = clientInstance;
      this.m_listenThread = new Thread(this.ListenThread);
    }

    public void Start()
    {
      Debug.Log("Starting client threads.");
      this.m_listenThread.Start();
    }

    public void UpdatePublicKey(PublicKey clientPublicKey)
    {
      byte[] modulus = Convert.FromBase64String(clientPublicKey.Modulus);
      byte[] exponent = Convert.FromBase64String(clientPublicKey.Exponent);

      RsaKeyParameters publicKeyParameters = new RsaKeyParameters(false, new BigInteger(modulus), new BigInteger(exponent));
      this.m_publicKey = new PgpPublicKey(PublicKeyAlgorithmTag.RsaGeneral, publicKeyParameters, DateTime.Now);

      Debug.Log("Updated client PublicKey.");
    }

    public bool SendMessage(int type, IMessage message)
    {
      return this.SendMessage(type, message, true);
    }

    public bool SendMessage(int type, IMessage message, bool encrypt)
    {
      Debug.Log("Sending server message Type={0} Encrypted={1}.", type, encrypt);

      NetworkPacket l_networkPacket = new NetworkPacket();
      l_networkPacket.Type = type;
      l_networkPacket.Server = true;

      if (encrypt) {
        l_networkPacket.Encrypted = true;

      }
      else {
        l_networkPacket.Message = message.ToByteString();
      }

      return this.SendMessage(l_networkPacket);
    }

    private bool SendMessage(NetworkPacket packet)
    {
      int sendBytes = this.m_networkClient.Client.Send(packet.ToByteArray());
      Debug.Log("Send {0} bytes. Packet size: {1} bytes.", sendBytes, packet.CalculateSize());
      return (sendBytes == packet.CalculateSize());
    }

    public bool ForwardNetworkPacket(long senderClientId, NetworkPacket packet)
    {
      Debug.Log("Sending message from {0}, Type={1:X2}.", senderClientId, packet.Type);

      NetworkPacket l_forwardPacket = new NetworkPacket();
      l_forwardPacket.Server = false;
      l_forwardPacket.ClientId = senderClientId;

      l_forwardPacket.Encrypted = packet.Encrypted;
      l_forwardPacket.Message = packet.Message;
      l_forwardPacket.Type = packet.Type;

      return this.SendMessage(l_forwardPacket);
    }

    private void ListenThread()
    {
      Debug.Log("Starting listening for client.");
      byte[] messageBuffer = new byte[LISTEN_MESSAGE_SIZE];
      int bytesReceived = 0;

      while ((bytesReceived = this.m_networkClient.Client.Receive(messageBuffer)) != 0) {
        Debug.Log("Received {0} bytes.", bytesReceived);
        NetworkPacket receivedMessage = NetworkPacket.Parser.ParseFrom(new CodedInputStream(messageBuffer, 0, bytesReceived));

        this.onClientMessage?.Invoke(this, receivedMessage);
      }
    }

  }
}
