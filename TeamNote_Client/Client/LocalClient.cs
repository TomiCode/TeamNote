using System;
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
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

using Google.Protobuf;

using TeamNote.Protocol;
using System.IO;

namespace TeamNote.Client
{
  class LocalClient
  {
    /* Constants. */
    const int KEY_STRENGTH = 1024;

    /* Client events and delegates. */
    public delegate void MessageReceivedEvent(int type, ByteString message);
    public event MessageReceivedEvent onMessageReceived;

    /* Local pgp-keys. */
    private PgpPublicKey m_localPublicKey;
    private PgpPrivateKey m_localPrivateKey;

    /* Server public key. */
    private PgpPublicKey m_serverKey;

    private Dictionary<long, PgpPublicKey> m_localKeyring;
    
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
        PublicKey key = new PublicKey();

        /* Add exception handling. */
        RsaKeyParameters keyParams = this.m_localPublicKey.GetKey() as RsaKeyParameters;
        
        key.Exponent = Convert.ToBase64String(keyParams.Exponent.ToByteArray());
        key.Modulus = Convert.ToBase64String(keyParams.Modulus.ToByteArray());

        return key;
      }
    }

    /* Public methods. */
    public LocalClient()
    {
      this.m_localKeyring = new Dictionary<long, PgpPublicKey>();

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
      Debug.Log("Starting client key generation.");
      RsaKeyPairGenerator keyGenerator = new RsaKeyPairGenerator();
      keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), KEY_STRENGTH));

      AsymmetricCipherKeyPair localKeyPair = keyGenerator.GenerateKeyPair();
      this.m_localPublicKey = new PgpPublicKey(PublicKeyAlgorithmTag.RsaGeneral, localKeyPair.Public, DateTime.Now);
      this.m_localPrivateKey = new PgpPrivateKey(this.m_localPublicKey.KeyId, this.m_localPublicKey.PublicKeyPacket, localKeyPair.Private);

      RsaKeyParameters localKeyParameters = this.m_localPublicKey.GetKey() as RsaKeyParameters;
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
      byte[] modulus = Convert.FromBase64String(serverKey.Modulus);
      byte[] exponent = Convert.FromBase64String(serverKey.Exponent);

      RsaKeyParameters publicKeyParams = new RsaKeyParameters(false, new BigInteger(modulus), new BigInteger(exponent));
      this.m_serverKey = new PgpPublicKey(PublicKeyAlgorithmTag.RsaGeneral, publicKeyParams, DateTime.FromBinary(-8587183191550527950));

      Debug.Log("Updated server public key.");
    }

    public void RegisterClientKey(long client, PublicKey key)
    {
      Debug.Log("Adding ClientId={0} to keyring.", client);
      RsaKeyParameters l_keyParam = new RsaKeyParameters(
        isPrivate: false,
        exponent: new BigInteger(Convert.FromBase64String(key.Exponent)),
        modulus: new BigInteger(Convert.FromBase64String(key.Modulus))
      );
      
      PgpPublicKey l_clientKey = new PgpPublicKey(PublicKeyAlgorithmTag.RsaGeneral, l_keyParam, DateTime.Now);
      this.m_localKeyring.Add(client, l_clientKey);
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
      ByteString l_messageBytes = message.ToByteString();
      NetworkPacket l_networkPacket = new NetworkPacket();
      l_networkPacket.Type = type;
      l_networkPacket.Server = server;
      if (!server)
        l_networkPacket.ClientId = client;
      
      if (encrypt) {
        l_networkPacket.Encrypted = true;
        l_messageBytes = this.EncryptMessage(client, server, l_messageBytes);

        if (l_messageBytes == null) {
          Debug.Error("Invalid message bytes.");
          return false;
        }
      }
      l_networkPacket.Message = l_messageBytes;

      int packetSize = l_networkPacket.CalculateSize();
      int sendBytes = this.m_tcpClient.Client.Send(l_networkPacket.ToByteArray());
      Debug.Log("Sended {0} bytes. Network packet Size={1}.", sendBytes, packetSize);

      return (sendBytes == packetSize);
    }

    private ByteString EncryptMessage(long clientId, bool server, ByteString messageBytes)
    {
      Debug.Log("Processing message bytes. ClientId={0} Server={1} Size={2}.", clientId, server, messageBytes.Length);
      PgpPublicKey l_packetKey = null;
      if (server)
        l_packetKey = this.m_serverKey;
      else
        l_packetKey = this.m_localKeyring[clientId];

      if (l_packetKey == null) {
        Debug.Error("Required PGP Public Key does not exist. ClientId={0} Server={1}.", clientId, server);
        return null;
      }

      PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();
      PgpEncryptedDataGenerator l_dataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Blowfish);
      l_dataGenerator.AddMethod(l_packetKey);

      MemoryStream l_byteStream = new MemoryStream();
      Stream l_dataStream = l_dataGenerator.Open(l_byteStream, messageBytes.Length);
      Stream literalData = lData.Open(l_dataStream, PgpLiteralData.Binary, PgpLiteralData.Console, messageBytes.Length, DateTime.UtcNow);
      // l_byteStream.Write(messageBytes.ToByteArray(), 0, messageBytes.Length);
      // l_byteStream.Close();
      literalData.Write(messageBytes.ToByteArray(), 0, messageBytes.Length);
      literalData.Close();

      return ByteString.CopyFrom(l_byteStream.ToArray());
    }

    private ByteString DecryptMessage(ByteString messageBytes)
    {
      MemoryStream memoryStream = new MemoryStream();
      Stream inputStream = new MemoryStream(messageBytes.ToByteArray());
      inputStream = PgpUtilities.GetDecoderStream(inputStream);

      try {
        PgpObjectFactory objectFactory = new PgpObjectFactory(inputStream);
        PgpEncryptedDataList encDataList;
        PgpObject pgpObject = objectFactory.NextPgpObject();

        if (pgpObject is PgpEncryptedDataList)
          encDataList = (PgpEncryptedDataList)pgpObject;
        else
          encDataList = (PgpEncryptedDataList)objectFactory.NextPgpObject();
      }
      catch (Exception ex) {
        Debug.Exception(ex);
        return null;
      }
      return null;
    }

    private void ListenThread()
    {
      Debug.Log("Started listening for server responses.");

      byte[] messageBuffer = new byte[512];
      int bytesReceived = 0;

      while ((bytesReceived = this.m_tcpClient.Client.Receive(messageBuffer)) != 0) {
        Debug.Log("Received message bytes={0}.", bytesReceived);
        NetworkPacket receivedPacket = NetworkPacket.Parser.ParseFrom(new CodedInputStream(messageBuffer, 0, bytesReceived));

        this.onMessageReceived?.Invoke(receivedPacket.Type, receivedPacket.Message);
      }
    }

  }
}
