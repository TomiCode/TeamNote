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
    }

    public void GenerateServerKeypair()
    {
      RsaKeyPairGenerator keypairGenerator = new RsaKeyPairGenerator();
      keypairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), KEY_STRENGTH));

      AsymmetricCipherKeyPair l_keypair = keypairGenerator.GenerateKeyPair();
      this.m_serverPublicKey = new PgpPublicKey(PublicKeyAlgorithmTag.RsaGeneral, l_keypair.Public, DateTime.Now);
      this.m_serverPrivateKey = new PgpPrivateKey(this.m_serverPublicKey.KeyId, this.m_serverPublicKey.PublicKeyPacket, l_keypair.Private);

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

    private void ClientMessageReceived(int messageType, ByteString messageBytes, NetworkClient senderClient)
    {
      Debug.Log("Handling message Type={0:X8} Size={1}.", messageType, messageBytes.Length);
      if (messageType == MessageType.ClientHandshakeRequest) {
        HandshakeRequest clientRequest = HandshakeRequest.Parser.ParseFrom(messageBytes);
        senderClient.UpdatePublicKey(clientRequest.Key);

        PublicKey serverPublicKey = new PublicKey();
        RsaKeyParameters publicKeyParams = this.m_serverPublicKey.GetKey() as RsaKeyParameters;

        serverPublicKey.Modulus = Convert.ToBase64String(publicKeyParams.Modulus.ToByteArray());
        serverPublicKey.Exponent = Convert.ToBase64String(publicKeyParams.Exponent.ToByteArray());

        HandshakeResponse serverResponse = new HandshakeResponse();
        serverResponse.Key = serverPublicKey;

        senderClient.SendMessage(MessageType.ClientHandshakeResponse, serverResponse, false);
      }
    }

  }
}
