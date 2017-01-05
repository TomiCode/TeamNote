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
using Google.Protobuf.Collections;

namespace TeamNote.Client
{
  class ClientInstance
  {
    public const string CONFIG_FILENAME = "ClientConfig.json";

    private App.ApplicationCloseDelegate m_appCloseDelegate;

    /* Client private members. */
    private Configuration m_clientConfig;
    private ServerDiscoverer m_serverDiscoverer;

    private LocalClient m_localClient;

    /* Client GUI types. */
    private GUI.Splash m_guiSplash;
    private GUI.Contacts m_guiContacts;
    private GUI.Authenticate m_guiAuthenticate;
    private GUI.ContactInformation m_guiContactInformation;

    private Dictionary<long, GUI.Message> m_guiMessages;

    public ClientInstance(App.ApplicationCloseDelegate appCloseHandle)
    {
      /* Close application handler. */
      this.m_appCloseDelegate = appCloseHandle;

      /* Client config. */
      this.m_clientConfig = new Configuration(CONFIG_FILENAME);

      /* Server Discoverer. */
      this.m_serverDiscoverer = new ServerDiscoverer();
      this.m_serverDiscoverer.onDiscoveryResponse += this.HandleDiscoveryResponse;
      this.m_serverDiscoverer.onDiscoveryFailed += this.HandleDiscoveryFailure;

      /* Self object for TCPIP Handling. */
      this.m_localClient = new LocalClient();
      this.m_localClient.onServerMessageReceived += this.ReceivedServerMessage;
      this.m_localClient.onClientMessageReceived += this.ReceivedClientMessage;
      this.m_localClient.onConnectionErrors += this.HandleConnectionErrors;

      /* GUI initialization. */
      this.m_guiSplash = new GUI.Splash();
      
      this.m_guiAuthenticate = new GUI.Authenticate();
      this.m_guiAuthenticate.onAuthorizationAccept += this.SendAuthorization;
      this.m_guiAuthenticate.onAuthorizationCancel += this.HandleApplicationClose;

      this.m_guiContacts = new GUI.Contacts(this.HandleContactItemButton, this.SendClientDataUpdateChange);
      this.m_guiContacts.onApplicationClose += this.HandleApplicationClose;

      this.m_guiContactInformation = new GUI.ContactInformation();
      this.m_guiMessages = new Dictionary<long, GUI.Message>();
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
      if (!this.m_clientConfig.ConfigLoaded) {
        Debug.Error("Config not loaded. Closing client.");
        this.m_appCloseDelegate(0);
      }
      this.m_localClient.InitializeKeypair();

      this.m_guiSplash.SetMessage("Splash_Discover");
      this.m_guiSplash.Show();

      this.m_serverDiscoverer.Start(this.m_clientConfig.UDP_Port);
    }

    private void HandleDiscoveryResponse(IPEndPoint serverAddress)
    {
      Debug.Log("Address: {0}", serverAddress);
      this.m_guiSplash.SetMessage("Splash_Connect");
      Task.Delay(2000).ContinueWith(_ => this.ConnectToServer(serverAddress));
    }

    private void HandleDiscoveryFailure(int retriesCount)
    {
      Debug.Warn("Discovery had failed after {0} retries. :(", retriesCount);

      this.CloseApplicationAfter(5000);
      this.UpdateStatusMessage("Splash_DiscoverFailure");
    }

    private void ConnectToServer(IPEndPoint serverAddress)
    {
      if (this.m_localClient.Connect(serverAddress)) {
        Debug.Log("Connected to server!");
        if (!this.m_localClient.SendHandshake()) {
          this.m_guiSplash.SetMessage("Splash_HandshakeFail");
          this.CloseApplicationAfter(5000);
        }
      }
      else {
        Debug.Error("Could not connect to remote server {0}.", serverAddress);
        this.m_guiSplash.SetMessage("Splash_CannotConnect");
        this.CloseApplicationAfter(5000);
      }
    }

    private void SendAuthorization(string name, string surname)
    {
      AuthorizationRequest request = new AuthorizationRequest();
      request.Name = name;
      request.Surname = surname;

      Debug.Log("Sending authorization to server [{0} {1}].", name, surname);
      if (this.m_localClient.SendMessage(MessageType.AuthorizationRequest, request)) {
        Task.Delay(500).ContinueWith(_ => this.m_guiContacts.LocalContact.SetUsername(name, surname));
      }
    }

    private void SendContactRequest()
    {
      Debug.Log("Sending request for Contacts update.");
      ContactUpdateRequest request = new ContactUpdateRequest();
      request.Clients.Add(this.m_guiContacts.Clients);

      this.m_localClient.SendMessage(MessageType.ContactUpdateRequest, request);
    }

