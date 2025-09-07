using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;

public class JGH_TEST_CRYPT : MonoBehaviour
{
    [System.Serializable]
    public class JGH_TEST_UserInfo {
        public int Coin;
        public int Star;
    }

    [System.Serializable]
    public class JGH_TEST_PlayerData {
        public string PlayerName;
        public int Stage;
        public UserInfo UserInfo;
    }
    
    void Start()
    {
        // ===== 1. 텍스트(CSV 등) =====
        string textInput = "Assets/StreamingAssets/dialog.csv";
        string textOutput = "Assets/StreamingAssets/dialog.bytes";

        // 암호화
        CryptoUtility.TextEncryptFile(textInput, textOutput, false); // true 면 원본 제거

        // 복호화 (파일에서 복구)
        string decryptedText = CryptoUtility.TextDecrypt(File.ReadAllBytes(textOutput));
        Debug.Log("복호화된 텍스트: " + decryptedText);
        
        
        
        // ===== 1. 텍스트(JSON) =====
        string jsonInput  = "Assets/StreamingAssets/test.json";
        string jsonOutput = "Assets/StreamingAssets/test.bytes";

        // 암호화 
        CryptoUtility.TextEncryptFile(jsonInput, jsonOutput, false);

        // 복호화
        string decrypted = CryptoUtility.TextDecrypt(File.ReadAllBytes(jsonOutput));
        // JSON → 객체 변환
        JGH_TEST_PlayerData data = JsonUtility.FromJson<JGH_TEST_PlayerData>(decrypted);
        Debug.Log(data.UserInfo);
        Debug.Log(data.PlayerName);
        Debug.Log(data.Stage);

        
        
        
        
        // ===== 2. 바이너리(CSV) =====
        string binInput = "Assets/StreamingAssets/string.csv";
        string binOutput = "Assets/StreamingAssets/string.bytes";

        // 복호화 (파일에서 읽기)
        byte[] encryptedFromFile = File.ReadAllBytes(textOutput);
        string decryptedText1 = CryptoUtility.TextDecrypt(encryptedFromFile);

        // 원본과 복호화 결과 비교

        // 실제 출력 확인
        Debug.Log("복호화된 텍스트1: " + decryptedText1);
        
    }
}
