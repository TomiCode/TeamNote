using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TeamNote.Server
{
  class ServerInstance
  {
    private Configuration m_serverConfig;
    private DiscoveryService m_discoveryService;

    private Dictionary<long, object> m_connectedClients;
    
    private TcpListener m_serverListener;
    private Thread m_serverThread;

    public ServerInstance()
    {
      this.m_serverConfig = new Configuration("ServerConfig.json");
      this.m_connectedClients = new Dictionary<long, object>();

      if (!this.m_serverConfig.LoadConfig()) {
        this.m_serverConfig.CreateDefaults();
        if (!this.m_serverConfig.SaveConfig()) {
          Debug.Error("Can not create new configuration file. Loaded={0}", this.m_serverConfig.ConfigLoaded);
        }
      }

      this.m_discoveryService = new DiscoveryService(this.m_serverConfig.ConfigService);

      /* TCP/IP Listener */
      this.m_serverListener = new TcpListener(this.m_serverConfig.ListenAddress);

      /* Thread initialization. */
      this.m_serverThread = new Thread(this.ServerListener);
    }

    public void Start()
    {
      Debug.Log("Starting server threads and listeners..");
      this.m_discoveryService.Start(this.m_serverConfig.ListenAddress);
      this.m_serverListener.Start();
      this.m_serverThread.Start();
    }

    private void ServerListener()
    {
      while (this.m_serverListener != null) {
        TcpClient l_tcpClient = this.m_serverListener.AcceptTcpClient();
        Debug.Log("New client connection accepted.");
      }
    }

  }
}