    private void SendClientDataUpdateChange()
    {
      ContactUpdateChangeRequest clientRequest = this.m_guiContacts.LocalContact.ContactUpdate;
      Debug.Log("Sending update change to server. Online={0} Name={1} Surname={2}.", 
        clientRequest.Online, clientRequest.Name, clientRequest.Surname);

      this.m_localClient.SendMessage(MessageType.ContactUpdateChangeRequest, clientRequest);
    }

    private bool SendClientMessage(long clientId, string messageContent)
    {
      Debug.Log("Sending to ClientId={0} message='{1}'.", clientId, messageContent);
      DirectMessage clientDirectMessage = new DirectMessage();
      clientDirectMessage.Content = messageContent;

      return this.m_localClient.SendMessage(clientId, MessageType.DirectMessage, clientDirectMessage);
    }

    public void RequestClientPublicKey(long clientId)
    {
      Debug.Log("Requesting ClientId={0} public key from server.", clientId);
      MessageRequestClientPublic requestMessage = new MessageRequestClientPublic();
      requestMessage.ClientId = clientId;

      this.m_localClient.SendMessage(MessageType.MessageClientPublicRequest, requestMessage);
    }

    private void UpdateStatusMessage(string resourceString)
    {
      this.m_guiSplash.SetMessage(resourceString);
    }

    private void CloseApplicationAfter(int miliseconds)
    {
      Debug.Log("Closing aplication after {0} ms.", miliseconds);
      Task.Delay(miliseconds).ContinueWith(a => {
        if (this.m_guiSplash.IsVisible) {
          this.m_guiSplash.Dispatcher.Invoke(() => this.m_guiSplash.Hide());
          Task.Delay(2000).ContinueWith(t => this.m_appCloseDelegate(0));
        }
        else this.m_appCloseDelegate(0);
      });
    }

    private void ReceivedServerMessage(int messageType, ByteString messageContent)
    {
      Debug.Log("Received from server, message Type={0:X8} Size={1}.", messageType, messageContent.Length);

      switch (messageType) {
        /* Handshake response. */
        case MessageType.ClientHandshakeResponse: {
            HandshakeResponse response = HandshakeResponse.Parser.ParseFrom(messageContent);

            this.m_localClient.UpdateServerKey(response.Key);
            this.m_guiSplash.Dispatcher.Invoke(() => {
              this.m_guiSplash.Hide();
              this.m_guiAuthenticate.Show();
            });
          }
          break;

        /* Authorization response. */
        case MessageType.AuthorizationResponse: {
            AuthorizationResponse response = AuthorizationResponse.Parser.ParseFrom(messageContent);
            this.m_guiContacts.StatusText = response.ServerName;

            this.m_guiSplash.Dispatcher.Invoke(() => {
              this.m_guiSplash.SetMessage("Splash_Contacts");
              this.m_guiSplash.Show();
            });
            Task.Delay(1000).ContinueWith(_ => { this.SendContactRequest(); });
          }
          break;

        /* Contacts update. */
        case MessageType.ContactUpdate: {
            ContactUpdate contactUpdate = ContactUpdate.Parser.ParseFrom(messageContent);
            foreach (ContactUpdate.Types.Client newContact in contactUpdate.Add) {
              Debug.Log("New contact ClientId={0}.", newContact.ClientId);
              this.m_guiContacts.CreateClient(newContact);
            }
            foreach (long removeClient in contactUpdate.Remove) {
              Debug.Log("Removing ClientId={0}.", removeClient);
              this.m_guiContacts.RemoveClient(removeClient);
              this.m_localClient.RemoveClientKey(removeClient);
            }

            this.m_guiContacts.Dispatcher.Invoke(() => {
              this.m_guiSplash.Hide();
              this.m_guiContacts.Show();
            });
          }
          break;

        /* Client PublicKey response. */
        case MessageType.MessageClientPublicResponse: {
            MessageResponseClientPublic responseMessage = MessageResponseClientPublic.Parser.ParseFrom(messageContent);
            this.m_localClient.RegisterClientKey(responseMessage.ClientId, responseMessage.Key);

            if (this.m_guiMessages.ContainsKey(responseMessage.ClientId)) {
              Debug.Log("Notifying message window, from ClientId={0}.", responseMessage.ClientId);
              GUI.Message guiMessage = this.m_guiMessages[responseMessage.ClientId];
              guiMessage.AddServerMessage("Message_KeyUpdated");
            }
          }
          break;
      }
    }

