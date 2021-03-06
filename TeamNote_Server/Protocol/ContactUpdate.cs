// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: ContactUpdate.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace TeamNote.Protocol {

  /// <summary>Holder for reflection information generated from ContactUpdate.proto</summary>
  public static partial class ContactUpdateReflection {

    #region Descriptor
    /// <summary>File descriptor for ContactUpdate.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ContactUpdateReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChNDb250YWN0VXBkYXRlLnByb3RvIo4BCg1Db250YWN0VXBkYXRlEiIKA0Fk",
            "ZBgBIAMoCzIVLkNvbnRhY3RVcGRhdGUuQ2xpZW50Eg4KBlJlbW92ZRgCIAMo",
            "AxpJCgZDbGllbnQSEAoIQ2xpZW50SWQYASABKAMSDgoGT25saW5lGAIgASgI",
            "EgwKBE5hbWUYAyABKAkSDwoHU3VybmFtZRgEIAEoCUIWSAGqAhFUZWFtTm90",
            "ZS5Qcm90b2NvbGIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::TeamNote.Protocol.ContactUpdate), global::TeamNote.Protocol.ContactUpdate.Parser, new[]{ "Add", "Remove" }, null, null, new pbr::GeneratedClrTypeInfo[] { new pbr::GeneratedClrTypeInfo(typeof(global::TeamNote.Protocol.ContactUpdate.Types.Client), global::TeamNote.Protocol.ContactUpdate.Types.Client.Parser, new[]{ "ClientId", "Online", "Name", "Surname" }, null, null, null)})
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class ContactUpdate : pb::IMessage<ContactUpdate> {
    private static readonly pb::MessageParser<ContactUpdate> _parser = new pb::MessageParser<ContactUpdate>(() => new ContactUpdate());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<ContactUpdate> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::TeamNote.Protocol.ContactUpdateReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ContactUpdate() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ContactUpdate(ContactUpdate other) : this() {
      add_ = other.add_.Clone();
      remove_ = other.remove_.Clone();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ContactUpdate Clone() {
      return new ContactUpdate(this);
    }

    /// <summary>Field number for the "Add" field.</summary>
    public const int AddFieldNumber = 1;
    private static readonly pb::FieldCodec<global::TeamNote.Protocol.ContactUpdate.Types.Client> _repeated_add_codec
        = pb::FieldCodec.ForMessage(10, global::TeamNote.Protocol.ContactUpdate.Types.Client.Parser);
    private readonly pbc::RepeatedField<global::TeamNote.Protocol.ContactUpdate.Types.Client> add_ = new pbc::RepeatedField<global::TeamNote.Protocol.ContactUpdate.Types.Client>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::TeamNote.Protocol.ContactUpdate.Types.Client> Add {
      get { return add_; }
    }

    /// <summary>Field number for the "Remove" field.</summary>
    public const int RemoveFieldNumber = 2;
    private static readonly pb::FieldCodec<long> _repeated_remove_codec
        = pb::FieldCodec.ForInt64(18);
    private readonly pbc::RepeatedField<long> remove_ = new pbc::RepeatedField<long>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<long> Remove {
      get { return remove_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as ContactUpdate);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(ContactUpdate other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!add_.Equals(other.add_)) return false;
      if(!remove_.Equals(other.remove_)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= add_.GetHashCode();
      hash ^= remove_.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      add_.WriteTo(output, _repeated_add_codec);
      remove_.WriteTo(output, _repeated_remove_codec);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += add_.CalculateSize(_repeated_add_codec);
      size += remove_.CalculateSize(_repeated_remove_codec);
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(ContactUpdate other) {
      if (other == null) {
        return;
      }
      add_.Add(other.add_);
      remove_.Add(other.remove_);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            add_.AddEntriesFrom(input, _repeated_add_codec);
            break;
          }
          case 18:
          case 16: {
            remove_.AddEntriesFrom(input, _repeated_remove_codec);
            break;
          }
        }
      }
    }

    #region Nested types
    /// <summary>Container for nested types declared in the ContactUpdate message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static partial class Types {
      public sealed partial class Client : pb::IMessage<Client> {
        private static readonly pb::MessageParser<Client> _parser = new pb::MessageParser<Client>(() => new Client());
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pb::MessageParser<Client> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::TeamNote.Protocol.ContactUpdate.Descriptor.NestedTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Client() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Client(Client other) : this() {
          clientId_ = other.clientId_;
          online_ = other.online_;
          name_ = other.name_;
          surname_ = other.surname_;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Client Clone() {
          return new Client(this);
        }

        /// <summary>Field number for the "ClientId" field.</summary>
        public const int ClientIdFieldNumber = 1;
        private long clientId_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public long ClientId {
          get { return clientId_; }
          set {
            clientId_ = value;
          }
        }

        /// <summary>Field number for the "Online" field.</summary>
        public const int OnlineFieldNumber = 2;
        private bool online_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Online {
          get { return online_; }
          set {
            online_ = value;
          }
        }

        /// <summary>Field number for the "Name" field.</summary>
        public const int NameFieldNumber = 3;
        private string name_ = "";
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string Name {
          get { return name_; }
          set {
            name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        /// <summary>Field number for the "Surname" field.</summary>
        public const int SurnameFieldNumber = 4;
        private string surname_ = "";
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string Surname {
          get { return surname_; }
          set {
            surname_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
          return Equals(other as Client);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(Client other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (ClientId != other.ClientId) return false;
          if (Online != other.Online) return false;
          if (Name != other.Name) return false;
          if (Surname != other.Surname) return false;
          return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
          int hash = 1;
          if (ClientId != 0L) hash ^= ClientId.GetHashCode();
          if (Online != false) hash ^= Online.GetHashCode();
          if (Name.Length != 0) hash ^= Name.GetHashCode();
          if (Surname.Length != 0) hash ^= Surname.GetHashCode();
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(pb::CodedOutputStream output) {
          if (ClientId != 0L) {
            output.WriteRawTag(8);
            output.WriteInt64(ClientId);
          }
          if (Online != false) {
            output.WriteRawTag(16);
            output.WriteBool(Online);
          }
          if (Name.Length != 0) {
            output.WriteRawTag(26);
            output.WriteString(Name);
          }
          if (Surname.Length != 0) {
            output.WriteRawTag(34);
            output.WriteString(Surname);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
          int size = 0;
          if (ClientId != 0L) {
            size += 1 + pb::CodedOutputStream.ComputeInt64Size(ClientId);
          }
          if (Online != false) {
            size += 1 + 1;
          }
          if (Name.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
          }
          if (Surname.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(Surname);
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(Client other) {
          if (other == null) {
            return;
          }
          if (other.ClientId != 0L) {
            ClientId = other.ClientId;
          }
          if (other.Online != false) {
            Online = other.Online;
          }
          if (other.Name.Length != 0) {
            Name = other.Name;
          }
          if (other.Surname.Length != 0) {
            Surname = other.Surname;
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(pb::CodedInputStream input) {
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                input.SkipLastField();
                break;
              case 8: {
                ClientId = input.ReadInt64();
                break;
              }
              case 16: {
                Online = input.ReadBool();
                break;
              }
              case 26: {
                Name = input.ReadString();
                break;
              }
              case 34: {
                Surname = input.ReadString();
                break;
              }
            }
          }
        }

      }

    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
