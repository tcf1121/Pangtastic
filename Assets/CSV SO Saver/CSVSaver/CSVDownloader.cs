using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace SCR
{
    public class CSVDownloader : MonoBehaviour
    {
        // CSV 다운로드용 URL
        private string csvUrl = "https://docs.google.com/spreadsheets/d/1xcGCea-eQcnl4eSx3U0PcOP8J5t4lvFuvpFnosF8WMc/export?format=csv&gid=";

        // 파일 저장 경로 (유니티 에디터의 Assets 폴더 내)
        private string savePath = "Assets/CSV/";

        [SerializeField] SheetsID saveType;

        void Start()
        {
            // 코루틴을 사용하여 웹에서 파일을 다운로드합니다.

            foreach (SheetsID.SheetInfo type in saveType.sheetInfos)
            {
                StartCoroutine(DownloadCSVFile(type));
            }

        }

        IEnumerator DownloadCSVFile(SheetsID.SheetInfo type)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(csvUrl + type.id);

            // 요청을 보내고 응답을 기다립니다.
            yield return webRequest.SendWebRequest();

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