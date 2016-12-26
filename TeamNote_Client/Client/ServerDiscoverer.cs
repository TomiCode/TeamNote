using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

using TeamNote.Protocol;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace TeamNote.Client
{
  class ServerDiscoverer
  {
    public const int DISCOVERY_LOW_PORTNUM = 49152;
    public const int DISCOVERY_HIGH_PORTNUM = 65534;

    public delegate void ReceivedDiscoveryResponse(IPEndPoint serverAddress);
    public event ReceivedDiscoveryResponse onDiscoveryResponse;

    private UdpClient m_discoverClient;
    private Thread m_discovererThread;

    private int m_requestPort;
    private int m_responsePort;
    private int m_serviceId;

    private DispatcherTimer m_discoverDispatcher;
    private bool m_serverDiscovered;
    
    public ServerDiscoverer()
    {
      Random l_random = new Random(DateTime.Now.Millisecond);
      this.m_responsePort = l_random.Next(DISCOVERY_LOW_PORTNUM, DISCOVERY_HIGH_PORTNUM);
      this.m_serviceId = l_random.Next(short.MaxValue);
      this.m_requestPort = 0;

      this.m_discoverClient = new UdpClient(new IPEndPoint(IPAddress.Any, this.m_responsePort));
      this.m_discovererThread = new Thread(this.DiscoverResponseListener);

      this.m_discoverDispatcher = new DispatcherTimer();
      this.m_discoverDispatcher.Tick += this.DiscoverDispatcher_Tick;
      this.m_discoverDispatcher.Interval = TimeSpan.FromSeconds(5.0);

      this.m_serverDiscovered = false;
    }

    public void Start(int requestPort)
    {
      this.m_requestPort = requestPort;
      Debug.Log("Starting server discoverer requestPort={0} responsePort={1} serviceId={2}.", this.m_requestPort, this.m_responsePort, this.m_serviceId);

      this.m_discovererThread.Start();
      this.m_discoverDispatcher.Start();
    }

    private void DiscoverResponseListener()
    {
      IPEndPoint responseAddress = new IPEndPoint(IPAddress.Any, this.m_responsePort);
      while (this.m_discoverClient != null) {
        byte[] responseData = this.m_discoverClient.Receive(ref responseAddress);
        Debug.Log("Response received Bytes={0}.", responseData.Length);

        NetworkPacket l_responsePacket = NetworkPacket.Parser.ParseFrom(responseData);
        if (l_responsePacket == null) {
          Debug.Warn("Invalid Discovery response.");
          continue;
        }

        if (l_responsePacket.Type != MessageType.ServiceConfigurationResponse) {
          Debug.Error("Invalid response packet type '{0}'.", l_responsePacket.Type);
          continue;
        }

        ConfigResponse l_discoveryResponse = l_responsePacket.Message.Unpack<ConfigResponse>();
        if (l_discoveryResponse == null) {
          Debug.Warn("Empty discovery response message.");
          continue;
        }

        if (l_discoveryResponse.ServiceId != this.m_serviceId) {
          Debug.Log("Response serviceId missmatch ({0} != {1}), skipping response packet.", this.m_serviceId, l_discoveryResponse.ServiceId);
          continue;
        }

        IPAddress serverAddress;
        if (!IPAddress.TryParse(l_discoveryResponse.IPAddress, out serverAddress)) {
          Debug.Error("Cannot parse server IPAddress field '{0}'.", l_discoveryResponse.IPAddress);
          continue;
        }

        if (this.onDiscoveryResponse == null) {
          Debug.Error("No response event handler. :(");
          continue;
        }

        this.onDiscoveryResponse(new IPEndPoint(serverAddress, l_discoveryResponse.Port));
        this.m_discoverDispatcher.Stop();
        return;
      }
    }

    private void DiscoverDispatcher_Tick(object sender, EventArgs args)
    {
      if (this.m_serverDiscovered) {
        Debug.Log("Stopping discovery dispatcher.");
        this.m_discoverDispatcher.Stop();
        return;
      }
      
      ConfigRequest l_requestMessage = new ConfigRequest();
      l_requestMessage.Port = this.m_responsePort;
      l_requestMessage.ServiceId = this.m_serviceId;

      NetworkPacket l_requestPacket = new NetworkPacket();
      l_requestPacket.Type = MessageType.ServiceConfigurationRequest;
      l_requestPacket.Message = Any.Pack(l_requestMessage, "teamnote.v2.protocol");

      byte[] requestData = l_requestPacket.ToByteArray();
      int sendBytesCount = this.m_discoverClient.Send(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, this.m_requestPort));

      Debug.Log("Sended request bytes={0} port={1}.", sendBytesCount, this.m_requestPort);
    }

  }
}
