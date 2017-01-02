using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace TeamNote.Server
{
  class Configuration
  {
    public const int INVALID_PORT = -1;

    public const int DEFAULT_UDP_PORT = 1337;
    public const int DEFAULT_TCP_PORT = 1330;

    public const string SERVER_NAME = "TeamNote v2 Srv";

    [DataContract]
    class Fields
    {
      [DataMember(IsRequired = true, Name = "configServicePort")]
      public int udpPort { get; set; }

      [DataMember(IsRequired = false, Name = "configServiceBindAddress")]
      public string udpAddress { get; set; }

      [DataMember(IsRequired = false, Name = "listenAddress")]
      public string tcpListener { get; set; }

      [DataMember(IsRequired = true, Name = "listenPort")]
      public int tcpListenPort { get; set; }

      [DataMember(IsRequired = false, Name = "serverName")]
      public string serverName { get; set; }

      public Fields()
      {
        Debug.Log("Creating new config fields.");

        /* Client automatic configuration port. */
        this.udpPort = DEFAULT_UDP_PORT;
        this.udpAddress = "127.0.0.1";

        /* Server tcp configuration. */
        this.tcpListener = "127.0.0.1";
        this.tcpListenPort = DEFAULT_TCP_PORT;
      }
    }

    private string m_configFilename;

    private Fields m_configFields;

    private DataContractJsonSerializer m_currentSerializer;

    public bool ConfigLoaded {
      get {
        return (this.m_configFields != null);
      }
    }

    public IPEndPoint ConfigService {
      get {
        if (!this.ConfigLoaded) {
          Debug.Warn("Config not loaded. Using default IPEndPoint.");
          return new IPEndPoint(IPAddress.Loopback, DEFAULT_UDP_PORT);
        }

        IPAddress l_ipAddress;
        if (!IPAddress.TryParse(this.m_configFields.udpAddress, out l_ipAddress)) {
          Debug.Error("Invalid ConfigServiceAddress in config file.");
          return new IPEndPoint(IPAddress.Loopback, DEFAULT_UDP_PORT);
        }

        return new IPEndPoint(l_ipAddress, this.m_configFields.udpPort);
      }
    }

    public IPEndPoint ListenAddress {
      get {
        if (!this.ConfigLoaded) {
          Debug.Warn("Config not loaded. Loopback is the result value.");
          return new IPEndPoint(IPAddress.Loopback, DEFAULT_TCP_PORT);
        }

        IPAddress l_readAddress;
        if (!IPAddress.TryParse(this.m_configFields.tcpListener, out l_readAddress)) {
          Debug.Error("Config listenAddress format invalid!");
          return new IPEndPoint(IPAddress.Loopback, DEFAULT_TCP_PORT);
        }
        return new IPEndPoint(l_readAddress, this.m_configFields.tcpListenPort);
      }
    }

    public string ServerName {
      get {
        if (this.m_configFields.serverName == null || this.m_configFields.serverName == string.Empty)
          return SERVER_NAME;

        return this.m_configFields.serverName;
      }
    }

    public Configuration(string configFilename)
    {
      this.m_configFilename = configFilename;
      Debug.Log("Config file set to: {0}.", this.m_configFilename);

      /* Default field initialization. */
      this.m_currentSerializer = new DataContractJsonSerializer(typeof(Fields));
      this.m_configFields = new Fields();
    }

    public void CreateDefaults()
    {
      this.m_configFields = new Fields();
    }

    public bool LoadConfig()
    {
      Debug.Log("Using {0} as config.", this.m_configFilename);

      try {
        using (StreamReader configStream = new StreamReader(this.m_configFilename)) {
          this.m_configFields = this.m_currentSerializer.ReadObject(configStream.BaseStream) as Fields;
          if (this.m_configFields == null) {
            Debug.Warn("Can not read config fields. [{0}]", this.m_configFilename);
            return false;
          }
        }
      }
      catch (Exception e) {
        Debug.Exception(e);
        return false;
      }
      return true;
    }

    public bool SaveConfig()
    {
      Debug.Log("Using {0} as config file.", this.m_configFilename);

      try {
        using (StreamWriter configStream = new StreamWriter(this.m_configFilename)) {
          this.m_currentSerializer.WriteObject(configStream.BaseStream, this.m_configFields);
        }
      }
      catch (Exception ex) {
        Debug.Exception(ex);
        return false;
      }
      return true;
    }
  }
}
