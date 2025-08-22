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

        public GemType[,] dataToSave = new GemType[,]
    {
        { GemType.Strawberry, GemType.Cloche, GemType.Sugar },
        { GemType.Strawberry, GemType.Strawberry, GemType.Strawberry },
        { GemType.Strawberry, GemType.Strawberry, GemType.Strawberry }
    };
        string path = "Personal work/SCR/StageInfo.csv";

        [SerializeField]
        public class Stage
        {
            public int Num;
            public int Size;
            public GemType[,] MapArray;

            public void SetStage(string num, string size, string map)
            {
                Num = int.Parse(num);
                Size = int.Parse(size);
                MapArray = GetMap(Size, map);
            }

            private GemType[,] GetMap(int size, string value)
            {
                GemType[,] map = new GemType[size, size];
                string[] gem = value.Split('|');
                int i = 0;
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        map[y, x] = (GemType)Enum.Parse(typeof(GemType), gem[i++]);
                    }
                }
                return map;
            }
        }

        private void SortAndCreateArray()
        {
            // 1. 딕셔너리의 모든 키-값 쌍을 가져옵니다.
            // 이 시점에도 순서는 보장되지 않습니다.
            var unsortedPairs = Board.GetDictionary();

            // 2. LINQ의 OrderBy를 사용하여 Vector3Int 키를 기준으로 정렬합니다.
            // 사전식 순서: X -> Y -> Z 순으로 비교합니다.
            // 그리고 .Select()를 사용하여 각 키-값 쌍에서 '값(Item)'만 추출합니다.
            var sortedItemsList = unsortedPairs
                .OrderBy(pair => pair.Key.x) // 1차: X 값으로 정렬
                .ThenBy(pair => pair.Key.y) // 2차: X 값이 같으면 Y 값으로 정렬
                .ThenBy(pair => pair.Key.z) // 3차: Y 값까지 같으면 Z 값으로 정렬
                .Select(pair => pair.Value) // 정렬된 순서대로 'Item' 객체만 선택
                .ToList(); // 결과를 List<Item>으로 변환

            // 3. 리스트를 배열로 변환
            sortedItemsArray = sortedItemsList.ToArray();

            Debug.Log("딕셔너리를 정렬하여 배열로 생성 완료!");
        }


        public void SaveMapInfo()
        {
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
                if (num > lines.Count)
                {
                    for (int i = lines.Count; i < num; i++)
                    {
                        lines.Add(" ");
                    }
                }

                StringBuilder csvBuilder = new StringBuilder();

                // 배열의 각 행을 순회
                csvBuilder.Append(num);
                csvBuilder.Append(",");
                csvBuilder.Append(dataToSave.GetLength(0));
                csvBuilder.Append(",");
                for (int i = 0; i < dataToSave.GetLength(0); i++)
                {

                    // 각 행의 열을 순회하며 쉼표로 구분하여 추가
                    for (int j = 0; j < dataToSave.GetLength(1); j++)
                    {
                        csvBuilder.Append(dataToSave[i, j]);
                        if (j < dataToSave.GetLength(1) - 1)
                        {
                            csvBuilder.Append("|");
                        }
                    }
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
                Debug.Log(splitData);

                Stage loadstage = new();
                loadstage.SetStage(values[0], values[1], values[2]);
                Debug.Log(loadstage.Num);
                Debug.Log(loadstage.Size);
                Debug.Log(loadstage.MapArray[0, 0]);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading CSV file: {e.Message}");
            }
        }


    }
}
