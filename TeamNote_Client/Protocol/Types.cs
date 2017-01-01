namespace TeamNote.Protocol
{
  static class MessageType
  {
    /* Discovery Messages. */
    public const int ServiceConfigurationRequest = 0x01;
    public const int ServiceConfigurationResponse = 0x02;

    /* Handshake Messages. (Unencrypted data) */
    public const int ClientHandshakeRequest = 0x04;
    public const int ClientHandshakeResponse = 0x08;

    /* Authorization Messages. */
    public const int AuthorizationRequest = 0x10;
    public const int AuthorizationResponse = 0x11;
  }
}
