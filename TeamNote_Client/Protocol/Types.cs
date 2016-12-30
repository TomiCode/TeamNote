using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamNote.Protocol
{
  static class MessageType
  {
    /* Discovery Messages. */
    public const int ServiceConfigurationRequest = 0x7F000001;
    public const int ServiceConfigurationResponse = 0x7F000002;

    /* Handshake Messages. (Unencrypted data) */
    public const int ClientHandshakeRequest = 0x04000001;
    public const int ClientHandshakeResponse = 0x04000002;

    public static bool IsEncrypted(int t)
    {
      return ((t & 0xF0000000) == 0x80000000);
    }
  }
}
