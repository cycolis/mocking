using System.Text.Json.Serialization;

namespace Okta.DeviceAccess.Core.Models;

public class OobRequestPayload
{
    [JsonPropertyName("challenge_hint")]
    public string ChallengeHint { get; set; } = "urn:okta:params:oauth:grant-type:oob";

    [JsonPropertyName("login_hint")]
    public string LoginHint { get; set; }

    [JsonPropertyName("channel_hint")]
    public string ChannelHint { get; set; } = "push"; // Default to "push"

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }
}
