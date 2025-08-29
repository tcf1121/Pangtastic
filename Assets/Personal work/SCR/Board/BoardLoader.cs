using SCR;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SCR_B
{
    public class BoardData
    {
        public Vector2Int ZeroPos;
        public Vector2Int Size;
        public bool[,] BlockPlateArray;
        public Block[,] BlockArray;
        public Block[,] OverlayArray;
        public List<Vector2Int> RespawnPos;
        private List<GemType> _respawnDonut = new();

        public void SetArray(int x, int y, GemType gemType)
        {
            if (gemType == GemType.Dust)
            {
                OverlayArray[y, x] = new Dust(x, y);
                int donutNum = UnityEngine.Random.Range(0, 6);
                BlockArray[y, x] = new Donut(x, y, (GemType)donutNum);
            }
            else if (gemType == GemType.Syrup)
            {
                OverlayArray[y, x] = new Syrup(x, y);
                int donutNum = UnityEngine.Random.Range(0, 6);
                BlockArray[y, x] = new Donut(x, y, (GemType)donutNum);
            }
            else if (gemType == GemType.Ice) BlockArray[y, x] = new Ice(x, y);
            else if (gemType == GemType.DonutBag) BlockArray[y, x] = new DonutBag(x, y);
            else if (gemType == GemType.Coin) BlockArray[y, x] = new Coin(x, y);
            else if (gemType == GemType.GiftBox) BlockArray[y, x] = new GiftBox(x, y);
            else if (gemType == GemType.Egg) BlockArray[y, x] = new Egg(x, y);
            else if (gemType == GemType.FlourBag) BlockArray[y, x] = new FlourBag(this, x, y);
            else if (gemType == GemType.Flour_s) { }
            else if (gemType == GemType.Random) BlockArray[y, x] = new Donut(x, y, (GemType)Random.Range(0, 6));
            else BlockArray[y, x] = new Donut(x, y, gemType);
        }

        public void SetDonut(GemType gemType)
        {
            if (gemType == GemType.Random)
            {
                for (int i = 0; i < 6; i++)
                    if (!_respawnDonut.Contains((GemType)i))
                        _respawnDonut.Add((GemType)i);
            }
            else if (gemType == GemType.Lavender)
            {
                if (!_respawnDonut.Contains(GemType.Lavender))
                    _respawnDonut.Add(GemType.Lavender);
            }
            else if (gemType == GemType.Chocolate)
            {
                if (!_respawnDonut.Contains(GemType.Chocolate))
                    _respawnDonut.Add(GemType.Chocolate);
            }
            else if (gemType == GemType.Blueberry)
            {
                if (!_respawnDonut.Contains(GemType.Blueberry))
                    _respawnDonut.Add(GemType.Blueberry);
            }
            else if (gemType == GemType.Cheese)
            {
                if (!_respawnDonut.Contains(GemType.Cheese))
                    _respawnDonut.Add(GemType.Cheese);
            }
            else if (gemType == GemType.Strawberry)
            {
                if (!_respawnDonut.Contains(GemType.Strawberry))
                    _respawnDonut.Add(GemType.Strawberry);
            }
            else if (gemType == GemType.Sugar)
            {
                if (!_respawnDonut.Contains(GemType.Sugar))
                    _respawnDonut.Add(GemType.Sugar);
            }
            else if (gemType == GemType.Egg)
            {
                if (!_respawnDonut.Contains(GemType.Egg))
                    _respawnDonut.Add(GemType.Egg);
            }
            else if (gemType == GemType.Coin)
            {
                if (!_respawnDonut.Contains(GemType.Coin))
                    _respawnDonut.Add(GemType.Coin);
            }

        }

        public void DelArray(int x, int y)
        {
            BlockArray[y, x] = null;
        }

        public GemType RespawnDount()
        {
            var obstacleTypes = _respawnDonut.FindAll(v => v == GemType.Coin || v == GemType.Egg);
            if (obstacleTypes.Count > 0)
            {
                int num = Random.Range(0, 10);
                if (num < 1)
                {
                    return obstacleTypes[Random.Range(0, obstacleTypes.Count)];
                }
            }

            var normalTypes = _respawnDonut.FindAll(v => v < GemType.Roller_v);

            return normalTypes[Random.Range(0, normalTypes.Count)];
        }

        public int GetWidth()
        {
            return Size.x;
        }

        public int GetHeight()
        {
            return Size.y;
        }
    }

    public class BoardLoader : MonoBehaviour
    {
        private PuzzleBoardSO puzzleBoardSO;
        public BoardData BoardDataArray;
        [SerializeField] BlockPlate blockPlate;

        public BoardData LoadBoard()
        {
            puzzleBoardSO = StageManager.Instance.CurrentStage.PuzzleBoard;
            if (puzzleBoardSO == null)
            {
                Debug.LogError("PuzzleBoardSO 없음");
                return null;
            }

            blockPlate.DrawTile(puzzleBoardSO.Cells, puzzleBoardSO.SpawnPoints);
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
                Size = new Vector2Int(plateWidth, plateHeight),
                BlockPlateArray = new bool[plateHeight, plateWidth],
                BlockArray = new Block[plateHeight + 1, plateWidth], // BlockArray는 한 줄 더 높습니다.
                OverlayArray = new Block[plateHeight, plateWidth],
                RespawnPos = new()
            };

            foreach (var pos in puzzleBoardSO.SpawnPoints)
            {
                boardData.RespawnPos.Add(new Vector2Int(pos.x, pos.y) + boardData.ZeroPos + boardData.Size);
            }
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

                boardData.SetDonut(gem);
                boardData.SetArray(x, y, gem);
            }
            BoardDataArray = boardData;
            return boardData;
        }
    }
}
