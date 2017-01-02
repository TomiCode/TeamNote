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
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using System.IO;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Digests;

namespace TeamNote.Server
{
  class NetworkClient
  {
    public const int LISTEN_MESSAGE_SIZE = 1024;

    /* Client information fields class. */
    public class ClientProfile
    {
      public string Name { get; private set; }
      public string Surname { get; private set; }
      public bool Status { get; set; }

      public bool Valid {
        get {
          return (this.Name != null && this.Name != string.Empty && this.Surname != null && this.Surname != string.Empty); 
        }
      }

      public void UpdateProfile(AuthorizationRequest clientRequest)
      {
        this.Name = clientRequest.Name;
        this.Surname = clientRequest.Surname;
        Debug.Log("Updating profile from client request. [{0} {1}]", this.Name, this.Surname);
      }
    }

    /* Delegates. */
    public delegate void ReceivedClientMessageHandler(NetworkClient sender, NetworkPacket packet);
    public delegate void ReceivedServerMessageHandler(NetworkClient sender, int type, ByteString message);
    public delegate AsymmetricKeyParameter RequestServerCipherKey();
    
    /* Public events. */
    public event ReceivedClientMessageHandler onClientMessageRequest;
    public event ReceivedServerMessageHandler onServerMessageRequest;

    /* Client private class members. */
    private AsymmetricKeyParameter m_clientPublicKey;
    private TcpClient m_networkClient;
    private Thread m_listenThread;

    /* Client profile informations. */
    private ClientProfile m_clientProfile;
    private long m_clientId;

    /* Request private server key. */
    private RequestServerCipherKey m_clientRequestServerKey;

    /* Public properties. */
    public bool IsConnected {
      get {
        return this.m_networkClient.Connected;
      }
    }

    public long ClientId {
      get {
        return this.m_clientId;
      }
    }

    public ClientProfile Profile {
      get {
        return this.m_clientProfile;
      }
    }
    
    public ContactUpdate.Types.Client ContactClient {
      get {
        ContactUpdate.Types.Client resultClient = new ContactUpdate.Types.Client();
        resultClient.ClientId = this.m_clientId;
        resultClient.Surname = this.Profile.Surname;
        resultClient.Name = this.Profile.Name;
        resultClient.Online = this.Profile.Status;

        return resultClient;
      }
    }

    /* Public methods. */
    public NetworkClient(TcpClient clientInstance, RequestServerCipherKey keyRequester)
    {
      /* TcpClient from server handler. */
      this.m_networkClient = clientInstance;

      /* Server Private key request delegate. */
      this.m_clientRequestServerKey = keyRequester;

      /* ClientId Creation. */
      this.m_clientId = DateTime.Now.Ticks;

      /* Client members initialization. */
      this.m_clientProfile = new ClientProfile();
      this.m_listenThread = new Thread(this.ListenThread);

      Debug.Log("Created client Id={0}.", this.m_clientId);
    }

    public void Start()
    {
      Debug.Log("Starting client threads.");
      this.m_listenThread.Start();
    }

    public ByteString ProceedMessageEncoding(AsymmetricKeyParameter messageKey, ByteString messageBytes)
    {
      Debug.Log("Proceeding with key IsPrivate={0} Message Size={1}.", messageKey.IsPrivate, messageBytes.Length);

      OaepEncoding cipherEncoder = new OaepEncoding(new RsaBlindedEngine(), new Sha256Digest());
      cipherEncoder.Init(!(messageKey.IsPrivate), messageKey);

      using (MemoryStream outputStream = new MemoryStream())
      using (Stream inputStream = new MemoryStream(messageBytes.ToByteArray())) {
        byte[] inputBlock = new byte[cipherEncoder.GetInputBlockSize()];
        int readBytes = 0;

        while ((readBytes = inputStream.Read(inputBlock, 0, inputBlock.Length)) > 0) {
          byte[] outputBlock = cipherEncoder.ProcessBlock(inputBlock, 0, readBytes);
          outputStream.Write(outputBlock, 0, outputBlock.Length);

          if (readBytes != inputBlock.Length)
            break;
        }
        return ByteString.CopyFrom(outputStream.ToArray(), 0, (int)outputStream.Length);
      }
    }

