using SCR;
using System.Collections.Generic;
using UnityEngine;

namespace KDJ
{
    public class BoardMatchChecker : MonoBehaviour
    {
        #region 배열 체크

        public bool AllBlockMatchPossibilityCheck(BoardManager boardManager, out int matchCount)
        {
            var possibleMoves = new HashSet<string>();
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (gameBoard == null) { matchCount = 0; return false; }

            int height = gameBoard.Height;
            int width = gameBoard.Width;

            Block[,] tempBlockArray = (Block[,])gameBoard.BlockArray.Clone();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!gameBoard.BlockPlate.BlockPlateArray[y, x] || tempBlockArray[y, x] == null) continue;

                    if (tempBlockArray[y, x].GemType > GemType.Sugar && tempBlockArray[y, x].GemType < GemType.Dust)
                    {
                        possibleMoves.Add($"({x},{y})");
                        continue;
                    }

                    if (x + 1 < width && gameBoard.BlockPlate.BlockPlateArray[y, x + 1] && tempBlockArray[y, x + 1] != null)
                    {
                        var temp = tempBlockArray[y, x];
                        tempBlockArray[y, x] = tempBlockArray[y, x + 1];
                        tempBlockArray[y, x + 1] = temp;

                        if (CheckForMatchAt(boardManager, x, y, tempBlockArray) || CheckForMatchAt(boardManager, x + 1, y, tempBlockArray))
                        {
                            possibleMoves.Add($"({x},{y}):({x + 1},{y})");
                        }

                        temp = tempBlockArray[y, x];
                        tempBlockArray[y, x] = tempBlockArray[y, x + 1];
                        tempBlockArray[y, x + 1] = temp;
                    }

