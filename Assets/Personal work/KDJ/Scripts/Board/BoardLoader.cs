using SCR;
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
        [SerializeField] TestStageManager testStageManager;
        private PuzzleBoardSO puzzleBoardSO;
        public BoardData BoardDataArray;

        private void Awake()
        {
            if (testStageManager == null)
            {
                testStageManager = FindObjectOfType<TestStageManager>();
                if (testStageManager == null)
                {
                    Debug.LogError("TestStageManager를 찾을 수 없습니다!");
                }
            }
        }

        public BoardData LoadBoard()
        {
            int maxBlockType;

            if (Manager.User.GetStage() < 52)
                maxBlockType = 5;
            else
                maxBlockType = 6;
                
            //실 사용 코드
            if (!BoardManager.Instance.IsTest)
                puzzleBoardSO = Manager.Stage.CurrentStage.PuzzleBoard;
            // 테스트용 코드
            else
                puzzleBoardSO = testStageManager.CurrentBoard;

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

                GemType finalGemType = gem;

                if (finalGemType == GemType.Random)
                {
                    finalGemType = (GemType)UnityEngine.Random.Range(0, maxBlockType); // 0~5 사이의 GemType을 바로 생성
                }

                if (finalGemType == GemType.Dust)
                {
                    boardData.OverlayArray[y, x] = new Dust(x, y);
                    int donutNum = UnityEngine.Random.Range(0, maxBlockType);
                    boardData.BlockArray[y, x] = new Block()
                    {
                        GemType = (GemType)donutNum,

                    };
                }
                else if (finalGemType == GemType.Syrup)
                {
                    boardData.OverlayArray[y, x] = new Syrup(x, y);
                    int donutNum = UnityEngine.Random.Range(0, maxBlockType);
                    boardData.BlockArray[y, x] = new Block()
                    {
                        GemType = (GemType)donutNum,
                    };
                }
                else if (finalGemType == GemType.Ice)
                {
                    boardData.BlockArray[y, x] = new Block()
                    {
                        GemType = (GemType)Random.Range(0, maxBlockType),
                    };
                    boardData.OverlayArray[y, x] = new Ice(x, y);
                }
                else if (finalGemType == GemType.DonutBag) boardData.BlockArray[y, x] = new DonutBag(x, y);
                else if (finalGemType == GemType.Coin) boardData.BlockArray[y, x] = new Coin(x, y);
                else if (finalGemType == GemType.GiftBox) boardData.BlockArray[y, x] = new GiftBox(x, y);
                else if (finalGemType == GemType.Egg) boardData.BlockArray[y, x] = new Egg(x, y);
                else if (finalGemType == GemType.FlourBag) boardData.BlockArray[y, x] = new FlourBag(boardData.BlockArray, x, y);
                else if (finalGemType == GemType.Flour_s) { }
                else
                {
                    boardData.BlockArray[y, x] = new Block()
                    {
                        GemType = finalGemType,
                    };
                }
            }
            BoardDataArray = boardData;
            return boardData;
        }
    }
}
