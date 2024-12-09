namespace MockOktaClientLibrary.Models;

public class OobRequestPayload
{
    public string ChallengeHint { get; set; } = "urn:okta:params:oauth:grant-type:oob";
    public string LoginHint { get; set; }
    public string ChannelHint { get; set; } = "push"; // Default to "push"
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}
