using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

using Google.Protobuf;

using TeamNote.Protocol;

namespace TeamNote.Client
{
  class ServerDiscoverer
  {
    public const int DISCOVERY_LOW_PORTNUM = 49152;
    public const int DISCOVERY_HIGH_PORTNUM = 65534;

    public const int MAX_RETRIES = 0;

    public delegate void ServerDiscovererResponseDelegate(IPEndPoint serverAddress);
    public delegate void ServerDiscovererFailedDelegate(int retries);

    public event ServerDiscovererResponseDelegate onDiscoveryResponse;
    public event ServerDiscovererFailedDelegate onDiscoveryFailed;

    private DispatcherTimer m_discoverDispatcher;
    private Thread m_discovererThread;

    private UdpClient m_discoverClient;
    private IPEndPoint m_requestAddress;

    private bool m_serverDiscovered;
    private int m_responsePort;
    private int m_serviceId;

    private int m_sendRequests;

    public bool ServerDiscovered {
      get {
        return this.m_serverDiscovered;
      }
    }
    
    public ServerDiscoverer()
    {
      Random l_random = new Random(DateTime.Now.Millisecond);
      this.m_responsePort = l_random.Next(DISCOVERY_LOW_PORTNUM, DISCOVERY_HIGH_PORTNUM);
      this.m_serviceId = l_random.Next(short.MaxValue);

      this.m_serverDiscovered = false;
      this.m_sendRequests = 0;

      this.m_discoverClient = new UdpClient(new IPEndPoint(IPAddress.Any, this.m_responsePort));
      this.m_discovererThread = new Thread(this.DiscoverResponseListener);

      this.m_discoverDispatcher = new DispatcherTimer();
      this.m_discoverDispatcher.Tick += this.DiscoverDispatcher_Tick;
      this.m_discoverDispatcher.Interval = TimeSpan.FromSeconds(2.0);
    }

    public void Start(int requestPort)
    {
      this.m_requestAddress = new IPEndPoint(IPAddress.Broadcast, requestPort);
      Debug.Log("Starting server discoverer requestAddress={0} responsePort={1} serviceId={2}.", this.m_requestAddress, this.m_responsePort, this.m_serviceId);

      this.m_discovererThread.Start();
      this.m_discoverDispatcher.Start();
    }

    public void Stop(bool abortThread = false)
    {
      Debug.Warn("Stopping discoverer.");

      /* Stop threads and listeners. */
      if (abortThread)
        this.m_discovererThread.Abort();

      this.m_discoverDispatcher.Stop();

      /* Close UDP Client. */
      this.m_discoverClient.Close();
    }

    private void DiscoverResponseListener()
    {
      IPEndPoint responseAddress = new IPEndPoint(IPAddress.Any, this.m_responsePort);
      while (!this.m_serverDiscovered) {
        byte[] responseData = this.m_discoverClient.Receive(ref responseAddress);
        Debug.Log("Response received Bytes={0}.", responseData.Length);

        NetworkPacket l_responsePacket = NetworkPacket.Parser.ParseFrom(responseData);
        if (l_responsePacket == null || l_responsePacket.Type != MessageType.ServiceConfigurationResponse) {
          Debug.Warn("Invalid Discovery response.");
          continue;
        }

        try {
          ConfigResponse l_discoveryResponse = ConfigResponse.Parser.ParseFrom(l_responsePacket.Message);
          if (l_discoveryResponse.ServiceId != this.m_serviceId) {
            Debug.Log("Response serviceId missmatch ({0} != {1}), skipping response packet.", this.m_serviceId, l_discoveryResponse.ServiceId);
            continue;
          }

          this.onDiscoveryResponse?.Invoke(new IPEndPoint(IPAddress.Parse(l_discoveryResponse.IPAddress), l_discoveryResponse.Port));
          this.m_serverDiscovered = true;
          this.Stop();
        }
        catch (Exception ex) {
          Debug.Exception(ex);
        }
      }
      Debug.Log("Stopped listener.");
    }

    private void DiscoverDispatcher_Tick(object sender, EventArgs args)
    {
      if (this.m_sendRequests > MAX_RETRIES) {
        Debug.Error("Could not discover network server. Sended {0} discovery requests.", this.m_sendRequests);

        this.Stop(true);
        this.onDiscoveryFailed?.Invoke(this.m_sendRequests);
        return;
      }

      ConfigRequest l_requestMessage = new ConfigRequest();
      l_requestMessage.Port = this.m_responsePort;
      l_requestMessage.ServiceId = this.m_serviceId;

      NetworkPacket l_requestPacket = new NetworkPacket();
      l_requestPacket.Type = MessageType.ServiceConfigurationRequest;
      l_requestPacket.Message = l_requestMessage.ToByteString();

      byte[] requestData = l_requestPacket.ToByteArray();
      int sendBytesCount = this.m_discoverClient.Send(requestData, requestData.Length, this.m_requestAddress);

      Debug.Log("Send {0}. Discovery request Address={1} Bytes={2}.", this.m_sendRequests, this.m_requestAddress, sendBytesCount);
      this.m_sendRequests++;
    }
  }
}
