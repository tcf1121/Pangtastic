using SCR;
using UnityEngine;

namespace SCR_B
{

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
                OverlayArray = new Block[plateHeight + 1, plateWidth],
                RespawnPos = new()
            };

            foreach (var pos in puzzleBoardSO.SpawnPoints)
            {
                boardData.RespawnPos.Add(new Vector2Int(pos.x, pos.y) + boardData.ZeroPos + boardData.Size);
            }
            int xOffset = -xMin;
            int yOffset = -yMin;

            foreach (var cell in cellArray)
            {
                boardData.SetDonut(cell.gemType);
            }

            foreach (var cell in cellArray)
            {
                int x = cell.Position.x + xOffset;
                int y = cell.Position.y + yOffset;
                if (y >= boardData.GetHeight() || x >= boardData.GetWidth() || y < 0 || x < 0)
                {
                    Debug.LogWarning($"좌표가 배열 범위를 벗어났습니다.");
                    continue;
                }
                boardData.BlockPlateArray[y, x] = true;
                boardData.SetArray(x, y, cell.gemType);
            }

            for (int x = 0; x < boardData.Size.x; x++)
                for (int y = 0; y < boardData.Size.y; y++)
                {
                    if (!boardData.BlockPlateArray[y, x])
                    {
                        blockPlate.DrawWall(new Vector3Int(x - xOffset, y - yOffset, 0));
                    }
                }
            BoardDataArray = boardData;

            InGameManager.SetPuzzleSize(-xOffset, -yOffset, boardData.Size.x, boardData.Size.y);
            return boardData;
        }
    }
}
