using Konscious.Security.Cryptography;

namespace Karata.Server.Services;

public class Argon2PasswordService : IPasswordService
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
}