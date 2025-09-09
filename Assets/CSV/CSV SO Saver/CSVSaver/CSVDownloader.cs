using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEditor;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

namespace SCR
{
    public class CSVDownloader
    {
        // CSV 다운로드용 URL
        private static string csvUrl = "https://docs.google.com/spreadsheets/d/1xcGCea-eQcnl4eSx3U0PcOP8J5t4lvFuvpFnosF8WMc/export?format=csv&gid=";

        // 파일 저장 경로 (유니티 에디터의 Assets 폴더 내)
        private static string savePath = "Assets/CSV/";
        private static string sheetsIDPath = "/SheetsID.assetSO Saver/CSVSaver/SheetsID.asset";
        private static SheetsID saveType;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("PangTastic/Download CSV")] // 메뉴 경로
        public static void StartDownloadCSVFile()
        {
            AsyncOperationHandle<SheetsID> handle = Addressables.LoadAssetAsync<SheetsID>(sheetsIDPath);
            handle.Completed += OnSheetsIDLoaded;
        }
#endif

        private static void OnSheetsIDLoaded(AsyncOperationHandle<SheetsID> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                saveType = handle.Result;
                Debug.Log($"SheetsID 로드 성공:{saveType}");
                foreach (SheetsID.SheetInfo type in saveType.sheetInfos)
                {
                    DownloadCSVFile(type);
                }
            }
            else
            {
                Debug.LogError($"SheetsID 로드 실패:{handle.OperationException}");
            }
        }

        private static void DownloadCSVFile(SheetsID.SheetInfo type)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(csvUrl + type.id);

            // 요청을 보내고 응답을 기다립니다.

            UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();
            while (!asyncOperation.isDone)
            {

            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // 성공적으로 다운로드했을 때
                string csvContent = webRequest.downloadHandler.text;

                // 파일 경로에서 디렉토리가 없으면 생성
                string directoryPath = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // 파일에 내용 쓰기
                File.WriteAllText($"{savePath}{type.name}.csv", csvContent, System.Text.Encoding.UTF8);

                Debug.Log("CSV 파일 다운로드 및 저장 완료: " + $"{savePath}{type.name}.csv");
            }
            else
            {
                // 다운로드 실패 시
                Debug.LogError("파일 다운로드 실패: " + webRequest.error);
            }
        }
    }
}