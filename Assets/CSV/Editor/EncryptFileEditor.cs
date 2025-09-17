#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class EncryptFileEditors
{
    // ===== Text =====
    // [MenuItem("Tools/Encrypt/Text File (streamingAssetsPath)")]
    [MenuItem("PangTastic/Encrypt/Text File (streamingAssetsPath)")]
    private static void EncryptText_Default()
    {
        TextEncrypt(Application.streamingAssetsPath);
    }

    // [MenuItem("Tools/Encrypt/Text File (경로 선택)")]
    [MenuItem("PangTastic/Encrypt/Text File (경로 선택)")]
    private static void EncryptText_Custom()
    {
        string folder = EditorUtility.OpenFolderPanel("저장할 위치 선택", Application.dataPath, "");
        if (!string.IsNullOrEmpty(folder))
            TextEncrypt(folder);
    }


    private static void TextEncrypt(string outputFolder)
    {
        string inputPath = EditorUtility.OpenFilePanel("암호화할 Text 파일 선택", Application.dataPath, "txt,csv,json");
        if (string.IsNullOrEmpty(inputPath))
            return;

        string fileName = Path.GetFileNameWithoutExtension(inputPath);
        string outputPath = Path.Combine(outputFolder, $"{fileName}.bytes");

        // CryptoManager 사용
        //CryptoUtility.TextEncryptFile(inputPath, outputPath);
        // Debug.Log($"암호화 완료 → {outputPath}");
        AssetDatabase.Refresh();
    }


    // ===== Binary =====
    // [MenuItem("Tools/Encrypt/Binary File (streamingAssetsPath)")]
    [MenuItem("PangTastic/Encrypt/Binary File (streamingAssetsPath)")]
    private static void EncryptBinary_Default()
    {
        EncryptBinary(Application.streamingAssetsPath);
    }

    // [MenuItem("Tools/Encrypt/Binary File (경로 선택)")]
    [MenuItem("PangTastic/Encrypt/Binary File (경로 선택)")]
    private static void EncryptBinary_Custom()
    {
        string folder = EditorUtility.OpenFolderPanel("저장할 위치 선택", Application.dataPath, "");
        if (!string.IsNullOrEmpty(folder))
            EncryptBinary(folder);
    }
    private static void EncryptBinary(string outputFolder)
    {
        string inputPath = EditorUtility.OpenFilePanel("암호화할 바이너리 파일 선택", Application.dataPath, "*");
        if (string.IsNullOrEmpty(inputPath))
            return;

        string fileName = Path.GetFileNameWithoutExtension(inputPath);
        string outputPath = Path.Combine(outputFolder, $"{fileName}.bytes");

        //byte[] encrypted = CryptoUtility.BinaryEncrypt(inputPath, outputPath);
        // Debug.Log($"바이너리 암호화 완료 → {encrypted}");
        AssetDatabase.Refresh();
    }
}
#endif