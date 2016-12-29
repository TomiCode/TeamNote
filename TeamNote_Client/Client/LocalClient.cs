using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

using TeamNote.Protocol;

namespace TeamNote.Client
{
  class LocalClient
  {
    const int KEY_STRENGTH = 512;

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

    public bool Connect(IPEndPoint serverAddr)
    {
      Debug.Log("Connecting to {0}.", serverAddr);

      try {
        this.m_tcpClient.Connect(serverAddr);
      }
      catch (Exception ex) {
        Debug.Exception(ex);
        return false;
      }
      return true;
    }

    public void InitializeEncryption()
    {
      Debug.Log("Starting client key generation.");
      RsaKeyPairGenerator keyGenerator = new RsaKeyPairGenerator();
      keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), KEY_STRENGTH));

      AsymmetricCipherKeyPair localKeyPair = keyGenerator.GenerateKeyPair();
      this.m_localPublicKey = new PgpPublicKey(PublicKeyAlgorithmTag.RsaGeneral, localKeyPair.Public, DateTime.Now);
      this.m_localPrivateKey = new PgpPrivateKey(this.m_localPublicKey.KeyId, this.m_localPublicKey.PublicKeyPacket, localKeyPair.Private);

      RsaKeyParameters localKeyParameters = this.m_localPublicKey.GetKey() as RsaKeyParameters;
      Debug.Log("Generated Public key: E={0}, M={1}.",
        Convert.ToBase64String(localKeyParameters.Exponent.ToByteArray()),
        Convert.ToBase64String(localKeyParameters.Modulus.ToByteArray())
      );
    }

    public void SendHandshake()
    {

    }

    private bool SendUnencrypted(NetworkPacket packet)
    {
      return false;
    }

    private bool SendEncrypted(NetworkPacket packet)
    {
      return false;
    }

  }
}
