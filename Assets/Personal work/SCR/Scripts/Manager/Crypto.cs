using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class Crypto
{
    private static byte[] keyBytes;
    private static byte[] ivBytes;


    public static void InitializeKeys()
    {
        // 기기 고유 ID를 기반으로 해시값 생성
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        byte[] hashBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(deviceId));

        // 키와 IV를 필요한 바이트 길이로 자름
        keyBytes = new byte[32]; // AES-256 키 (32바이트)
        Array.Copy(hashBytes, keyBytes, 32);

        ivBytes = new byte[16]; // AES IV (16바이트)
        Array.Copy(hashBytes, ivBytes, 16);
    }

    public static string Encrypt(string plainText)
    {
        if (keyBytes == null || ivBytes == null)
        {
            InitializeKeys();
        }
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = ivBytes;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            return System.Convert.ToBase64String(encrypted);
        }
    }

    public static string Decrypt(string cipherText)
    {
        if (keyBytes == null || ivBytes == null)
        {
            InitializeKeys();
        }
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = ivBytes;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] cipherBytes = System.Convert.FromBase64String(cipherText);
            byte[] decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
