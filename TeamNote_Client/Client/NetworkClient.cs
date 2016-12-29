using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;

namespace TeamNote.Client
{
  class NetworkClient
  {
    private PgpPublicKey m_clientPublicKey;
    // private RsaKeyParameters m_clientPublicKey;
    // private TcpClient m_networkClient;

    public NetworkClient()
    {
      
    }

    public bool Connect(IPEndPoint serverAddress)
    {
      //try {
      //  this.m_networkClient.Connect(serverAddress);
      //}
      //catch (Exception ex) {
      //  Debug.Exception(ex);
      //  return false;
      //}
      //return true;
    }
  }
}
