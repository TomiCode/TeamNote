using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace TeamNote.Client
{
  class Configuration
  {
    public const int INVALID_UDP_PORT = -1;

    [DataContract]
    class Fields
    {
      [DataMember(IsRequired = true, Name = "udpBroadcastPort")]
      public int udpConfig { get; set; }

      [DataMember(IsRequired = false, Name = "clientLocale")]
      public string localeOverride { get; set; }

      [IgnoreDataMember]
      public bool HasLocaleOverride {
        get {
          return (this.localeOverride != "");
        }
      }

      public Fields()
      {
        Debug.Log("Creating new config fields.");

        this.udpConfig = 1337;
        this.localeOverride = "";
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

    public int UDP_Port {
      get {
        if (!this.ConfigLoaded) {
          Debug.Warn("Config not properly loaded.");
          return INVALID_UDP_PORT;
        }
        return this.m_configFields.udpConfig;
      }
    }

    public bool HasLocaleOverride {
      get {
        if (!this.ConfigLoaded) {
          Debug.Warn("Config not properly loaded.");
          return false;
        }
        return this.m_configFields.HasLocaleOverride;
      }
    }

    public string CurrentLocale {
      get {
        if (!this.ConfigLoaded) {
          Debug.Warn("Config not properly loaded.");
          return "";
        }
        return this.m_configFields.localeOverride;
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
