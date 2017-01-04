// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: MessageResponseClientPublic.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace TeamNote.Protocol {

  /// <summary>Holder for reflection information generated from MessageResponseClientPublic.proto</summary>
  public static partial class MessageResponseClientPublicReflection {

    #region Descriptor
    /// <summary>File descriptor for MessageResponseClientPublic.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static MessageResponseClientPublicReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiFNZXNzYWdlUmVzcG9uc2VDbGllbnRQdWJsaWMucHJvdG8aD1B1YmxpY0tl",
            "eS5wcm90byJIChtNZXNzYWdlUmVzcG9uc2VDbGllbnRQdWJsaWMSEAoIQ2xp",
            "ZW50SWQYASABKAMSFwoDS2V5GAIgASgLMgouUHVibGljS2V5QhZIAaoCEVRl",
            "YW1Ob3RlLlByb3RvY29sYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::TeamNote.Protocol.PublicKeyReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::TeamNote.Protocol.MessageResponseClientPublic), global::TeamNote.Protocol.MessageResponseClientPublic.Parser, new[]{ "ClientId", "Key" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class MessageResponseClientPublic : pb::IMessage<MessageResponseClientPublic> {
    private static readonly pb::MessageParser<MessageResponseClientPublic> _parser = new pb::MessageParser<MessageResponseClientPublic>(() => new MessageResponseClientPublic());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<MessageResponseClientPublic> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::TeamNote.Protocol.MessageResponseClientPublicReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MessageResponseClientPublic() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MessageResponseClientPublic(MessageResponseClientPublic other) : this() {
      clientId_ = other.clientId_;
      Key = other.key_ != null ? other.Key.Clone() : null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MessageResponseClientPublic Clone() {
      return new MessageResponseClientPublic(this);
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

    /// <summary>Field number for the "Key" field.</summary>
    public const int KeyFieldNumber = 2;
    private global::TeamNote.Protocol.PublicKey key_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::TeamNote.Protocol.PublicKey Key {
      get { return key_; }
      set {
        key_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as MessageResponseClientPublic);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(MessageResponseClientPublic other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ClientId != other.ClientId) return false;
      if (!object.Equals(Key, other.Key)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ClientId != 0L) hash ^= ClientId.GetHashCode();
      if (key_ != null) hash ^= Key.GetHashCode();
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
      if (key_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Key);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ClientId != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(ClientId);
      }
      if (key_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Key);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(MessageResponseClientPublic other) {
      if (other == null) {
        return;
      }
      if (other.ClientId != 0L) {
        ClientId = other.ClientId;
      }
      if (other.key_ != null) {
        if (key_ == null) {
          key_ = new global::TeamNote.Protocol.PublicKey();
        }
        Key.MergeFrom(other.Key);
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
          case 18: {
            if (key_ == null) {
              key_ = new global::TeamNote.Protocol.PublicKey();
            }
            input.ReadMessage(key_);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
