using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf;

using Org.BouncyCastle.Bcpg.OpenPgp;

using TeamNote.Protocol;

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Bcpg;
using System.IO;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities.IO;

namespace TeamNote.Server
{
  class ServerInstance
  {
    public const int KEY_STRENGTH = 1024;

    /* Server services. */
    private Configuration m_serverConfig;
    private DiscoveryService m_discoveryService;

    /* Connected clients. */
    private Dictionary<long, NetworkClient> m_connectedClients;
    
    /* Internal members. */
    private TcpListener m_serverListener;
    private Thread m_serverThread;

    /* Pgp Server Keypair. */
    private PgpKeyPair m_serverKeyPair;
    private PgpPublicKey m_serverPublicKey;
    private PgpPrivateKey m_serverPrivateKey;

    public ServerInstance()
    {
      this.m_serverConfig = new Configuration("ServerConfig.json");
      
      if (!this.m_serverConfig.LoadConfig()) {
        this.m_serverConfig.CreateDefaults();
        if (!this.m_serverConfig.SaveConfig()) {
          Debug.Error("Can not create new configuration file. Loaded={0}", this.m_serverConfig.ConfigLoaded);
        }
      }

      /* Current connected clients. */
      this.m_connectedClients = new Dictionary<long, NetworkClient>();

      /* UDP Server discovery service. */
      this.m_discoveryService = new DiscoveryService(this.m_serverConfig.ConfigService);

      /* TCP/IP Listener. */
      this.m_serverListener = new TcpListener(this.m_serverConfig.ListenAddress);

      /* Thread initialization. */
      this.m_serverThread = new Thread(this.ServerListener);
    }

    public void Start()
    {
      Debug.Log("Starting socket listeners. [{0} => {1}]", this.m_serverConfig.ConfigService, this.m_serverConfig.ListenAddress);
      this.m_discoveryService.Start(this.m_serverConfig.ListenAddress);
      this.m_serverListener.Start();
      this.m_serverThread.Start();


      AuthorizationRequest request = new AuthorizationRequest();
      request.Name = "test";
      byte[] messageBytes = request.ToByteArray();
      Debug.Log("Message size={0}.", messageBytes.Length);

      MemoryStream bOut = new MemoryStream();
      PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Uncompressed);
      Stream cos = comData.Open(bOut);
      PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();

      Stream pOut = lData.Open(cos, PgpLiteralData.Binary, PgpLiteralData.Console, messageBytes.Length, DateTime.UtcNow);
      pOut.Write(messageBytes, 0, messageBytes.Length);
      pOut.Close();
      comData.Close();
      messageBytes = bOut.ToArray();
      Debug.Log("Compressed message size={0}.", messageBytes.Length);

      PgpEncryptedDataGenerator l_dataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, new SecureRandom());
      l_dataGenerator.AddMethod(this.m_serverKeyPair.PublicKey);

      MemoryStream l_byteStream = new MemoryStream();
      Stream l_dataStream = l_dataGenerator.Open(l_byteStream, messageBytes.Length);
      l_byteStream.Write(messageBytes, 0, messageBytes.Length);
      l_byteStream.Close();

      messageBytes = l_byteStream.ToArray();
      Debug.Log("Encoded message size={0}.", messageBytes.Length);

      Stream inputStream = new MemoryStream(messageBytes);
      inputStream = PgpUtilities.GetDecoderStream(inputStream);
      MemoryStream decoded = new MemoryStream();

      PgpObjectFactory pgpF = new PgpObjectFactory(inputStream);
      PgpEncryptedDataList enc = null;
      PgpObject o = pgpF.NextPgpObject();

      if (o is PgpEncryptedDataList) {
        enc = (PgpEncryptedDataList)o;
      }
      else {
        enc = (PgpEncryptedDataList)pgpF.NextPgpObject();
      }
      
      PgpPublicKeyEncryptedData pbe = enc.GetEncryptedDataObjects().Cast<PgpPublicKeyEncryptedData>().First();
      // Debug.Log("KeyId={0} KeyId={1}.", pbe.KeyId, this.m_serverPrivateKey.KeyId);

      Stream clear = pbe.GetDataStream(this.m_serverKeyPair.PrivateKey);
      PgpObjectFactory plainFact = new PgpObjectFactory(clear);
      PgpObject message = plainFact.NextPgpObject();

