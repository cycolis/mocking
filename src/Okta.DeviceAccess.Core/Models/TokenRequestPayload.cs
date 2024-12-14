using System.Text.Json.Serialization;

namespace Okta.DeviceAccess.Core.Models;

public class TokenRequestPayload
{
    /// <summary>
    /// password or urn:okta:params:oauth:grant-type:oob
    /// </summary>
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; }

    [JsonPropertyName("acr_values")]
    public string AcrValues { get; set; } = "urn:okta:app:mfa:attestation";

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("oob_code")]
    public string OobCode { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = "openid";

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }
}
