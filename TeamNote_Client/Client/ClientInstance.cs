using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Net.Sockets;
using System.Net;

using TeamNote.Protocol;

using Google.Protobuf;

namespace TeamNote.Client
{
  class ClientInstance
  {
    public const string CONFIG_FILENAME = "ClientConfig.json";

    /* Client private members. */
    private Configuration m_clientConfig;
    private ServerDiscoverer m_serverDiscoverer;

    private LocalClient m_localClient;

    /* Client GUI types. */
    private GUI.Splash m_guiSplash;
    private GUI.Authenticate m_guiAuthenticate;

    public ClientInstance()
    {
      /* Client config. */
      this.m_clientConfig = new Configuration(CONFIG_FILENAME);

      /* Server Discoverer. */
      this.m_serverDiscoverer = new ServerDiscoverer();
      this.m_serverDiscoverer.onDiscoveryResponse += this.ConnectToServer;

      /* Self object for TCPIP Handling. */
      this.m_localClient = new LocalClient();
      this.m_localClient.onServerMessageReceived += this.ReceivedServerMessage;
      this.m_localClient.onClientMessageReceived += this.ReceivedClientMessage;

      /* GUI initialization. */
      this.m_guiSplash = new GUI.Splash();

      this.m_guiAuthenticate = new GUI.Authenticate();
      this.m_guiAuthenticate.onAuthorizationAccept += this.SendAuthorization;
    }

    public void Initialize()
    {
      if (!this.m_clientConfig.LoadConfig()) {
        Debug.Log("Failed to load configuration file.. Creating.");

        this.m_clientConfig.CreateDefaults();
        if (!this.m_clientConfig.SaveConfig()) {
          Debug.Warn("Error occured while saving configuration file. Config Fields={0}", this.m_clientConfig.ConfigLoaded);
        }
      }
      this.m_localClient.InitializeKeypair();

      this.m_guiSplash.Show();
      this.m_serverDiscoverer.Start(this.m_clientConfig.UDP_Port);

      this.UpdateStatusMessage("Splash_Discover");
    }

    private void ConnectToServer(IPEndPoint serverAddress)
    {
      Debug.Log("Address: {0}", serverAddress);
      this.m_guiSplash.SetMessage("Splash_Connect");
      /* Delay. */

      if (this.m_localClient.Connect(serverAddress)) {
        Debug.Log("Connected to server!");
        /* Delay. */
        this.m_localClient.SendHandshake();
      }
    }

    private void SendAuthorization(string name, string surname)
    {
      AuthorizationRequest request = new AuthorizationRequest();
      request.Name = name;
      request.Surname = surname;

      Debug.Log("Sending authorization to server [{0} {1}].", name, surname);
      this.m_localClient.SendMessage(MessageType.AuthorizationRequest, request);
    }

    private void UpdateStatusMessage(string resourceString)
    {
      this.m_guiSplash.SetMessage(resourceString);
    }

    private void ReceivedServerMessage(int type, ByteString message)
    {
      Debug.Log("Received from server, message Type={0:X8} Size={1}.", type, message.Length);

      if (type == MessageType.ClientHandshakeResponse) {
        HandshakeResponse responseMessage = HandshakeResponse.Parser.ParseFrom(message);
        this.m_localClient.UpdateServerKey(responseMessage.Key);

        this.m_guiSplash.Dispatcher.Invoke(() => {
          this.m_guiSplash.Hide();
          this.m_guiAuthenticate.Show();
        });
      }
    }

    private void ReceivedClientMessage(long senderClientId, int messageType, ByteString messageContent)
    {

    }

  }
}