      if (message is PgpCompressedData) {
        PgpCompressedData cData = (PgpCompressedData)message;
        PgpObjectFactory pgpFact = new PgpObjectFactory(cData.GetDataStream());
        message = pgpFact.NextPgpObject();
      }
      if (message is PgpLiteralData) {
        PgpLiteralData ld = (PgpLiteralData)message;
        Stream unc = ld.GetInputStream();
        Streams.PipeAll(unc, decoded);
      }

    }

    public void GenerateServerKeypair()
    {
      RsaKeyPairGenerator keypairGenerator = new RsaKeyPairGenerator();
      keypairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), KEY_STRENGTH));

      AsymmetricCipherKeyPair l_keypair = keypairGenerator.GenerateKeyPair();
      this.m_serverKeyPair = new PgpKeyPair(PublicKeyAlgorithmTag.RsaEncrypt, l_keypair, DateTime.Now);
      // this.m_serverPrivateKey = pair.
      // this.m_serverPublicKey = new PgpPublicKey(PublicKeyAlgorithmTag.RsaEncrypt, l_keypair.Public, DateTime.UtcNow);
      // this.m_serverPrivateKey = new PgpPrivateKey(this.m_serverPublicKey.KeyId, this.m_serverPublicKey.PublicKeyPacket, l_keypair.Private);

      Debug.Log("Keypair created.");
    }

    private void ServerListener()
    {
      while (this.m_serverListener != null) {
        TcpClient l_tcpClient = this.m_serverListener.AcceptTcpClient();
        Debug.Log("New client connection accepted.");

        NetworkClient l_networkClient = new NetworkClient(l_tcpClient);
        l_networkClient.onClientMessage += this.ClientMessageReceived;

        this.m_connectedClients.Add(DateTime.Now.Ticks, l_networkClient);
        l_networkClient.Start();
      }
    }

    private ByteString DecryptServerMessage(ByteString message)
    {
      Debug.Log("Private KeyId={0}.", this.m_serverPrivateKey.KeyId);

      Stream inputStream = new MemoryStream(message.ToByteArray());

      inputStream = PgpUtilities.GetDecoderStream(inputStream);

      PgpObjectFactory pgpF = new PgpObjectFactory(inputStream);
      PgpEncryptedDataList enc = null;
      PgpObject o = pgpF.NextPgpObject();

      //
      // the first object might be a PGP marker packet.
      //
      if (o is PgpEncryptedDataList) {
        enc = (PgpEncryptedDataList)o;
      }
      else {
        enc = (PgpEncryptedDataList)pgpF.NextPgpObject();
      }

      PgpPublicKeyEncryptedData publicEncrypted = enc[0] as PgpPublicKeyEncryptedData;
      Debug.Log("KeyId={0} Server Private KeyId={1}.", publicEncrypted.KeyId, this.m_serverPrivateKey.KeyId);
      BcpgInputStream clear = publicEncrypted.GetDataStream(this.m_serverPrivateKey) as BcpgInputStream;

      //PgpPbeEncryptedData pbe = (PgpPbeEncryptedData)enc[0];
      //Stream clear = pbe.GetDataStream();

      PgpObjectFactory pgpFact = new PgpObjectFactory(clear);
      // Debug.Log("Object count: {0}.", pgpFact.AllPgpObjects().Count);

      // PgpCompressedData cData = (PgpCompressedData) pgpFact.NextPgpObject();
      //pgpFact = new PgpObjectFactory(cData.GetDataStream());

      PgpLiteralData ld = (PgpLiteralData) pgpFact.NextPgpObject();
      Stream unc = ld.GetInputStream();

      Debug.Log("Type={0}", unc.ToString());

      byte[] test = new byte[512];
      int count = unc.Read(test, 0, 512);
      Debug.Log("Read {0} bytes.", count);

      return null;
    }

    private void ClientMessageReceived(NetworkClient senderClient, NetworkPacket receivedPacket)
    {
      Debug.Log("NetworkPacket Type={0:X2} Server={1} Encrypted={2} ClientId={3} Message Size={4}.", 
        receivedPacket.Type, receivedPacket.Server, receivedPacket.Encrypted, receivedPacket.ClientId, receivedPacket.Message.Length);

      ByteString l_receivedMessage = receivedPacket.Message;
      if (receivedPacket.Server && receivedPacket.Encrypted) {
        l_receivedMessage = this.DecryptServerMessage(l_receivedMessage);
        if (l_receivedMessage == null) {
          Debug.Warn("Error occured while message encryption.");
          return;
        }
      }

      if (receivedPacket.Type == MessageType.ClientHandshakeRequest) {
        HandshakeRequest clientRequest = HandshakeRequest.Parser.ParseFrom(l_receivedMessage);
        senderClient.UpdatePublicKey(clientRequest.Key);

        PublicKey serverPublicKey = new PublicKey();
        RsaKeyParameters publicKeyParams = this.m_serverPublicKey.GetKey() as RsaKeyParameters;

        serverPublicKey.Modulus = Convert.ToBase64String(publicKeyParams.Modulus.ToByteArray());
        serverPublicKey.Exponent = Convert.ToBase64String(publicKeyParams.Exponent.ToByteArray());

        HandshakeResponse serverResponse = new HandshakeResponse();
        serverResponse.Key = serverPublicKey;

        senderClient.SendMessage(MessageType.ClientHandshakeResponse, serverResponse, false);
      }
      else if (receivedPacket.Type == MessageType.AuthorizationRequest) {
        AuthorizationRequest authRequest = AuthorizationRequest.Parser.ParseFrom(l_receivedMessage);
        senderClient.Profile.UpdateProfile(authRequest);

        /* Response. */
      }


    }

  }
}
