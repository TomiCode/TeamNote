﻿using System;
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

      /* GUI initialization. */
      this.m_guiSplash = new GUI.Splash();
      
      this.m_guiAuthenticate = new GUI.Authenticate();
      this.m_guiAuthenticate.onAuthorizationAccept += this.SendAuthorization;

      this.m_guiContacts = new GUI.Contacts(this.HandleContactItemButton, this.SendClientDataUpdateChange);
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
      this.m_localClient.InitializeKeypair();

      this.m_guiSplash.Show();
      this.m_serverDiscoverer.Start(this.m_clientConfig.UDP_Port);

      this.UpdateStatusMessage("Splash_Discover");
    }

    private void HandleDiscoveryResponse(IPEndPoint serverAddress)
    {
      Debug.Log("Address: {0}", serverAddress);
      this.m_guiSplash.SetMessage("Splash_Connect");
      Task.Delay(2000).ContinueWith(_ => this.ConnectToServer(serverAddress));
    }

    private void HandleDiscoveryFailure(int retriesCount)
    {
      Debug.Warn("Handling discoverer failure.");
      this.UpdateStatusMessage("Splash_DiscoverFailure");

      Task.Delay(5000).ContinueWith(_ => {
        this.m_appCloseDelegate(1);
      });
    }

    private void ConnectToServer(IPEndPoint serverAddress)
    {
      if (this.m_localClient.Connect(serverAddress)) {
        Debug.Log("Connected to server!");
        this.m_localClient.SendHandshake();
      }
      else {
        Debug.Error("Could not connect to remote server {0}.", serverAddress);
        this.UpdateStatusMessage("Splash_CannotConnect");
      }
    }

    private void SendAuthorization(string name, string surname)
    {
      AuthorizationRequest request = new AuthorizationRequest();
      request.Name = name;
      request.Surname = surname;

      Debug.Log("Sending authorization to server [{0} {1}].", name, surname);
      this.m_localClient.SendMessage(MessageType.AuthorizationRequest, request);
      this.m_guiContacts.LocalContact.SetUsername(name, surname);
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

    private void UpdateStatusMessage(string resourceString)
    {
      this.m_guiSplash.SetMessage(resourceString);
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
      }
    }

    private void ReceivedClientMessage(long senderClientId, int messageType, ByteString messageContent)
    {

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
    }
  }
}