namespace MockOktaClientLibrary.Models;

public class PollRequestPayload
{
    public string GrantType { get; set; } = "urn:okta:params:oauth:grant-type:oob";
    public string AcrValues { get; set; } = "urn:okta:app:mfa:attestation";
    public string OobCode { get; set; }
    public string Scope { get; set; } = "openid";
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}
