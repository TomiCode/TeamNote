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
    private UdpClient m_udpClient;
    private Configuration m_clientConfig;

    /* Client GUI types. */
    private GUI.Splash m_guiSplash;
    private GUI.Authenticate m_guiAuthenticate;

    private Thread m_configResponseListener;

    public ClientInstance()
    {
      /* udpClient initialization. */
      this.m_udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 48750));
      this.m_clientConfig = new Configuration(CONFIG_FILENAME);

      /* GUI initialization. */
      this.m_guiSplash = new GUI.Splash();
      this.m_guiAuthenticate = new GUI.Authenticate();

      this.m_configResponseListener = new Thread(this.ConfiguratorListener);
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

      this.m_configResponseListener.Start();
      this.SendBroadcastHello();
    }

    private bool SendBroadcastHello()
    {
      int udpServicePort = this.m_clientConfig.UDP_Port;
      if (udpServicePort == Configuration.INVALID_UDP_PORT) {
        Debug.Error("Invalid Broadcast service address configuration!");
        return false;
      }

      IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, udpServicePort);
      Header helloMessage = new Header();

      helloMessage.Type = MessageType.BroadcastClientRequest;
      helloMessage.Size = MessageSize.NoData;

      int sended = this.m_udpClient.Send(helloMessage.ToByteArray(), helloMessage.CalculateSize(), broadcastEndPoint);
      Debug.Log("Sended {0} bytes to {1}.", sended, broadcastEndPoint.Address);
      return true;
    }

    private void ConfiguratorListener()
    {
      Debug.Log("Started listen thread..");
      while (this.m_configResponseListener != null) {
        IPEndPoint senderAddress = new IPEndPoint(IPAddress.Any, 1337);
        byte[] dataBuffer = this.m_udpClient.Receive(ref senderAddress);

        Debug.Log("Response from {0}, bytes {1}.", senderAddress.Address, dataBuffer.Length);
      }
    }
  }
}