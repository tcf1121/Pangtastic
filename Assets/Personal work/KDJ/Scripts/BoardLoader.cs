using SCR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KDJ
{
    public class BoardData
    {
        public bool[,] BlockPlateArray;
        public Block[,] BlockArray;
    }

    public class BoardLoader : MonoBehaviour
    {
        private string _path = "Personal work/SCR/StageInfo.csv";
        public BoardData BoardDataArray;

        public BoardData LoadBoard(string value)
        {
            try
            {
                string[] values = value.Split('|').Where(s => !string.IsNullOrEmpty(s)).ToArray();
                if (values.Length == 0)
                {
                    Debug.LogError("CSV 행에 유효한 보드 데이터가 없습니다.");
                    return new BoardData { BlockPlateArray = new bool[0, 0], BlockArray = new Block[0, 0] };
                }

                var blockInfos = values.Select(v => {
                    var parts = v.Split(':');
                    if (parts.Length < 4)
                    {
                        Debug.LogWarning($"잘못된 형식의 셀 데이터를 건너뜁니다: {v}");
                        return null;
                    }
                    return new {
                        x = int.Parse(parts[0]),
                        y = int.Parse(parts[1]),
                        gemType = (GemType)Enum.Parse(typeof(GemType), parts[3])
                    };
                }).Where(info => info != null).ToList();

                if (blockInfos.Count == 0)
                {
                    Debug.LogError("보드 데이터에서 유효한 블록 정보를 파싱하지 못했습니다.");
                    return new BoardData { BlockPlateArray = new bool[0, 0], BlockArray = new Block[0, 0] };
                }

                int xMin = blockInfos.Min(b => b.x);
                int xMax = blockInfos.Max(b => b.x);
                int yMin = blockInfos.Min(b => b.y);
                int yMax = blockInfos.Max(b => b.y);

                int plateHeight = yMax - yMin + 1;
                int plateWidth = xMax - xMin + 1;

                var boardData = new BoardData
                {
                    BlockPlateArray = new bool[plateHeight, plateWidth],
                    BlockArray = new Block[plateHeight + 1, plateWidth] // BlockArray는 한 줄 더 높습니다.
                };

                int xOffset = -xMin;
                int yOffset = -yMin;

                foreach (var info in blockInfos)
                {
                    int x = info.x + xOffset;
                    int y = info.y + yOffset;

                    if (y >= plateHeight || x >= plateWidth || y < 0 || x < 0)
                    {
                        Debug.LogWarning($"계산된 좌표 [{info.y},{info.x}]가 배열 범위를 벗어났습니다.");
                        continue;
                    }

                    boardData.BlockPlateArray[y, x] = true;
                    
                    int tempNum = 0;
                    GemType finalGemType = info.gemType;
                    int blockType;

                    if (finalGemType == GemType.Random)
                    {
                        tempNum = UnityEngine.Random.Range(1, 7);
                        finalGemType = (GemType)tempNum;
                        blockType = tempNum + 1;
                    }
                    else
                    {
                        blockType = (int)finalGemType + 1;
                    }
                    
                    // BlockArray의 해당 위치에 블록을 배치합니다.
                    boardData.BlockArray[y, x] = new Block()
                    {
                        GemType = finalGemType,
                        BlockType = blockType,
                    };
                }

                return boardData;
            }
            catch (Exception e)
            {
                Debug.LogError($"LoadBoard 오류: {e.ToString()}");
                return null;
            }
        }

        public BoardData ReadCSV(int num)
        {
            try
            {
                string filePath = Path.Combine(Application.dataPath, _path);
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"CSV 파일을 다음 경로에서 찾을 수 없습니다: {filePath}");
                    return null;
                }

                // 헤더 행을 건너뛰고, num 라인만큼 건너뜁니다 (스테이지는 1부터 시작).
                var line = File.ReadLines(filePath).Skip(num).FirstOrDefault();

                if (string.IsNullOrEmpty(line))
                {
                    Debug.LogError($"CSV 파일에 스테이지 {num}이 없거나 해당 라인이 비어 있습니다.");
                    return null;
                }

                string[] values = line.Split(',');
                if (values.Length < 2)
                {
                    Debug.LogError($"스테이지 {num}의 데이터 형식이 잘못되었습니다. 최소 2개의 열이 필요합니다.");
                    return null;
                }

                BoardDataArray = LoadBoard(values[1]);
                return BoardDataArray;
            }
            catch (Exception e)
            {
                Debug.LogError($"스테이지 {num}의 CSV 파일을 읽는 중 오류 발생: {e.ToString()}");
                return null;
            }
        }
    }
}
