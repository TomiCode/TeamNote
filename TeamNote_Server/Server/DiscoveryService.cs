using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Google.Protobuf;

using TeamNote.Protocol;

namespace TeamNote.Server
{
  class DiscoveryService
  {
    private UdpClient m_discoveryService;
    private IPEndPoint m_serverAddress;

    private Thread m_discoveryThread;
    private bool m_serviceListening;

    public DiscoveryService(IPEndPoint listenAddress)
    {
      this.m_discoveryService = new UdpClient(listenAddress);

      this.m_discoveryThread = new Thread(this.DiscoveryListener);
      this.m_serviceListening = false;
    }

    public void Start(IPEndPoint serverAddress)
    {
      Debug.Notice("Starting discovery service.");

      this.m_serverAddress = serverAddress;
      this.m_serviceListening = true;

      this.m_discoveryThread.Start();
    }

    private void DiscoveryListener()
    {
      IPEndPoint l_receiveAddress = new IPEndPoint(IPAddress.Any, 0);
      while (this.m_serviceListening) {
        byte[] dataPacket = this.m_discoveryService.Receive(ref l_receiveAddress);
        if (l_receiveAddress.Address == IPAddress.Any) {
          Debug.Warn("Invalid response address '{0}'.", l_receiveAddress);
          continue;
        }

        NetworkPacket l_receivedPacket = NetworkPacket.Parser.ParseFrom(dataPacket);
        if (l_receivedPacket == null || l_receivedPacket.Type != MessageType.ServiceConfigurationRequest) {
          Debug.Error("Invalid network packet.");
          continue;
        }

        try {
          ConfigRequest l_clientRequest = ConfigRequest.Parser.ParseFrom(l_receivedPacket.Message);
          Debug.Log("Client request ServiceId={0} Port={1}.", l_clientRequest.ServiceId, l_clientRequest.Port);

          ConfigResponse l_responseMessage = new ConfigResponse();
          l_responseMessage.ServiceId = l_clientRequest.ServiceId;
          l_responseMessage.IPAddress = this.m_serverAddress.Address.ToString();
          l_responseMessage.Port = this.m_serverAddress.Port;

          NetworkPacket l_responsePacket = new NetworkPacket();
          l_responsePacket.Type = MessageType.ServiceConfigurationResponse;
          // l_responsePacket.Message = Any.Pack(l_responseMessage, String.Empty);
          l_responsePacket.Message = l_responseMessage.ToByteString();

          l_receiveAddress.Port = l_clientRequest.Port;
          byte[] responseData = l_responsePacket.ToByteArray();
          int sendBytes = this.m_discoveryService.Send(responseData, responseData.Length, l_receiveAddress);

          Debug.Log("Response send to {0}, Bytes={1}.", l_receiveAddress, sendBytes);
        }
        catch (Exception ex) {
          Debug.Exception(ex);
          continue;
        }
      }
    }

  }
}
