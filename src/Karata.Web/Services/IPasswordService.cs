#nullable enable

namespace Karata.Web.Services;

public interface IPasswordService
{
    byte[] HashPassword(byte[] password, byte[] salt);
    bool VerifyPassword(byte[] password, byte[] salt, byte[] hash);
}
