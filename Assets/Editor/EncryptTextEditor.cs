using UnityEditor;
using UnityEngine;
using System.IO;

public class EncryptTextEditor
{
    [MenuItem("Tools/Encrypt Text File")]
    private static void EncryptFile()
    {
        // 파일 선택창 열기
        string inputPath = EditorUtility.OpenFilePanel("암호화할 Text 파일 선택", Application.dataPath, "txt,csv,json");

        if (string.IsNullOrEmpty(inputPath))
            return;

        string fileName = Path.GetFileNameWithoutExtension(inputPath);
        string outputPath = Path.Combine(Application.streamingAssetsPath, $"{fileName}.bytes");

        // CryptoManager 사용
        CryptoUtility.EncryptTextFile(inputPath, outputPath);

        // 원본 파일 삭제
        try
        {
            File.Delete(inputPath);
            Debug.Log($"암호화 완료 후 원본 제거\n결과: {outputPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"원본 파일 삭제 실패: {ex.Message}");
        } 
        
        AssetDatabase.Refresh(); // Unity 에셋 갱신
    }
}