    public void UpdatePublicKey(PublicKey clientKey)
    {
      this.m_clientPublicKey = new RsaKeyParameters(
        modulus: new BigInteger(clientKey.Modulus.ToByteArray()),
        exponent: new BigInteger(clientKey.Exponent.ToByteArray()),
        isPrivate: false
      );
      Debug.Log("Updated client encryption key.");
    }

    public bool SendMessage(int type, IMessage message)
    {
      return this.SendMessage(type, message, true);
    }

    public bool SendMessage(int type, IMessage message, bool encrypt)
    {
      Debug.Log("Sending server message Type={0} Encrypted={1}.", type, encrypt);

      ByteString messageContent = message.ToByteString();
      NetworkPacket networkPacket = new NetworkPacket();
      networkPacket.Type = type;
      networkPacket.Server = true;
      networkPacket.Encrypted = encrypt;

      if (networkPacket.Encrypted) {
        Debug.Log("Encrypting message content. Size={0}", messageContent.Length);

        messageContent = this.ProceedMessageEncoding(this.m_clientPublicKey, messageContent);
        if (messageContent == null) {
          Debug.Error("Encoded message content is invalid.");
          return false;
        }
      }
      networkPacket.Message = messageContent;
      return this.SendMessage(networkPacket);
    }

    public bool ForwardNetworkPacket(long senderClientId, NetworkPacket packet)
    {
      Debug.Log("Sending message from {0}, Type={1:X2}.", senderClientId, packet.Type);

      NetworkPacket forwardPacket = new NetworkPacket();
      forwardPacket.Server = false;
      forwardPacket.ClientId = senderClientId;

      forwardPacket.Encrypted = packet.Encrypted;
      forwardPacket.Message = packet.Message;
      forwardPacket.Type = packet.Type;

      return this.SendMessage(forwardPacket);
    }

    private bool SendMessage(NetworkPacket packet)
    {
      int sendBytes = this.m_networkClient.Client.Send(packet.ToByteArray());
      Debug.Log("Send {0} bytes. Packet size: {1} bytes.", sendBytes, packet.CalculateSize());
      return (sendBytes == packet.CalculateSize());
    }
    
    private void ListenThread()
    {
      Debug.Log("Starting listening for client.");
      byte[] messageBuffer = new byte[LISTEN_MESSAGE_SIZE];
      int bytesReceived = 0;

      while ((bytesReceived = this.m_networkClient.Client.Receive(messageBuffer)) != 0) {
        NetworkPacket receivedMessage = NetworkPacket.Parser.ParseFrom(ByteString.CopyFrom(messageBuffer, 0, bytesReceived));
        Debug.Log("Received message Server={0} ClientId={1} Encrypted={2}.", receivedMessage.Server, 
          receivedMessage.ClientId, receivedMessage.Encrypted);

        if (receivedMessage.Server) {
          ByteString messageContent = receivedMessage.Message;

          if (receivedMessage.Encrypted) {
            Debug.Log("Decrypting server message.");

            AsymmetricKeyParameter cipherPrivateKey = this.m_clientRequestServerKey?.Invoke();
            if (cipherPrivateKey == null) {
              Debug.Error("Cannot receive server private cipher key.");
              continue;
            }

            messageContent = this.ProceedMessageEncoding(cipherPrivateKey, messageContent);
            if (messageContent == null) {
              Debug.Error("Invalid message content after encoding.");
              continue;
            }
          }
          this.onServerMessageRequest?.Invoke(this, receivedMessage.Type, messageContent);
        }
        else {
          this.onClientMessageRequest?.Invoke(this, receivedMessage);
        }
      }
    }

  }
}
