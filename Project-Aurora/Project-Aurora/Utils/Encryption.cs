using System.Security.Cryptography;
using System.Text;

namespace AuroraRgb.Utils;

internal static class Encryption
{
    public static byte[] Encrypt(string plainText)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        return encryptedBytes;
    }

    // Decrypt a password using DPAPI
    public static string Decrypt(byte[] encryptedBytes)
    {
        var plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plainBytes);
    }
}