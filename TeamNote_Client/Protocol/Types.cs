namespace TeamNote.Protocol
{
  static class MessageType
  {
    /* Discovery Messages. */
    public const int ServiceConfigurationRequest = 1;
    public const int ServiceConfigurationResponse = 2;

    /* Handshake Messages. (Unencrypted data) */
    public const int ClientHandshakeRequest = 3;
    public const int ClientHandshakeResponse = 4;

    /* Authorization Messages. */
    public const int AuthorizationRequest = 5;
    public const int AuthorizationResponse = 6;

    /* Contacts Messages. */
    public const int ContactUpdateRequest = 7;
    public const int ContactUpdateStatus = 8;
    public const int ContactUpdate = 9;
    public const int ContactUpdateChangeRequest = 10;

    /* Messages. */
    public const int MessageClientPublicRequest = 11;
    public const int MessageClientPublicResponse = 12;
  }
}