    private void ReceivedClientMessage(long senderClientId, int messageType, ByteString messageContent)
    {
      Debug.Log("Received client message from ClientId={0}.", senderClientId);
      switch (messageType) {
        case MessageType.DirectMessage: {
            DirectMessage clientMessage = DirectMessage.Parser.ParseFrom(messageContent);
            
            UI.ContactItem.Contact senderContact = this.m_guiContacts.GetClientContact(senderClientId);
            if (senderContact == null) {
              Debug.Error("Cannot proceed invalid message contact.");
              return;
            }

            GUI.Message clientWindow = null;
            if (this.m_guiMessages.ContainsKey(senderClientId)) {
              clientWindow = this.m_guiMessages[senderClientId];
            }
            else {
              clientWindow = new GUI.Message();
              clientWindow.SetWindow(this.m_guiContacts.LocalContact, senderContact);
              clientWindow.onMessageAccept += (string msgContent) => this.SendClientMessage(senderContact.ClientId, msgContent);
              this.m_guiMessages.Add(senderClientId, clientWindow);
            }

            clientWindow.AddMessage(senderContact, clientMessage.Content);
            if (clientWindow.Visibility != System.Windows.Visibility.Visible) {
              Debug.Log("Showing notification to the local client.");
            }
          }
          break;
      }
    }

    private void HandleContactItemButton(UI.ContactItem.Contact senderContact, UI.ContactItem.Buttons clickedButton)
    {
      Debug.Log("Clicked on ClientId={0} Button={1}.", senderContact.ClientId, clickedButton.ToString());
      if (clickedButton == UI.ContactItem.Buttons.Information) {
        if (this.m_guiContactInformation.Visibility == System.Windows.Visibility.Visible) {
          this.m_guiContactInformation.Dispatcher.Invoke(() => this.m_guiContactInformation.Hide());
        }

        this.m_guiContactInformation.UpdatePublicKey(this.m_localClient.GetClientKey(senderContact.ClientId));
        this.m_guiContactInformation.UpdateClient(senderContact.Username, senderContact.ClientId);
        this.m_guiContactInformation.Dispatcher.Invoke(() => this.m_guiContactInformation.Show());
      }
      else if (clickedButton == UI.ContactItem.Buttons.Message) {
        GUI.Message clientMessageUI = null;
        if (this.m_guiMessages.ContainsKey(senderContact.ClientId)) {
          Debug.Log("Accessing created message window for ClientId={0}.", senderContact.ClientId);
          clientMessageUI = this.m_guiMessages[senderContact.ClientId];
        }
        else {
          Debug.Log("Creating message window for ClientId={0}.", senderContact.ClientId);
          clientMessageUI = new GUI.Message();
          clientMessageUI.SetWindow(this.m_guiContacts.LocalContact, senderContact);
          clientMessageUI.onMessageAccept += (string messageContent) => this.SendClientMessage(senderContact.ClientId, messageContent);
          this.m_guiMessages.Add(senderContact.ClientId, clientMessageUI);

          Task.Delay(500).ContinueWith(_ => this.RequestClientPublicKey(senderContact.ClientId));
        }

        if (clientMessageUI.Visibility != System.Windows.Visibility.Visible) {
          Debug.Log("Showing message window for ClientId={0}.", senderContact.ClientId);
          clientMessageUI.Show();
        }
      }
    }

    private void HandleConnectionErrors()
    {
      Debug.Log("Hiding all open windowses.");
      if (this.m_guiAuthenticate.IsVisible) {
        this.m_guiAuthenticate.Dispatcher.Invoke(() => this.m_guiAuthenticate.Hide());
      }
      if (this.m_guiContactInformation.IsVisible) {
        this.m_guiContactInformation.Dispatcher.Invoke(() => this.m_guiContactInformation.Hide());
      }
      if (this.m_guiContacts.IsVisible) {
        this.m_guiContacts.Dispatcher.Invoke(() => this.m_guiContacts.Hide());
      }
      foreach (var window in this.m_guiMessages) {
        if (window.Value.IsVisible) {
          window.Value.Dispatcher.Invoke(() => window.Value.Hide());
        }
      }

      Debug.Log("Show splash with error message.");
      this.m_guiSplash.SetMessage("Splash_ServerDisconnected");
      this.m_guiSplash.Show();
      this.CloseApplicationAfter(6000);
    }

    private void HandleApplicationClose()
    {
      this.m_guiSplash.SetMessage("Splash_Closing");
      this.m_guiSplash.Show();
      this.m_localClient.Disconnect();
      this.CloseApplicationAfter(6000);
    }
  }
}