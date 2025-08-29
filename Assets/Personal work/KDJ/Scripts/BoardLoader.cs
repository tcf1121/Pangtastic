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
        public Vector2Int ZeroPos;
        public bool[,] BlockPlateArray;
        public Block[,] BlockArray;
        public Block[,] OverlayArray;
    }

    public class BoardLoader : MonoBehaviour
    {
        private PuzzleBoardSO puzzleBoardSO;
        public BoardData BoardDataArray;

        public BoardData LoadBoard()
        {
            puzzleBoardSO = StageManager.Instance.CurrentStage.PuzzleBoard;
            if (puzzleBoardSO == null)
            {
                Debug.LogError("PuzzleBoardSO 없음");
                return null;
            }

            CellData[] cellArray = puzzleBoardSO.BuildMapArray();

            if (cellArray.Length == 0)
            {
                Debug.LogError("SO에 테이터없음");
                return null;
            }

            int xMin = int.MaxValue;
            int xMax = int.MinValue;
            int yMin = int.MaxValue;
            int yMax = int.MinValue;

            for (int i = 0; i < cellArray.Length; i++)
            {
                Vector3Int pos = cellArray[i].Position; // 셀 좌표
                if (pos.x < xMin) xMin = pos.x;
                if (pos.x > xMax) xMax = pos.x;
                if (pos.y < yMin) yMin = pos.y;
                if (pos.y > yMax) yMax = pos.y;
            }

            int plateHeight = yMax - yMin + 1;
            int plateWidth = xMax - xMin + 1;

            var boardData = new BoardData
            {
                ZeroPos = new Vector2Int(xMin, yMin),
                BlockPlateArray = new bool[plateHeight, plateWidth],
                BlockArray = new Block[plateHeight + 1, plateWidth], // BlockArray는 한 줄 더 높습니다.
                OverlayArray = new Block[plateHeight + 1, plateWidth]
            };

            int xOffset = -xMin;
            int yOffset = -yMin;

            for (int i = 0; i < cellArray.Length; i++)
            {
                Vector3Int pos = cellArray[i].Position;
                GemType gem = cellArray[i].gemType;

                int x = pos.x + xOffset;
                int y = pos.y + yOffset;

                if (y >= plateHeight || x >= plateWidth || y < 0 || x < 0)
                {
                    Debug.LogWarning($"좌표 {pos} 가 배열 범위를 벗어났습니다.");
                    continue;
                }
                boardData.BlockPlateArray[y, x] = true;

                int blockType;
                GemType finalGemType = gem;

                if (finalGemType == GemType.Random)
                {
                    int tempNum = UnityEngine.Random.Range(1, 7);
                    finalGemType = (GemType)(tempNum - 1);
                    blockType = tempNum;
                }
                else
                {
                    blockType = (int)finalGemType + 1;
                }

                if (finalGemType == GemType.Dust)
                {
                    boardData.OverlayArray[y, x] = new SCR_O.Dust(x, y);
                    int donutNum = UnityEngine.Random.Range(0, 6);
                    boardData.BlockArray[y, x] = new Block()
                    {
                        BlockType = donutNum + 1,
                        GemType = (GemType)donutNum,

                    };
                }
                else if (finalGemType == GemType.Syrup)
                {
                    boardData.OverlayArray[y, x] = new SCR_O.Syrup(x, y);
                    int donutNum = UnityEngine.Random.Range(0, 6);
                    boardData.BlockArray[y, x] = new Block()
                    {
                        BlockType = donutNum + 1,
                        GemType = (GemType)donutNum,
                    };
                }
                else if (finalGemType == GemType.Ice) boardData.BlockArray[y, x] = new SCR_O.Ice(x, y);
                else if (finalGemType == GemType.DonutBag) boardData.BlockArray[y, x] = new SCR_O.DonutBag(x, y);
                else if (finalGemType == GemType.Coin) boardData.BlockArray[y, x] = new SCR_O.Coin(x, y);
                else if (finalGemType == GemType.GiftBox) boardData.BlockArray[y, x] = new SCR_O.GiftBox(x, y);
                else if (finalGemType == GemType.Egg) boardData.BlockArray[y, x] = new SCR_O.Egg(x, y);
                else if (finalGemType == GemType.FlourBag) boardData.BlockArray[y, x] = new SCR_O.FlourBag(boardData, x, y);
                else if (finalGemType == GemType.Flour_s) { }
                else
                {
                    boardData.BlockArray[y, x] = new Block()
                    {
                        GemType = finalGemType,
                        BlockType = blockType,
                    };
                }
            }
            BoardDataArray = boardData;
            return boardData;
        }
    }
}
