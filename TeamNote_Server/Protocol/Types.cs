namespace TeamNote.Protocol
{
  static class MessageType
  {
    /* Discovery Messages. */
    public const int ServiceConfigurationRequest = 0x02000001;
    public const int ServiceConfigurationResponse = 0x02000002;

    /* Handshake Messages. (Unencrypted data) */
    public const int ClientHandshakeRequest = 0x04000001;
    public const int ClientHandshakeResponse = 0x04000002;

    /* Authorization Messages. */
    public const int AuthorizationRequest = 0x18000001;
    public const int AuthorizationResponse = 0x18000002;

    public static bool IsEncrypted(int t)
    {
      return ((t & 0xF8000000) == 0x18000000);
    }
  }
}