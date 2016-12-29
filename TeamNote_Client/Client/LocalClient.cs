using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using Org.BouncyCastle.Bcpg.OpenPgp;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace TeamNote.Client
{
  class LocalClient
  {
    private PgpPublicKey m_localPublicKey;
    private PgpPrivateKey m_localPrivateKey;
    
    private TcpClient m_tcpClient;

    public bool IsConnected {
      get {
        return this.m_tcpClient.Connected;
      }
    }

    public LocalClient()
    {
      this.m_tcpClient = new TcpClient();
    }

    public void InitializeEncryption()
    {

    }
  }
}
