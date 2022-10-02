using System.Security.Cryptography;
using Konscious.Security.Cryptography;

namespace Karata.Server.Services;

public class PasswordService : IPasswordService
{
    public byte[] HashPassword(byte[] password, byte[] salt)
    {
        using var argon2 = new Argon2id(password);
        argon2.Salt = salt;
        argon2.DegreeOfParallelism = 4;
        argon2.Iterations = 2;
        argon2.MemorySize = 1024;
        return argon2.GetBytes(32);
    }

    public bool VerifyPassword(byte[] password, byte[] salt, byte[] hash) =>
        HashPassword(password, salt).SequenceEqual(hash);

    public static byte[] GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[32];
        rng.GetBytes(buffer);
        return buffer;
    }
}