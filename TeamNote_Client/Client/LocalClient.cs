using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Zlib;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

using Google.Protobuf;

using TeamNote.Protocol;

namespace TeamNote.Client
{
  class LocalClient
  {
    /* Constants. */
    const int KEY_STRENGTH = 1024;
    const int LISTEN_BUFFER_SIZE = 4096;

    /* Client events and delegates. */
    public delegate void ServerMessageReceivedHandler(int type, ByteString message);
    public delegate void ClientMessageReceivedHandler(long sender, int type, ByteString message);

    public event ServerMessageReceivedHandler onServerMessageReceived;
    public event ClientMessageReceivedHandler onClientMessageReceived;

    /* Local Keypair. */
    private AsymmetricCipherKeyPair m_localCipherKeys;

    /* Server public key. */
    private AsymmetricKeyParameter m_serverKey;

    /* Connected client keyring. */
    private Dictionary<long, AsymmetricKeyParameter> m_localKeyring;
    
    /* Private members. */
    private TcpClient m_tcpClient;
    private Thread m_listenThread;

    /* Properties. */
    public bool IsConnected {
      get {
        return this.m_tcpClient.Connected;
      }
    }

    private PublicKey ServerKey {
      get {
        PublicKey serverKey = new PublicKey();
        RsaKeyParameters publicKey = this.m_localCipherKeys.Public as RsaKeyParameters;
        if (publicKey == null) {
          Debug.Warn("Invalid server public key.");
          return null;
        }

        serverKey.Exponent = ByteString.CopyFrom(publicKey.Exponent.ToByteArray());
        serverKey.Modulus = ByteString.CopyFrom(publicKey.Modulus.ToByteArray());
        return serverKey;
      }
    }

    /* Public methods. */
    public LocalClient()
    {
      this.m_localKeyring = new Dictionary<long, AsymmetricKeyParameter>();

      this.m_tcpClient = new TcpClient();
      this.m_listenThread = new Thread(this.ListenThread);
    }

    public bool Connect(IPEndPoint serverAddr)
    {
      Debug.Log("Connecting to {0}.", serverAddr);

      try {
        this.m_tcpClient.Connect(serverAddr);
      }
      catch (Exception ex) {
        Debug.Exception(ex);
        return false;
      }

      this.m_listenThread.Start();
      return true;
    }

    public void InitializeKeypair()
    {
      RsaKeyPairGenerator keyGenerator = new RsaKeyPairGenerator();
      keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), KEY_STRENGTH));

      this.m_localCipherKeys = keyGenerator.GenerateKeyPair();

      RsaKeyParameters localKeyParameters = this.m_localCipherKeys.Public as RsaKeyParameters;
      Debug.Log("Generated Public key: E={0}, M={1}.",
        Convert.ToBase64String(localKeyParameters.Exponent.ToByteArray()),
        Convert.ToBase64String(localKeyParameters.Modulus.ToByteArray())
      );
    }

    public void SendHandshake()
    {
      Debug.Log("Sending handshake to server.");

      HandshakeRequest handshakeRequest = new HandshakeRequest();
      PublicKey publicKey = this.ServerKey;
      handshakeRequest.Key = publicKey;

      this.SendMessage(MessageType.ClientHandshakeRequest, handshakeRequest, false);
    }

    public void UpdateServerKey(PublicKey serverKey)
    {
      this.m_serverKey = new RsaKeyParameters(
        isPrivate: false,
        exponent: new BigInteger(serverKey.Exponent.ToByteArray()),
        modulus: new BigInteger(serverKey.Modulus.ToByteArray())
      );
      Debug.Log("Server public key updated.");
    }

    public void RegisterClientKey(long client, PublicKey key)
    {
      Debug.Log("Adding ClientId={0} to keyring.", client);
      RsaKeyParameters l_keyParam = new RsaKeyParameters(
        isPrivate: false,
        exponent: new BigInteger(key.Exponent.ToByteArray()),
        modulus: new BigInteger(key.Modulus.ToByteArray())
      );

      this.m_localKeyring.Add(client, l_keyParam);
    }

    public bool RemoveClientKey(long client)
    {
      Debug.Log("Removing ClientId={0} from keyring.", client);
      return this.m_localKeyring.Remove(client);
    }

    public bool SendMessage(int type, IMessage message)
    {
      return this.SendMessage(0, true, type, message, true);
    }

    public bool SendMessage(long client, int type, IMessage message)
    {
      return this.SendMessage(client, false, type, message, true);
    }

    private bool SendMessage(int type, IMessage message, bool encrypt)
    {
      return this.SendMessage(0, true, type, message, encrypt);
    }

    private bool SendMessage(long client, int type, IMessage message, bool encrypt)
    {
      return this.SendMessage(client, false, type, message, encrypt);
    }

    private bool SendMessage(long client, bool server, int type, IMessage message, bool encrypt)
    {
      ByteString messageContent = message.ToByteString();
      NetworkPacket networkPacket = new NetworkPacket();

      networkPacket.Type = type;
      networkPacket.Server = server;
      networkPacket.ClientId = client;
      networkPacket.Encrypted = encrypt;
      
      if (networkPacket.Encrypted) {
        AsymmetricKeyParameter localKeyParameter = null;

        if (networkPacket.Server) localKeyParameter = this.m_serverKey;
        else localKeyParameter = this.m_localKeyring[client];

        if (localKeyParameter == null) {
          Debug.Error("Invalid key for Send. ClientId={0} Server={1}.", client, server);
          return false;
        }

        messageContent = this.ProceedMessageEncoding(localKeyParameter, messageContent);
        if (messageContent == null) {
          Debug.Error("Processed message is invalid.");
          return false;
        }
      }
      networkPacket.Message = messageContent;

      int networkPacketSize = networkPacket.CalculateSize();
      int bytesSend = this.m_tcpClient.Client.Send(networkPacket.ToByteArray());

      Debug.Log("Sended {0} bytes. Network packet Size={1}.", bytesSend, networkPacketSize);
      return (bytesSend == networkPacketSize);
    }

    private ByteString ProceedMessageEncoding(AsymmetricKeyParameter messageKey, ByteString messageBytes)
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

    private void ListenThread()
    {
      Debug.Log("Started listening for server responses.");
      byte[] messageBuffer = new byte[LISTEN_BUFFER_SIZE];
      int bytesReceived = 0;

      while ((bytesReceived = this.m_tcpClient.Client.Receive(messageBuffer)) != 0) {
        NetworkPacket receivedPacket = NetworkPacket.Parser.ParseFrom(new CodedInputStream(messageBuffer, 0, bytesReceived));
        Debug.Log("Received packet Type={0} Server={1} ClientId={2} Encrypted={3}.",
          receivedPacket.Type, receivedPacket.Server, receivedPacket.ClientId, receivedPacket.Encrypted);

        ByteString messageContent = receivedPacket.Message;
        if (receivedPacket.Encrypted) {
          messageContent = this.ProceedMessageEncoding(this.m_localCipherKeys.Private, messageContent);
          if (messageContent == null) {
            Debug.Error("Encoded message content is invalid.");
            continue;
          }
        }

        if (receivedPacket.Server) {
          this.onServerMessageReceived?.Invoke(receivedPacket.Type, messageContent);
        }
        else {
          this.onClientMessageReceived?.Invoke(receivedPacket.ClientId, receivedPacket.Type, messageContent);
        }
      }
    }

  }
}
