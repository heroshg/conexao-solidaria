using System.Security.Cryptography;
using System.Text;
using Identity.Application.Abstractions;
using Konscious.Security.Cryptography;

namespace Identity.Infrastructure.Security;

/// <summary>
/// Argon2id per CLAUDE.md: ~19 MiB memory, 2 iterations, 1 degree of parallelism.
/// Parameters travel with the hash ("iterations.memoryKb.parallelism.salt.hash") so they
/// can be re-tuned later (benchmark-driven) without invalidating already-stored hashes.
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int DefaultMemorySizeKb = 19 * 1024;
    private const int DefaultIterations = 2;
    private const int DefaultDegreeOfParallelism = 1;

    public string Hash(string plainPassword)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = ComputeHash(plainPassword, salt, DefaultIterations, DefaultMemorySizeKb, DefaultDegreeOfParallelism);

        return string.Join('.',
            DefaultIterations,
            DefaultMemorySizeKb,
            DefaultDegreeOfParallelism,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string plainPassword, string passwordHash)
    {
        var parts = passwordHash.Split('.');
        if (parts.Length != 5) return false;

        var iterations = int.Parse(parts[0]);
        var memorySizeKb = int.Parse(parts[1]);
        var degreeOfParallelism = int.Parse(parts[2]);
        var salt = Convert.FromBase64String(parts[3]);
        var expectedHash = Convert.FromBase64String(parts[4]);

        var actualHash = ComputeHash(plainPassword, salt, iterations, memorySizeKb, degreeOfParallelism);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static byte[] ComputeHash(
        string plainPassword, byte[] salt, int iterations, int memorySizeKb, int degreeOfParallelism)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(plainPassword))
        {
            Salt = salt,
            Iterations = iterations,
            MemorySize = memorySizeKb,
            DegreeOfParallelism = degreeOfParallelism
        };

        return argon2.GetBytes(HashSize);
    }
}
