using System.Security.Cryptography;

namespace HighScoreAPI.Encryption;

public static class AesOperation
{
    public static string GenerateKeyBase64()
    {
        using Aes aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();

        return Convert.ToBase64String(aes.Key);
    }

    public static string EncryptData(string plainText, string keyBase64, out string vectorBase64)
    {
        using Aes aes = Aes.Create();
        aes.Key = Convert.FromBase64String(keyBase64);
        aes.GenerateIV();

        vectorBase64 = Convert.ToBase64String(aes.IV);

        ICryptoTransform encryptor = aes.CreateEncryptor();

        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
        using StreamWriter sw = new(cs);
        sw.Write(plainText);
        sw.Close();

        byte[] encryptedData = ms.ToArray();

        return Convert.ToBase64String(encryptedData);
    }

    public static string DecryptData(string cipherText, string keyBase64, string vectorBase64)
    {
        using Aes aes = Aes.Create();
        aes.Key = Convert.FromBase64String(keyBase64);
        aes.IV = Convert.FromBase64String(vectorBase64);

        ICryptoTransform decryptor = aes.CreateDecryptor();

        byte[] cipher = Convert.FromBase64String(cipherText);

        using MemoryStream ms = new(cipher);
        using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new(cs);

        return sr.ReadToEnd();
    }
}
