namespace TeamNote.Protocol
{
  static class MessageType
  {
    /* Discovery Messages. */
    public const int ServiceConfigurationRequest = 0x01;
    public const int ServiceConfigurationResponse = 0x02;

    /* Handshake Messages. (Unencrypted data) */
    public const int ClientHandshakeRequest = 0x03;
    public const int ClientHandshakeResponse = 0x04;

    /* Authorization Messages. */
    public const int AuthorizationRequest = 0x05;
    public const int AuthorizationResponse = 0x06;

    /* Contacts Messages. */
    public const int ContactUpdateRequest = 0x07;
    public const int ContactUpdateStatus = 0x08;
    public const int ContactUpdate = 0x09;
    public const int ContactUpdateChangeRequest = 0x0A;
  }
}
