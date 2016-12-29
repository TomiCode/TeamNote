using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Crypto.Parameters;
using System.Net.Sockets;

namespace TeamNote.Server
{
  class NetworkClient
  {
    private RsaKeyParameters m_clientPublicKey;
    private TcpClient m_networkClient;

    public bool IsConnected {
      get {
        return this.m_networkClient.Connected;
      }
    }

    public NetworkClient(TcpClient clientInstance)
    {
      this.m_networkClient = clientInstance;
    }
  }
}
