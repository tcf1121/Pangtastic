#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class EncryptFileEditors
{
    // ===== Text =====
    [MenuItem("Tools/Encrypt/Text File (기본 위치)")]
    private static void EncryptText_Default()
    {
        EncryptText(Application.streamingAssetsPath, true);
    }

    [MenuItem("Tools/Encrypt/Text File (경로 선택)")]
    private static void EncryptText_Custom()
    {
        string folder = EditorUtility.OpenFolderPanel("저장할 위치 선택", Application.dataPath, "");
        if (!string.IsNullOrEmpty(folder))
            EncryptText(folder, true);
    }

    private static void EncryptText(string outputFolder, bool deleteOriginal)
    {
        string inputPath = EditorUtility.OpenFilePanel("암호화할 Text 파일 선택", Application.dataPath, "txt,csv,json");
        if (string.IsNullOrEmpty(inputPath))
            return;

        string fileName = Path.GetFileNameWithoutExtension(inputPath);
        string outputPath = Path.Combine(outputFolder, $"{fileName}.bytes");

        // CryptoManager 사용
        Manager.Crypto.EncryptTextFile(inputPath, outputPath);

        Debug.Log($"암호화 완료 → {outputPath}");
        //  암호화 후 파일 삭제
        if (deleteOriginal)
        {
            TryDeleteOriginal(inputPath);
        }
        
        AssetDatabase.Refresh();
    }

    // ===== Binary =====
    [MenuItem("Tools/Encrypt/Binary File (기본 위치)")]
    private static void EncryptBinary_Default()
    {
        EncryptBinary(Application.streamingAssetsPath, true);
    }

    [MenuItem("Tools/Encrypt/Binary File (경로 선택)")]
    private static void EncryptBinary_Custom()
    {
        string folder = EditorUtility.OpenFolderPanel("저장할 위치 선택", Application.dataPath, "");
        if (!string.IsNullOrEmpty(folder))
            EncryptBinary(folder, true);
    }

    private static void EncryptBinary(string outputFolder, bool deleteOriginal)
    {
        string inputPath = EditorUtility.OpenFilePanel("암호화할 바이너리 파일 선택", Application.dataPath, "*");
        if (string.IsNullOrEmpty(inputPath))
            return;

        string fileName = Path.GetFileNameWithoutExtension(inputPath);
        string outputPath = Path.Combine(outputFolder, $"{fileName}.bytes");

        byte[] rawData = File.ReadAllBytes(inputPath);
        byte[] encrypted = CryptoUtility.BinaryEncryptUtil(rawData);
        File.WriteAllBytes(outputPath, encrypted);

        Debug.Log($"바이너리 암호화 완료 → {outputPath}");
        
        // 암호화 후 파일 제거
        if (deleteOriginal)
        {
            TryDeleteOriginal(inputPath);
        }
        
        
        AssetDatabase.Refresh();
    }
    
    // ========== 공통: 원본 삭제 함수 ==========
    private static void TryDeleteOriginal(string inputPath)
    {
        try
        {
            if (File.Exists(inputPath))
            {
                File.Delete(inputPath);
                Debug.Log($"원본 파일 삭제 완료: {inputPath}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"원본 파일 삭제 실패: {ex.Message}");
        }
    }
}
#endif