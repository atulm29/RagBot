using System.Text.Json.Serialization;

namespace RAGSERVERAPI.Models;

public class JwtAuthResult
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refreshToken")]
    public RefreshToken RefreshToken { get; set; }
}

public class RefreshToken
{
    [JsonPropertyName("username")]
    public string UserName { get; set; }

    [JsonPropertyName("tokenString")]
    public string TokenString { get; set; }

    [JsonPropertyName("expireAt")]
    public DateTime ExpireAt { get; set; }
}

public class JwtTokenConfig
{
    [JsonPropertyName("secret")]
    public string Secret { get; set; }

    [JsonPropertyName("issuer")]
    public string Issuer { get; set; }

    [JsonPropertyName("audience")]
    public string Audience { get; set; }

    [JsonPropertyName("accessTokenExpiration")]
    public int AccessTokenExpiration { get; set; }

    [JsonPropertyName("refreshTokenExpiration")]
    public int RefreshTokenExpiration { get; set; }
}
