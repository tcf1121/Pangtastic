using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    public class MapEditor : MonoBehaviour
    {
        private BoardCell[] sortedItemsArray;
        [SerializeField] private InputField inputField;
        private Dictionary<Vector3Int, GemType> _mapInfo = new();
        private List<Vector3Int> _spawnPoint = new();

        string path = "Personal work/SCR/StageInfo.csv";


        public void SaveMapInfo()
        {
            _mapInfo.Clear();
            _mapInfo = Board.GetPuzzleInfo();
            _spawnPoint.Clear();
            _spawnPoint = Board.GetSpawnPoint();
            int num = int.Parse(inputField.text);
            string directoryPath = Path.GetDirectoryName(Application.dataPath + "/" + path);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            List<string> lines = new List<string>();

            try
            {
                // 1. CSV 파일의 모든 줄을 읽어 메모리에 로드
                lines.AddRange(File.ReadAllLines(Application.dataPath + "/" + path, Encoding.UTF8));

                // 2. 덮어쓸 대상 줄의 인덱스가 유효한지 확인
                if (num < 0)
                {
                    Debug.LogError("0보다 큰 값을 입력하시오.");
                    return;
                }
                while (lines.Count <= num)
                {
                    lines.Add(""); // 빈 줄 추가
                }

                StringBuilder csvBuilder = new StringBuilder();

                // 배열의 각 행을 순회
                csvBuilder.Append(num);
                csvBuilder.Append(",");
                foreach (var data in _mapInfo)
                {
                    csvBuilder.Append(data.Key.x);
                    csvBuilder.Append(":");
                    csvBuilder.Append(data.Key.y);
                    csvBuilder.Append(":");
                    csvBuilder.Append(data.Key.z);
                    csvBuilder.Append(":");
                    csvBuilder.Append(data.Value);
                    csvBuilder.Append("|");
                }
                csvBuilder.Append(",");
                foreach (var data in _spawnPoint)
                {
                    csvBuilder.Append(data.x);
                    csvBuilder.Append(":");
                    csvBuilder.Append(data.y);
                    csvBuilder.Append(":");
                    csvBuilder.Append(data.z);
                    csvBuilder.Append("|");
                }

                // 4. 특정 인덱스의 줄을 새 데이터로 덮어쓰기
                lines[num] = csvBuilder.ToString();
                // 5. 수정된 모든 줄을 다시 파일에 저장 (기존 파일 내용 덮어씀)
                File.WriteAllLines(Application.dataPath + "/" + path, lines.ToArray(), Encoding.UTF8);

                Debug.Log("CSV 파일의 " + num + "번째 줄에 데이터 저장 완료: " + Application.dataPath + "/" + path);
            }
            catch (IOException e)
            {
                Debug.LogError($"파일 접근 오류 발생 (Sharing Violation?): {e.Message}");
                Debug.LogError("파일이 다른 프로그램에서 열려 있는지 확인하거나, Unity를 재시작해보세요.");
            }
            catch (Exception e)
            {
                Debug.LogError($"CSV 파일 저장 중 예상치 못한 오류 발생: {e.Message}");
            }
        }

        public void ReadCSV()
        {
            int num = int.Parse(inputField.text);
            try
            {
                var splitData = File.ReadLines(Application.dataPath + "/" + path).Skip(num).Take(1).FirstOrDefault();
                string[] values = splitData.ToString().Split(',');

                GetPuzzle(values[1]);
                GetSpawnPoint(values[2]);
                Board.SetPuzzleInfo(_mapInfo, _spawnPoint);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading CSV file: {e.Message}");
            }
        }

        private void GetPuzzle(string value)
        {
            _mapInfo.Clear();
            string[] gem = value.Split('|');
            for (int i = 0; i < gem.Length - 1; i++)
            {
                string cellInfo = gem[i];
                string[] info = cellInfo.Split(':');
                _mapInfo.Add(GetVec3Int(info[0], info[1], info[2]), (GemType)Enum.Parse(typeof(GemType), info[3]));
            }
        }

        private void GetSpawnPoint(string value)
        {
            _spawnPoint.Clear();
            string[] gem = value.Split('|');
            for (int i = 0; i < gem.Length - 1; i++)
            {
                string cellInfo = gem[i];
                string[] info = cellInfo.Split(':');
                _spawnPoint.Add(GetVec3Int(info[0], info[1], info[2]));
            }
        }

        private Vector3Int GetVec3Int(string stringX, string stringY, string stringZ)
        {
            int x, y, z;
            bool xSuccess = int.TryParse(stringX.Trim(), out x);
            bool ySuccess = int.TryParse(stringY.Trim(), out y);
            bool zSuccess = int.TryParse(stringZ.Trim(), out z);

            if (xSuccess && ySuccess && zSuccess)
            {
                return new Vector3Int(x, y, z);
            }
            else
            {
                Debug.LogError($"일부 값이 정수 형식이 아닙니다.");
                return Vector3Int.zero;
            }
        }
    }
}

