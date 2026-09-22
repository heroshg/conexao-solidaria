using System.Security.Cryptography;

namespace Identity.Infrastructure.Security;

public static class RsaKeyProvider
{
    public static RSA LoadOrCreatePrivateKey(string privateKeyPath)
    {
        var rsa = RSA.Create(2048);

        if (File.Exists(privateKeyPath))
        {
            rsa.ImportFromPem(File.ReadAllText(privateKeyPath));
            return rsa;
        }

        // Dev convenience only: generate and persist a keypair on first run so Identity.Api
        // and Campaigns.Api can start without a manual key-provisioning step. In K8s, mount
        // both PEMs from a Secret instead of relying on this fallback (see /deploy/identity-api).
        var directory = Path.GetDirectoryName(privateKeyPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        File.WriteAllText(privateKeyPath, rsa.ExportRSAPrivateKeyPem());

        var publicKeyPath = Path.Combine(string.IsNullOrEmpty(directory) ? "." : directory, "jwt-public.pem");
        File.WriteAllText(publicKeyPath, rsa.ExportRSAPublicKeyPem());

        return rsa;
    }
}
