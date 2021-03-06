// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: ContactUpdateStatus.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace TeamNote.Protocol {

  /// <summary>Holder for reflection information generated from ContactUpdateStatus.proto</summary>
  public static partial class ContactUpdateStatusReflection {

    #region Descriptor
    /// <summary>File descriptor for ContactUpdateStatus.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ContactUpdateStatusReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChlDb250YWN0VXBkYXRlU3RhdHVzLnByb3RvIjcKE0NvbnRhY3RVcGRhdGVT",
            "dGF0dXMSEAoIQ2xpZW50SWQYASABKAMSDgoGT25saW5lGAIgASgIQhZIAaoC",
            "EVRlYW1Ob3RlLlByb3RvY29sYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::TeamNote.Protocol.ContactUpdateStatus), global::TeamNote.Protocol.ContactUpdateStatus.Parser, new[]{ "ClientId", "Online" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class ContactUpdateStatus : pb::IMessage<ContactUpdateStatus> {
    private static readonly pb::MessageParser<ContactUpdateStatus> _parser = new pb::MessageParser<ContactUpdateStatus>(() => new ContactUpdateStatus());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<ContactUpdateStatus> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::TeamNote.Protocol.ContactUpdateStatusReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ContactUpdateStatus() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ContactUpdateStatus(ContactUpdateStatus other) : this() {
      clientId_ = other.clientId_;
      online_ = other.online_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ContactUpdateStatus Clone() {
      return new ContactUpdateStatus(this);
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

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as ContactUpdateStatus);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(ContactUpdateStatus other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ClientId != other.ClientId) return false;
      if (Online != other.Online) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ClientId != 0L) hash ^= ClientId.GetHashCode();
      if (Online != false) hash ^= Online.GetHashCode();
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
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(ContactUpdateStatus other) {
      if (other == null) {
        return;
      }
      if (other.ClientId != 0L) {
        ClientId = other.ClientId;
      }
      if (other.Online != false) {
        Online = other.Online;
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
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
