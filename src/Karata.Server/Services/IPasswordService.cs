using System.Security.Cryptography;

namespace Karata.Server.Services;

public interface IPasswordService
{
    byte[] HashPassword(byte[] password, byte[] salt);
    bool VerifyPassword(byte[] password, byte[] salt, byte[] hash);
    
    public static byte[] GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[32];
        rng.GetBytes(buffer);
        return buffer;
    }
}
