using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
    private int discoveryResponsePort;
    private int discoveryMagicNumber;
    
    public ServerDiscoverer()
    {
      Random l_random = new Random(DateTime.Now.Millisecond);
      this.discoveryResponsePort = l_random.Next(DISCOVERY_LOW_PORTNUM, DISCOVERY_HIGH_PORTNUM);
      this.discoveryMagicNumber = l_random.Next(0, ushort.MaxValue);

      this.m_discoverClient = new UdpClient(new IPEndPoint(IPAddress.Any, this.discoveryResponsePort));
      this.m_discovererThread = new Thread(this.DiscovererThread);
    }

    public void StartListening()
    {
      Debug.Log("Starting server discoverer. Port: {0} Magic: {1}.", this.discoveryResponsePort, this.discoveryMagicNumber);
      this.m_discovererThread.Start();
    }

    public void RequestDiscover()
    {

    }

    private void DiscovererThread()
    {
      IPEndPoint responseAddress = new IPEndPoint(IPAddress.Any, this.discoveryResponsePort);
      while (this.m_discoverClient != null) {
        this.m_discoverClient.Receive(ref responseAddress);
      }
    }

  }
}
