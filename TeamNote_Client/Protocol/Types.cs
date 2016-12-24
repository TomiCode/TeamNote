using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamNote.Protocol
{
  static class MessageType
  {
    public const uint BroadcastClientRequest = 0x7F000001;
    public const uint BroadcastServerResponse = 0x7F000002;
  }

  static class MessageSize
  {
    public const uint NoData = 0x00;
  }
}
