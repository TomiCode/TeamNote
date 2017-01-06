using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

using Google.Protobuf;

using TeamNote.Protocol;

namespace TeamNote.Client
{
  class LocalClient
  {
    /* Constants. */
    const int KEY_STRENGTH = 1024;
    const int LISTEN_BUFFER_SIZE = 4096;

    /* Client events and delegates. */
    public delegate void ServerMessageReceivedHandler(int type, ByteString message);
    public delegate void ClientMessageReceivedHandler(long sender, int type, ByteString message);
    public delegate void ServerConnectionErrorHandler();

    public event ServerMessageReceivedHandler onServerMessageReceived;
    public event ClientMessageReceivedHandler onClientMessageReceived;
    public event ServerConnectionErrorHandler onConnectionErrors;

    /* Local Keypair. */
    private AsymmetricCipherKeyPair m_localCipherKeys;

    /* Server public key. */
    private AsymmetricKeyParameter m_serverKey;

    /* Connected client keyring. */
    private Dictionary<long, AsymmetricKeyParameter> m_localKeyring;
    
    /* Private members. */
    private TcpClient m_tcpClient;
    private Thread m_listenThread;

    private volatile bool m_requestedDisconnect;

    /* Properties. */
    public bool IsConnected {
      get {
        return this.m_tcpClient.Connected;
      }
    }

    private PublicKey LocalPublicKey {
      get {
        PublicKey serverKey = new PublicKey();
        RsaKeyParameters publicKey = this.m_localCipherKeys.Public as RsaKeyParameters;
        if (publicKey == null) {
          Debug.Warn("Invalid server public key.");
          return null;
        }

        serverKey.Exponent = ByteString.CopyFrom(publicKey.Exponent.ToByteArray());
        serverKey.Modulus = ByteString.CopyFrom(publicKey.Modulus.ToByteArray());
        return serverKey;
      }
    }

    /* Public methods. */
    public LocalClient()
    {
      this.m_localKeyring = new Dictionary<long, AsymmetricKeyParameter>();
      this.m_requestedDisconnect = false;

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

    public void Disconnect()
    {
      Debug.Log("Disconnecting from server and stopping threads.");
      this.m_requestedDisconnect = true;

      if (this.m_tcpClient.Connected) {
        Debug.Log("Closing server connection.");
        this.m_tcpClient.Client.Disconnect(false);
        this.m_tcpClient.Close();
      }
    }

    public void InitializeKeypair()
    {
      RsaKeyPairGenerator keyGenerator = new RsaKeyPairGenerator();
      keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), KEY_STRENGTH));

      this.m_localCipherKeys = keyGenerator.GenerateKeyPair();

