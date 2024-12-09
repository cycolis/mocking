namespace MockOktaClientLibrary.Models;

public class TokenRequestPayload
{
    public string GrantType { get; set; } = "password";
    public string AcrValues { get; set; } = "urn:okta:app:mfa:attestation";
    public string Username { get; set; }
    public string Password { get; set; }
    public string Scope { get; set; } = "openid";
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}
