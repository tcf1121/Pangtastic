using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class Crypto
{
    private static string key = GenerateDeviceKey(32); // 반드시 16/24/32 글자
    private static string iv = GenerateDeviceKey(16);  // 반드시 16 글자


    public static string GenerateDeviceKey(int length)
    {
        // 기기 고유 ID를 가져와서 해시 함수를 적용
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        byte[] deviceIdBytes = Encoding.UTF8.GetBytes(deviceId);

        // SHA256 해시 함수를 사용하여 키 생성
        SHA256 sha256 = SHA256.Create();
        byte[] keyBytes = sha256.ComputeHash(deviceIdBytes);

        byte[] finalBytes = new byte[length];
        Array.Copy(keyBytes, finalBytes, length);

        // 바이트 배열을 문자열로 변환하여 키로 사용
        return BitConverter.ToString(finalBytes).Replace("-", "").ToLower();
    }

    public static string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            return System.Convert.ToBase64String(encrypted);
        }
    }

    public static string Decrypt(string cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] cipherBytes = System.Convert.FromBase64String(cipherText);
            byte[] decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
