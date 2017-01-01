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
    class ClientProfile
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

    public const int LISTEN_MESSAGE_SIZE = 512;

    public delegate void ClientMessageHandler(int type, ByteString message, NetworkClient client);
    public event ClientMessageHandler onClientMessage; 

    private PgpPublicKey m_publicKey;
    private TcpClient m_networkClient;
    private Thread m_listenThread;

    private string m_clientName;
    private string m_clientSurname;
    
    public string Name {
      get {
        return this.m_clientName;
      }
    }

    public string Surname {
      get {
        return this.m_clientSurname;
      }
    }

    public bool IsConnected {
      get {
        return this.m_networkClient.Connected;
      }
    }

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

    public bool SendMessage(int type, IMessage message, bool encrypt)
    {
      Debug.Log("Sending message Type={0:X8} Encrypt={1}.", type, encrypt);
      NetworkPacket l_networkPacket = new NetworkPacket();
      l_networkPacket.Type = type;

      if (encrypt) {

      }
      else {
        l_networkPacket.Message = message.ToByteString();
      }

      return this.SendClientData(l_networkPacket.ToByteArray());
    }

    public bool ForwardMessage(int type, ByteString message)
    {
      Debug.Log("Message Type={0:X8} Size={1}.", type, message.Length);
      NetworkPacket l_networkPacket = new NetworkPacket();
      l_networkPacket.Type = type;
      l_networkPacket.Message = message;

      return this.SendClientData(l_networkPacket.ToByteArray());
    }

    public void SetClientInfo(string name, string surname)
    {
      Debug.Log("Set client Name={0} Surname={1}.", name, surname);
      this.m_clientName = name;
      this.m_clientSurname = surname;
    }

    private bool SendClientData(byte[] buffer)
    {
      int bytesSend = this.m_networkClient.Client.Send(buffer);
      Debug.Log("Bytes send: {0}.", bytesSend);

      return true;
    }

    private void ListenThread()
    {
      Debug.Log("Starting listening for client.");
      byte[] messageBuffer = new byte[LISTEN_MESSAGE_SIZE];
      int bytesReceived = 0;

      while ((bytesReceived = this.m_networkClient.Client.Receive(messageBuffer)) != 0) {
        Debug.Log("Received {0} bytes.", bytesReceived);
        NetworkPacket receivedMessage = NetworkPacket.Parser.ParseFrom(new CodedInputStream(messageBuffer, 0, bytesReceived));
        this.onClientMessage?.Invoke(receivedMessage.Type, receivedMessage.Message, this);
      }
    }

  }
}
