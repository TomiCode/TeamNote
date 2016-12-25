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

      /* GUI initialization. */
      this.m_guiSplash = new GUI.Splash();
      this.m_guiAuthenticate = new GUI.Authenticate();
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

      this.m_serverDiscoverer.Start(this.m_clientConfig.UDP_Port);
    }

    private void ConnectToServer(IPEndPoint serverAddress)
    {
      Debug.Log("Address: {0}", serverAddress);
    }
  }
}