      RsaKeyParameters localKeyParameters = this.m_localCipherKeys.Public as RsaKeyParameters;
      Debug.Log("Generated Public key: E={0}, M={1}.",
        Convert.ToBase64String(localKeyParameters.Exponent.ToByteArray()),
        Convert.ToBase64String(localKeyParameters.Modulus.ToByteArray())
      );
    }

    public bool SendHandshake()
    {
      Debug.Log("Sending handshake to server.");

      HandshakeRequest handshakeRequest = new HandshakeRequest();
      PublicKey publicKey = this.LocalPublicKey;
      if (publicKey == null) {
        Debug.Warn("Client public key is invalid.");
        return false;
      }
      handshakeRequest.Key = publicKey;

      return this.SendMessage(MessageType.ClientHandshakeRequest, handshakeRequest, false);
    }

    public void UpdateServerKey(PublicKey serverKey)
    {
      this.m_serverKey = new RsaKeyParameters(
        isPrivate: false,
        exponent: new BigInteger(serverKey.Exponent.ToByteArray()),
        modulus: new BigInteger(serverKey.Modulus.ToByteArray())
      );
      Debug.Log("Server public key updated.");
    }

    public void RegisterClientKey(long client, PublicKey key)
    {
      Debug.Log("Adding ClientId={0} to keyring.", client);
      RsaKeyParameters l_keyParam = new RsaKeyParameters(
        isPrivate: false,
        exponent: new BigInteger(key.Exponent.ToByteArray()),
        modulus: new BigInteger(key.Modulus.ToByteArray())
      );

      this.m_localKeyring.Add(client, l_keyParam);
    }

    public AsymmetricKeyParameter GetClientKey(long client)
    {
      Debug.Log("Requesting ClientId={0} public key.", client);
      if (!this.m_localKeyring.ContainsKey(client)) return null;
      return this.m_localKeyring[client];
    }

    public bool HasClientKey(long client)
    {
      return this.m_localKeyring.ContainsKey(client);
    }

    public bool RemoveClientKey(long client)
    {
      Debug.Log("Removing ClientId={0} from keyring.", client);
      return this.m_localKeyring.Remove(client);
    }

    public bool SendMessage(int type, IMessage message)
    {
      return this.SendMessage(0, true, type, message, true);
    }

    public bool SendMessage(long client, int type, IMessage message)
    {
      return this.SendMessage(client, false, type, message, true);
    }

    private bool SendMessage(int type, IMessage message, bool encrypt)
    {
      return this.SendMessage(0, true, type, message, encrypt);
    }

    private bool SendMessage(long client, int type, IMessage message, bool encrypt)
    {
      return this.SendMessage(client, false, type, message, encrypt);
    }

    private bool SendMessage(long client, bool server, int type, IMessage message, bool encrypt)
    {
      if (!this.m_tcpClient.Connected) {
        Debug.Error("Cannot send data through a disconnected connection.");
        return false;
      }

      ByteString messageContent = message.ToByteString();
      NetworkPacket networkPacket = new NetworkPacket();

      networkPacket.Type = type;
      networkPacket.Server = server;
      networkPacket.ClientId = client;
      networkPacket.Encrypted = encrypt;
      
      if (networkPacket.Encrypted) {
        AsymmetricKeyParameter localKeyParameter = null;

        if (networkPacket.Server)
          localKeyParameter = this.m_serverKey;
        else if(this.m_localKeyring.ContainsKey(client))
          localKeyParameter = this.m_localKeyring[client];

        if (localKeyParameter == null) {
          Debug.Error("Invalid key for Send. ClientId={0} Server={1}.", client, server);
          return false;
        }

        messageContent = this.ProceedMessageEncoding(localKeyParameter, messageContent);
        if (messageContent == null) {
          Debug.Error("Processed message is invalid.");
          return false;
        }
      }
      networkPacket.Message = messageContent;

      try {
        int networkPacketSize = networkPacket.CalculateSize();
        int bytesSend = this.m_tcpClient.Client.Send(networkPacket.ToByteArray());

        Debug.Log("Sended {0} bytes. Network packet Size={1}.", bytesSend, networkPacketSize);
        return (bytesSend == networkPacketSize);
      }
      catch (Exception ex) {
        Debug.Exception(ex);

        if(!this.m_requestedDisconnect)
          this.ServerConnectionError();

        return false;
      }
    }

    private ByteString ProceedMessageEncoding(AsymmetricKeyParameter messageKey, ByteString messageBytes)
    {
      Debug.Log("Proceeding with key IsPrivate={0} Message Size={1}.", messageKey.IsPrivate, messageBytes.Length);

      OaepEncoding cipherEncoder = new OaepEncoding(new RsaBlindedEngine(), new Sha256Digest());
      cipherEncoder.Init(!(messageKey.IsPrivate), messageKey);

      using (MemoryStream outputStream = new MemoryStream())
      using (Stream inputStream = new MemoryStream(messageBytes.ToByteArray())) {
        byte[] inputBlock = new byte[cipherEncoder.GetInputBlockSize()];
        int readBytes = 0;

        while ((readBytes = inputStream.Read(inputBlock, 0, inputBlock.Length)) > 0) {
          byte[] outputBlock = cipherEncoder.ProcessBlock(inputBlock, 0, readBytes);
          outputStream.Write(outputBlock, 0, outputBlock.Length);

          if (readBytes != inputBlock.Length)
            break;
        }
        return ByteString.CopyFrom(outputStream.ToArray(), 0, (int)outputStream.Length);
      }
    }

    private void ListenThread()
    {
      Debug.Log("Started listening for server responses.");
      byte[] messageBuffer = new byte[LISTEN_BUFFER_SIZE];
      int bytesReceived = 0;

      try {
        while ((bytesReceived = this.m_tcpClient.Client.Receive(messageBuffer)) != 0) {
          NetworkPacket receivedPacket = NetworkPacket.Parser.ParseFrom(new CodedInputStream(messageBuffer, 0, bytesReceived));
          Debug.Log("Received packet Type={0} Server={1} ClientId={2} Encrypted={3}.",
            receivedPacket.Type, receivedPacket.Server, receivedPacket.ClientId, receivedPacket.Encrypted);

          ByteString messageContent = receivedPacket.Message;
          if (receivedPacket.Encrypted) {
            messageContent = this.ProceedMessageEncoding(this.m_localCipherKeys.Private, messageContent);
            if (messageContent == null) {
              Debug.Error("Encoded message content is invalid.");
              continue;
            }
          }

          if (receivedPacket.Server) {
            this.onServerMessageReceived?.Invoke(receivedPacket.Type, messageContent);
          }
          else {
            this.onClientMessageReceived?.Invoke(receivedPacket.ClientId, receivedPacket.Type, messageContent);
          }
        }
      }
      catch (Exception ex) {
        Debug.Exception(ex);
        if (!this.m_requestedDisconnect)
          this.ServerConnectionError();
      }
      Debug.Warn("Stopped listen thread.");
    }

    private void ServerConnectionError()
    {
      Debug.Warn("Connection to server closed.");
      if (this.m_tcpClient.Client != null && this.m_tcpClient.Connected) {
        Debug.Warn("Closing connected TCPClient.");
        this.m_tcpClient.Close();
      }
      this.onConnectionErrors?.Invoke();
    }
  }
}