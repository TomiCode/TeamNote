using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;

using Google.Protobuf;

using TeamNote.Protocol;

namespace TeamNote.Server
{
  class ServerInstance
  {
    public const int KEY_STRENGTH = 1024;

    /* Server services. */
    private Configuration m_serverConfig;
    private DiscoveryService m_discoveryService;

    /* Connected clients. */
    private List<NetworkClient> m_connectedClients;

    /* Internal members. */
    private TcpListener m_serverListener;
    private Thread m_serverThread;

    /* Pgp Server Keypair. */
    private AsymmetricCipherKeyPair m_serverCipherKeys;

    public PublicKey ServerKey {
      get {
        PublicKey resultKey = new PublicKey();
        RsaKeyParameters serverPublic = this.m_serverCipherKeys.Public as RsaKeyParameters;
        if (serverPublic == null) {
          Debug.Warn("Cannot create server PublicKey request.");
          return null;
        }

        resultKey.Modulus = ByteString.CopyFrom(serverPublic.Modulus.ToByteArray());
        resultKey.Exponent = ByteString.CopyFrom(serverPublic.Exponent.ToByteArray());
        return resultKey;
      }
    }

    public ServerInstance()
    {
      /* Configuration loader. */
      this.m_serverConfig = new Configuration("ServerConfig.json");
      
      if (!this.m_serverConfig.LoadConfig()) {
        this.m_serverConfig.CreateDefaults();
        if (!this.m_serverConfig.SaveConfig()) {
          Debug.Error("Can not create new configuration file. Loaded={0}", this.m_serverConfig.ConfigLoaded);
        }
      }

      /* Current connected clients. */
      this.m_connectedClients = new List<NetworkClient>();

      /* UDP Server discovery service. */
      this.m_discoveryService = new DiscoveryService(this.m_serverConfig.ConfigService);

      /* TCP/IP Listener. */
      this.m_serverListener = new TcpListener(this.m_serverConfig.ListenAddress);

      /* Thread initialization. */
      this.m_serverThread = new Thread(this.ServerListener);
    }

    public void Start()
    {
      Debug.Log("Starting socket listeners. [{0} => {1}]", 
        this.m_serverConfig.ConfigService, this.m_serverConfig.ListenAddress);

      /* Start listeners and threads. */
      this.m_discoveryService.Start(this.m_serverConfig.ListenAddress);
      this.m_serverListener.Start();
      this.m_serverThread.Start();
    }

