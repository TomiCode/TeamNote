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
    private Dictionary<long, object> m_connectedClients;
    
    private TcpListener m_serverListener;
    private UdpClient m_udpClient;

    private Thread m_serverThread;
    private Thread m_autoConfigThread;

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

      /* TCP/IP Listener */
      this.m_serverListener = new TcpListener(this.m_serverConfig.ListenAddress);
      /* UDP Auto-configuration client (data sender) */
      this.m_udpClient = new UdpClient(this.m_serverConfig.ConfigService);

      /* Thread initialization. */
      this.m_serverThread = new Thread(this.ServerListener);
      this.m_autoConfigThread = new Thread(this.ConfiguratorListener);
    }

    public void Start()
    {
      Debug.Log("Starting server threads and listeners..");
      this.m_serverListener.Start();
      this.m_serverThread.Start();
      this.m_autoConfigThread.Start();
    }

    private void ServerListener()
    {
      while (this.m_serverListener != null) {
        TcpClient l_tcpClient = this.m_serverListener.AcceptTcpClient();
        Debug.Log("New client connection accepted.");

        // ??
      }
    }

    private void ConfiguratorListener()
    {
      while (this.m_udpClient != null) {
        IPEndPoint senderAddress = new IPEndPoint(IPAddress.Any, 1337);
        byte[] dataBuffer = m_udpClient.Receive(ref senderAddress);

        Debug.Log("Received {0} bytes of configuration stream from {1}.", dataBuffer.Length, senderAddress.Address);

        byte[] test = new byte[] { 1,2,3,4 };
        senderAddress.Port = 48750;

        int sended = m_udpClient.Send(test, test.Length, senderAddress);
        Debug.Log("Response send: {0} bytes.", sended);
      }
    }
  }
}
