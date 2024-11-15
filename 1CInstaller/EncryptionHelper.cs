using System;
using System.Text;
using System.Security.Cryptography;

public static class EncryptionHelper
{
    public static string EncryptString(string plainText)
    {
        byte[] encryptedData = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(plainText),
            null,
            DataProtectionScope.CurrentUser);

        return Convert.ToBase64String(encryptedData);
    }

    public static string DecryptString(string encryptedText)
    {
        byte[] decryptedData = ProtectedData.Unprotect(
            Convert.FromBase64String(encryptedText),
            null,
            DataProtectionScope.CurrentUser);

        return Encoding.UTF8.GetString(decryptedData);
    }
}