    public void GenerateServerKeypair()
    {
      RsaKeyPairGenerator keypairGenerator = new RsaKeyPairGenerator();
      keypairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), KEY_STRENGTH));
      this.m_serverCipherKeys = keypairGenerator.GenerateKeyPair();

      Debug.Log("Keypair created.");
    }

    private void ServerListener()
    {
      while (this.m_serverListener != null) {
        TcpClient tcpClient = this.m_serverListener.AcceptTcpClient();
        Debug.Log("New client connection.");

        NetworkClient networkClient = new NetworkClient(tcpClient, this.ClientKeyRequester);
        networkClient.onServerMessageRequest += this.ServerMessageReceived;
        networkClient.onClientMessageRequest += this.ClientMessageReceived;
        networkClient.onClientDisconnected += this.ClientDisconnectHandler;

        this.m_connectedClients.Add(networkClient);
        networkClient.Start();
      }
    }

    private NetworkClient GetClientFromId(long clientId)
    {
      Debug.Log("Requesting ClientId={0}.", clientId);
      foreach (NetworkClient connectedClient in this.m_connectedClients) {
        if (connectedClient.ClientId == clientId)
          return connectedClient;
      }

      Debug.Warn("ClientId={0} does not exists.", clientId);
      return null;
    }

    private AsymmetricKeyParameter ClientKeyRequester()
    {
      Debug.Log("Requesting server private key.");
      return this.m_serverCipherKeys.Private;
    }

    private void ServerMessageReceived(NetworkClient senderClient, int messageType, ByteString messageContent)
    {
      Debug.Log("Server message Type={0} Size={1}.", messageType, messageContent.Length);

      switch (messageType) {
        /* Handshake request. */
        case MessageType.ClientHandshakeRequest: {
            HandshakeRequest request = HandshakeRequest.Parser.ParseFrom(messageContent);
            senderClient.UpdatePublicKey(request.Key);

            PublicKey serverPublic = this.ServerKey;
            if (serverPublic == null) {
              Debug.Error("Server public key is invalid.");
              return;
            }

            HandshakeResponse response = new HandshakeResponse();
            response.Key = serverPublic;
            senderClient.SendMessage(MessageType.ClientHandshakeResponse, response);
          }
          break;

        /* Authorization request. */
        case MessageType.AuthorizationRequest: {
            AuthorizationRequest request = AuthorizationRequest.Parser.ParseFrom(messageContent);
            senderClient.Profile.UpdateProfile(request);

            AuthorizationResponse response = new AuthorizationResponse();
            response.ServerName = this.m_serverConfig.ServerName;
            senderClient.SendMessage(MessageType.AuthorizationResponse, response);

            Task.Run(() => {
              ContactUpdate contactUpdate = new ContactUpdate();
              contactUpdate.Add.Add(senderClient.ContactClient);

              foreach (NetworkClient connectedClient in this.m_connectedClients) {
                if (connectedClient.ClientId == senderClient.ClientId) continue;

                Debug.Log("Sending contact update to ClientId={0}.", connectedClient.ClientId);
                connectedClient.SendMessage(MessageType.ContactUpdate, contactUpdate);
              }
            });
          }
          break;

        /* Contact list update. */
        case MessageType.ContactUpdateRequest: {
            ContactUpdateRequest request = ContactUpdateRequest.Parser.ParseFrom(messageContent);
            ContactUpdate contactUpdate = new ContactUpdate();

            List<long> clientList = request.Clients.ToList();
            foreach (NetworkClient connectedClient in this.m_connectedClients) {
              // if (connectedClient.ClientId == senderClient.ClientId) continue;
              if (!connectedClient.Profile.Valid) continue;

              if (clientList.Contains(connectedClient.ClientId)) {
                Debug.Log("Client {0} has got ClientId={1}.", senderClient.ClientId, connectedClient.ClientId);
                clientList.Remove(connectedClient.ClientId);
              }
              else {
                Debug.Log("Client {0} requires ClientId={1}.", senderClient.ClientId, connectedClient.ClientId);
                contactUpdate.Add.Add(connectedClient.ContactClient);
              }
            }

            Debug.Log("Trash clients count={0}.", clientList.Count);
            if (clientList.Count > 0) {
              contactUpdate.Remove.Add(clientList);
            }
            senderClient.SendMessage(MessageType.ContactUpdate, contactUpdate);
          }
          break;

        /* Client Public Key request. */
        case MessageType.MessageClientPublicRequest: {
            MessageRequestClientPublic requestMessage = MessageRequestClientPublic.Parser.ParseFrom(messageContent);
            PublicKey responseKey = null;

            foreach (NetworkClient connectedClient in this.m_connectedClients) {
              if (connectedClient.ClientId == requestMessage.ClientId) {
                responseKey = connectedClient.ClientPublic;
              }
            }

            if (responseKey != null) {
              MessageResponseClientPublic responseMessage = new MessageResponseClientPublic();
              responseMessage.ClientId = requestMessage.ClientId;
              responseMessage.Key = responseKey;

              senderClient.SendMessage(MessageType.MessageClientPublicResponse, responseMessage);
            }
            else 
              Debug.Warn("ClientId={0} has invalid public key.");
          }
          break;
      }
    }

    private void ClientMessageReceived(NetworkClient senderClient, NetworkPacket receivedPacket)
    {
      NetworkClient requestedUser = this.GetClientFromId(receivedPacket.ClientId);
      if (requestedUser == null) {
        Debug.Warn("Invalid ClientId={0} requested from ClientId={1}.", receivedPacket.ClientId, senderClient.ClientId);
        return;
      }

      Debug.Log("Forwarding message from ClientId={0} to ClientId={1}.", senderClient.ClientId, receivedPacket.ClientId);
      requestedUser.ForwardNetworkPacket(senderClient.ClientId, receivedPacket);
    }

    private void ClientDisconnectHandler(NetworkClient senderClient)
    {
      this.m_connectedClients.Remove(senderClient);
      Debug.Log("Current connected clients Count={0}.", this.m_connectedClients.Count);

      ContactUpdate removeUpdate = new ContactUpdate();
      removeUpdate.Remove.Add(senderClient.ClientId);

      foreach (NetworkClient connectedClient in this.m_connectedClients) {
        Debug.Log("Sending UpdateContact to remove Client from ClientId={0}.", connectedClient.ClientId);
        connectedClient.SendMessage(MessageType.ContactUpdate, removeUpdate);
      }
    }
  }
}
