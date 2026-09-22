namespace Identity.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "ConexaoSolidaria.Identity";
    public string Audience { get; set; } = "ConexaoSolidaria";
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Local path to the RSA private key (PEM). Dev: auto-generated on first run if missing
    /// (see RsaKeyProvider). K8s: must be mounted from a Secret instead.
    /// </summary>
    public string PrivateKeyPath { get; set; } = "keys/jwt-private.pem";
}
