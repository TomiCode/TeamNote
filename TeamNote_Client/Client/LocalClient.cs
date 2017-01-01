using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

using Google.Protobuf;

using TeamNote.Protocol;
using System.IO;

namespace TeamNote.Client
{
  class LocalClient
  {
    public delegate void MessageReceivedEvent(int type, ByteString message);
    public event MessageReceivedEvent onMessageReceived;

    const int KEY_STRENGTH = 1024;

    private PgpPublicKey m_localPublicKey;
    private PgpPrivateKey m_localPrivateKey;

    private PgpPublicKey m_serverKey;
    
    private TcpClient m_tcpClient;
    private Thread m_listenThread;

    public bool IsConnected {
      get {
        return this.m_tcpClient.Connected;
      }
    }

    private PublicKey ServerKey {
      get {
        PublicKey key = new PublicKey();

        /* Add exception handling. */
        RsaKeyParameters keyParams = this.m_localPublicKey.GetKey() as RsaKeyParameters;
        
        key.Exponent = Convert.ToBase64String(keyParams.Exponent.ToByteArray());
        key.Modulus = Convert.ToBase64String(keyParams.Modulus.ToByteArray());

        return key;
      }
    }

    public LocalClient()
    {
      this.m_tcpClient = new TcpClient();
      this.m_listenThread = new Thread(this.ListenThread);
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

      this.m_listenThread.Start();
      return true;
    }

    public void InitializeKeypair()
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
      Debug.Log("Sending handshake to server.");

      HandshakeRequest handshakeRequest = new HandshakeRequest();
      PublicKey publicKey = this.ServerKey;
      handshakeRequest.Key = publicKey;

      this.SendMessage(MessageType.ClientHandshakeRequest, handshakeRequest.ToByteString(), false);
    }

    public void UpdateServerPublicKey(PublicKey serverKey)
    {
      byte[] modulus = Convert.FromBase64String(serverKey.Modulus);
      byte[] exponent = Convert.FromBase64String(serverKey.Exponent);

      RsaKeyParameters publicKeyParams = new RsaKeyParameters(false, new BigInteger(modulus), new BigInteger(exponent));
      this.m_serverKey = new PgpPublicKey(PublicKeyAlgorithmTag.RsaGeneral, publicKeyParams, DateTime.Now);

      Debug.Log("Updated server public key.");
    }

    public bool SendMessage(int type, ByteString message)
    {
      return this.SendMessage(type, message, true);
    }

    private bool SendMessage(int type, ByteString message, bool encrypted)
    {
      NetworkPacket networkPacket = new NetworkPacket();
      networkPacket.Type = type;

      if (encrypted) {
        MemoryStream ms = new MemoryStream();

        PgpEncryptedDataGenerator encGen = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Blowfish, true);
        encGen.AddMethod(this.m_serverKey);

        Stream outStream = encGen.Open(ms, message.Length);
        outStream.Write(message.ToByteArray(), 0, message.Length);
        outStream.Close();

        networkPacket.Message = ByteString.CopyFrom(ms.ToArray());
      }
      else {
        networkPacket.Message = message;
      }
      int byteSend = this.m_tcpClient.Client.Send(networkPacket.ToByteArray());
      Debug.Log("Send message Type={0:X8} Size={1}.", type, byteSend);

      return true;
    }

    private void ListenThread()
    {
      Debug.Log("Started listening for server responses.");

      byte[] messageBuffer = new byte[512];
      int bytesReceived = 0;

      while ((bytesReceived = this.m_tcpClient.Client.Receive(messageBuffer)) != 0) {
        Debug.Log("Received message bytes={0}.", bytesReceived);
        NetworkPacket receivedPacket = NetworkPacket.Parser.ParseFrom(new CodedInputStream(messageBuffer, 0, bytesReceived));

        this.onMessageReceived?.Invoke(receivedPacket.Type, receivedPacket.Message);
      }
    }

  }
}
