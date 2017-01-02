// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: AuthorizationResponse.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace TeamNote.Protocol {

  /// <summary>Holder for reflection information generated from AuthorizationResponse.proto</summary>
  public static partial class AuthorizationResponseReflection {

    #region Descriptor
    /// <summary>File descriptor for AuthorizationResponse.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static AuthorizationResponseReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChtBdXRob3JpemF0aW9uUmVzcG9uc2UucHJvdG8iKwoVQXV0aG9yaXphdGlv",
            "blJlc3BvbnNlEhIKClNlcnZlck5hbWUYASABKAlCFkgBqgIRVGVhbU5vdGUu",
            "UHJvdG9jb2xiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::TeamNote.Protocol.AuthorizationResponse), global::TeamNote.Protocol.AuthorizationResponse.Parser, new[]{ "ServerName" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class AuthorizationResponse : pb::IMessage<AuthorizationResponse> {
    private static readonly pb::MessageParser<AuthorizationResponse> _parser = new pb::MessageParser<AuthorizationResponse>(() => new AuthorizationResponse());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<AuthorizationResponse> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::TeamNote.Protocol.AuthorizationResponseReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public AuthorizationResponse() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public AuthorizationResponse(AuthorizationResponse other) : this() {
      serverName_ = other.serverName_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public AuthorizationResponse Clone() {
      return new AuthorizationResponse(this);
    }

    /// <summary>Field number for the "ServerName" field.</summary>
    public const int ServerNameFieldNumber = 1;
    private string serverName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ServerName {
      get { return serverName_; }
      set {
        serverName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as AuthorizationResponse);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(AuthorizationResponse other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ServerName != other.ServerName) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ServerName.Length != 0) hash ^= ServerName.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (ServerName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ServerName);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ServerName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ServerName);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(AuthorizationResponse other) {
      if (other == null) {
        return;
      }
      if (other.ServerName.Length != 0) {
        ServerName = other.ServerName;
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
          case 10: {
            ServerName = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
