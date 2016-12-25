using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TeamNote.Protocol;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace TeamNote.Client
{
  class NetworkPacketZ
  {
    private Header m_packetHeader;
    private IMessage m_packetBody;

    public IMessage PacketBody {
      get {
        return this.m_packetBody;
      }
      set {
        if (this.m_packetHeader == null) {
          Debug.Warn("Can't set body size into Packet Header.");
          return;
        }

        this.m_packetBody = value;
        this.m_packetHeader.Size = this.m_packetBody.CalculateSize();
        Debug.Log("Body set, size={0}.", this.m_packetHeader.Size);
      }
    }

    public NetworkPacketZ()
    {
      this.m_packetHeader = null;
      this.m_packetBody = null;
    }

    public NetworkPacketZ(Header h, IMessage b)
    {
      this.m_packetBody = b;
      this.m_packetHeader = h;

      if (this.m_packetHeader.Size == 0) {
        this.m_packetHeader.Size = this.m_packetBody.CalculateSize();
      }
    }

    public void CreateHeader(uint messageType)
    {
      if (this.m_packetHeader != null) {
        Debug.Warn("Overriding header Type={0}.", this.m_packetHeader.Type);
      }

      this.m_packetHeader = new Header();
      this.m_packetHeader.Type = messageType;
      Debug.Log("Created header Type={0}.", m_packetHeader.Type);
    }

    public byte[] Encode()
    {
      // Any.Pack(this.m_packetBody);

      // this.m_packetHeader.Body
      return null;
    }

    public bool Decode(byte[] buffer)
    {
      return false;
    }

  }
}