                    if (y + 1 < height && gameBoard.BlockPlate.BlockPlateArray[y + 1, x] && tempBlockArray[y + 1, x] != null)
                    {
                        var temp = tempBlockArray[y, x];
                        tempBlockArray[y, x] = tempBlockArray[y + 1, x];
                        tempBlockArray[y + 1, x] = temp;

                        if (CheckForMatchAt(boardManager, x, y, tempBlockArray) || CheckForMatchAt(boardManager, x, y + 1, tempBlockArray))
                        {
                            possibleMoves.Add($"({x},{y}):({x},{y + 1})");
                        }

                        temp = tempBlockArray[y, x];
                        tempBlockArray[y, x] = tempBlockArray[y + 1, x];
                        tempBlockArray[y + 1, x] = temp;
                    }
                }
            }

            matchCount = possibleMoves.Count;
            return matchCount > 0;
        }

        public bool AllBlockMatchCheck(BoardManager boardManager)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (gameBoard == null) return false;

            int height = gameBoard.Height;
            int width = gameBoard.Width;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!CheckBlockIsAllValid(boardManager, x, y)) continue;

                    var block = gameBoard.GetBlock(x, y);
                    if (block.GemType > GemType.Sugar && block.GemType < GemType.Dust) continue;

                    if (FindFullLineMatch(boardManager, x, y, true).Count >= 3) return true;
                    if (FindFullLineMatch(boardManager, x, y, false).Count >= 3) return true;
                    if (x < width - 1 && y < height - 1 && IsCubeMatched(boardManager, x, y)) return true;
                }
            }
            return false;
        }

        public bool ProcessMatches(BoardManager boardManager, Vector2Int? swapPosition = null)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (gameBoard == null) return false;

            int height = gameBoard.Height;
            int width = gameBoard.Width;

            HashSet<Vector2Int> coordsToDestroy = new HashSet<Vector2Int>();
            (Vector2Int pos, int type)? specialToCreate = null;
            bool[,] visited = new bool[height, width];

            for (int y = 0; y < height; y++) { for (int x = 0; x < width; x++) {
                if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                var hMatch = FindFullLineMatch(boardManager, x, y, true);
                if (hMatch.Count >= 5) { if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 10); foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                var vMatch = FindFullLineMatch(boardManager, x, y, false);
                if (vMatch.Count >= 5) { if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 10); foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
            }}

            for (int y = 0; y < height; y++) { for (int x = 0; x < width; x++) {
                if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                var hMatch = FindFullLineMatch(boardManager, x, y, true);
                var vMatch = FindFullLineMatch(boardManager, x, y, false);
                if (hMatch.Count >= 3 && vMatch.Count >= 3) { if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 11); foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
            }}

            for (int y = 0; y < height - 1; y++) { for (int x = 0; x < width - 1; x++) {
                if (visited[y, x]) continue;
                if (IsCubeMatched(boardManager, x, y)) {
                    var coords = new Vector2Int[] { new(x, y), new(x + 1, y), new(x, y + 1), new(x + 1, y + 1) };
                    bool isOverlapped = false;
                    foreach (var c in coords) { if (visited[c.y, c.x]) isOverlapped = true; }
                    if (isOverlapped) continue;
                    if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 9);
                    foreach (var c in coords) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                }
            }}

            for (int y = 0; y < height; y++) { for (int x = 0; x < width; x++) {
                if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                var hMatch = FindFullLineMatch(boardManager, x, y, true);
                if (hMatch.Count == 4) { if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 7); foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                var vMatch = FindFullLineMatch(boardManager, x, y, false);
                if (vMatch.Count == 4) { if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 8); foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
            }}

            for (int y = 0; y < height; y++) { for (int x = 0; x < width; x++) {
                if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                var hMatch = FindFullLineMatch(boardManager, x, y, true);
                if (hMatch.Count == 3) { foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                var vMatch = FindFullLineMatch(boardManager, x, y, false);
                if (vMatch.Count == 3) { foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
            }}

            if (coordsToDestroy.Count > 0)
            {
                boardManager.MatchCombo.UpCombo();
                int score = 0;
                foreach (var coord in coordsToDestroy)
                {
                    Block block = gameBoard.GetBlock(coord.x, coord.y);
                    if (block != null && block.BlockInstance != null)
                    {
                        Destroy(block.BlockInstance);
                        block.BlockInstance = null; // 파괴되었음을 즉시 데이터에 반영
                        score += 10;
                    }
                }
                boardManager.UpdateUI(score);

                if (specialToCreate.HasValue)
                {
                    var creation = specialToCreate.Value;
                    Vector2Int spawnPos;
                    if (swapPosition.HasValue && coordsToDestroy.Contains(swapPosition.Value)) {
                        spawnPos = swapPosition.Value;
                    } else {
                        int minX = int.MaxValue, maxY = int.MinValue;
                        foreach (var coord in coordsToDestroy) { if (coord.x < minX) minX = coord.x; if (coord.y > maxY) maxY = coord.y; }
                        spawnPos = new Vector2Int(minX, maxY);
                        if (!coordsToDestroy.Contains(spawnPos)) spawnPos = creation.pos;
                    }
                    boardManager.Spawner.SpawnBlock(spawnPos.x, spawnPos.y, (GemType)(creation.type - 1), boardManager.BlockMover);
                }
                return true;
            }
            return false;
        }
        #endregion

        #region 유틸 함수
        private bool CheckForMatchAt(BoardManager boardManager, int x, int y, Block[,] blockArray)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (!gameBoard.BlockPlate.BlockPlateArray[y, x]) return false;

            var block = blockArray[y, x];
            if (block == null || !block.IsNormal) return false;

            GemType gemType = block.GemType;
            int height = gameBoard.Height;
            int width = gameBoard.Width;

            int horizontalCount = 1;
            for (int i = x - 1; i >= 0; i--) { var nextBlock = blockArray[y, i]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) horizontalCount++; else break; }
            for (int i = x + 1; i < width; i++) { var nextBlock = blockArray[y, i]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) horizontalCount++; else break; }
            if (horizontalCount >= 3) return true;

            int verticalCount = 1;
            for (int i = y - 1; i >= 0; i--) { var nextBlock = blockArray[i, x]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) verticalCount++; else break; }
            for (int i = y + 1; i < height; i++) { var nextBlock = blockArray[i, x]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) verticalCount++; else break; }
            if (verticalCount >= 3) return true;

            return false;
        }

        private bool IsCubeMatched(BoardManager boardManager, int x, int y)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            var startBlock = gameBoard.GetBlock(x, y);

            if (startBlock == null || !startBlock.IsNormal) return false;

            var startGemType = startBlock.GemType;

            for (int i = y; i < y + 2; i++)
            {
                for (int j = x; j < x + 2; j++)
                {
                    var currentBlock = gameBoard.GetBlock(j, i);
                    if (currentBlock == null || !currentBlock.IsNormal || currentBlock.GemType != startGemType)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private List<Vector2Int> FindFullLineMatch(BoardManager boardManager, int startX, int startY, bool isHorizontal)
        {
            var matches = new List<Vector2Int>();
            var gameBoard = boardManager.Spawner.GameBoardData;
            var startBlock = gameBoard.GetBlock(startX, startY);

            if (startBlock == null || !startBlock.IsNormal) return matches;

            var startType = startBlock.GemType;
            matches.Add(new Vector2Int(startX, startY));

            if (isHorizontal)
            {
                for (int x = startX - 1; x >= 0; x--) { var currentBlock = gameBoard.GetBlock(x, startY); if (currentBlock != null && currentBlock.IsNormal && currentBlock.GemType == startType) matches.Add(new Vector2Int(x, startY)); else break; }
                for (int x = startX + 1; x < gameBoard.Width; x++) { var currentBlock = gameBoard.GetBlock(x, startY); if (currentBlock != null && currentBlock.IsNormal && currentBlock.GemType == startType) matches.Add(new Vector2Int(x, startY)); else break; }
            }
            else
            {
                for (int y = startY - 1; y >= 0; y--) { var currentBlock = gameBoard.GetBlock(startX, y); if (currentBlock != null && currentBlock.IsNormal && currentBlock.GemType == startType) matches.Add(new Vector2Int(startX, y)); else break; }
                for (int y = startY + 1; y < gameBoard.Height; y++) { var currentBlock = gameBoard.GetBlock(startX, y); if (currentBlock != null && currentBlock.IsNormal && currentBlock.GemType == startType) matches.Add(new Vector2Int(startX, y)); else break; }
            }
            return matches;
        }

        public bool CheckInOfArray(BoardManager boardManager, int x, int y)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (gameBoard == null) return false;
            return y >= 0 && y < gameBoard.Height && x >= 0 && x < gameBoard.Width;
        }

        public bool CheckBlockIsValue(BoardManager boardManager, int x, int y)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (gameBoard == null) return false;
            return gameBoard.BlockPlate.BlockPlateArray[y, x] && gameBoard.GetBlock(x, y) != null;
        }

        public bool CheckBlockIsAllValid(BoardManager boardManager, int x, int y)
        {
            return CheckInOfArray(boardManager, x, y) && CheckBlockIsValue(boardManager, x, y);
        }

        #endregion
    }
}