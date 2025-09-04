using SCR;
using SCR_O;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms.Impl;

namespace KDJ
{
    public class BoardMatchChecker : MonoBehaviour
    {
        #region 배열 체크

        /// <summary>
        /// 현재 게임 보드에서 매치가 가능한 모든 경우의 수를 찾습니다. (특수 블록 포함)
        /// </summary>
        /// <param name="boardManager">게임 보드 관리자</param>
        /// <param name="matchCount">찾은 매치 가능성의 총 수</param>
        /// <returns>매치 가능한 움직임이 하나라도 있으면 true, 그렇지 않으면 false</returns>
        public bool AllBlockMatchPossibilityCheck(BoardManager boardManager, out int matchCount)
        {
            // 중복된 매치 가능성을 제외하기 위해 HashSet 사용
            var possibleMoves = new HashSet<string>();
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (gameBoard == null) { matchCount = 0; return false; }

            int height = gameBoard.Height;
            int width = gameBoard.Width;

            // 원본 배열을 건드리지 않기 위해 임시 배열 복사
            Block[,] tempBlockArray = (Block[,])gameBoard.BlockArray.Clone();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!gameBoard.BlockPlate.BlockPlateArray[y, x] || tempBlockArray[y, x] == null) continue;

                    // 현재 블록이 특수 블록인 경우, 그 자체로 매치 가능성으로 간주
                    if (tempBlockArray[y, x].GemType > GemType.Sugar && tempBlockArray[y, x].GemType < GemType.Dust)
                    {
                        possibleMoves.Add($"({x},{y})");
                        continue;
                    }

                    // 가로 방향으로 인접한 블록과 스왑하여 매치 확인
                    if (x + 1 < width && gameBoard.BlockPlate.BlockPlateArray[y, x + 1] && tempBlockArray[y, x + 1] != null)
                    {
                        // 임시 스왑
                        var temp = tempBlockArray[y, x];
                        tempBlockArray[y, x] = tempBlockArray[y, x + 1];
                        tempBlockArray[y, x + 1] = temp;

                        if (CheckForMatchAt(boardManager, x, y, tempBlockArray) || CheckForMatchAt(boardManager, x + 1, y, tempBlockArray))
                        {
                            possibleMoves.Add($"({x},{y}):({x + 1},{y})");
                        }

                        // 스왑 원상 복구
                        temp = tempBlockArray[y, x];
                        tempBlockArray[y, x] = tempBlockArray[y, x + 1];
                        tempBlockArray[y, x + 1] = temp;
                    }

                    // 세로 방향으로 인접한 블록과 스왑하여 매치 확인
                    if (y + 1 < height && gameBoard.BlockPlate.BlockPlateArray[y + 1, x] && tempBlockArray[y + 1, x] != null)
                    {
                        // 임시 스왑
                        var temp = tempBlockArray[y, x];
                        tempBlockArray[y, x] = tempBlockArray[y + 1, x];
                        tempBlockArray[y + 1, x] = temp;

                        if (CheckForMatchAt(boardManager, x, y, tempBlockArray) || CheckForMatchAt(boardManager, x, y + 1, tempBlockArray))
                        {
                            possibleMoves.Add($"({x},{y}):({x},{y + 1})");
                        }

                        // 스왑 원상 복구
                        temp = tempBlockArray[y, x];
                        tempBlockArray[y, x] = tempBlockArray[y + 1, x];
                        tempBlockArray[y + 1, x] = temp;
                    }
                }
            }

            matchCount = possibleMoves.Count;
            return matchCount > 0;
        }

        /// <summary>
        /// 현재 보드 상태에서 이미 완성된 매치가 있는지 확인합니다.
        /// </summary>
        /// <param name="boardManager">게임 보드 관리자</param>
        /// <returns>완성된 매치가 하나라도 있으면 true, 그렇지 않으면 false</returns>
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
                    // 특수 블록은 매치 검사에서 제외
                    if (block.GemType > GemType.Sugar && block.GemType < GemType.Dust) continue;

                    // 가로, 세로, 2x2 큐브 매치 확인
                    if (FindFullLineMatch(boardManager, x, y, true).Count >= 3) return true;
                    if (FindFullLineMatch(boardManager, x, y, false).Count >= 3) return true;
                    if (x < width - 1 && y < height - 1 && IsCubeMatched(boardManager, x, y)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 보드의 모든 매치를 찾아 처리하고, 특수 블록을 생성하며, 점수를 업데이트합니다.
        /// 이 함수는 매치 우선순위(5줄 > L/T > 2x2 > 4줄 > 3줄)에 따라 매치를 처리합니다.
        /// </summary>
        /// <param name="boardManager">게임 보드 관리자</param>
        /// <param name="swapPosition">플레이어 스왑이 발생한 경우, 해당 위치</param>
        /// <returns>매치가 발생하여 블록이 파괴되었으면 true, 아니면 false</returns>
        public (HashSet<Vector2Int> coordsToDestroy, (Vector2Int pos, int type)? specialToCreate, Vector2Int? specialSpawnPos) ProcessMatches(BoardManager boardManager, Vector2Int? swapPosition = null)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            var coordsToDestroy = new HashSet<Vector2Int>();
            (Vector2Int pos, int type)? specialToCreate = null;
            Vector2Int? specialSpawnPos = null;

            if (gameBoard == null) return (coordsToDestroy, specialToCreate, specialSpawnPos);

            int height = gameBoard.Height;
            int width = gameBoard.Width;
            bool[,] visited = new bool[height, width];

            // ... (매치 탐색 로직은 동일) ...

            // 우선순위 1: 5개짜리 직선 매치 (가로/세로)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                    var hMatch = FindFullLineMatch(boardManager, x, y, true);
                    if (hMatch.Count >= 5) { if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 10); foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                    var vMatch = FindFullLineMatch(boardManager, x, y, false);
                    if (vMatch.Count >= 5) { if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 10); foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                }
            }

            // 우선순위 2: L/T 형태 교차 매치
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                    var hMatch = FindFullLineMatch(boardManager, x, y, true);
                    var vMatch = FindFullLineMatch(boardManager, x, y, false);
                    if (hMatch.Count >= 3 && vMatch.Count >= 3) { if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 9); foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                }
            }

            // 우선순위 3: 2x2 큐브 매치
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    if (visited[y, x]) continue;
                    if (IsCubeMatched(boardManager, x, y))
                    {
                        var coords = new Vector2Int[] { new(x, y), new(x + 1, y), new(x, y + 1), new(x + 1, y + 1) };
                        bool isOverlapped = false;
                        foreach (var c in coords) { if (visited[c.y, c.x]) isOverlapped = true; }
                        if (isOverlapped) continue;
                        if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 6);
                        foreach (var c in coords) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                    }
                }
            }

            // 우선순위 4: 4개짜리 직선 매치
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                    var hMatch = FindFullLineMatch(boardManager, x, y, true);
                    if (hMatch.Count == 4) { if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 7); foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                    var vMatch = FindFullLineMatch(boardManager, x, y, false);
                    if (vMatch.Count == 4) { if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 8); foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                }
            }

            // 우선순위 5: 3개짜리 직선 매치
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                    var hMatch = FindFullLineMatch(boardManager, x, y, true);
                    if (hMatch.Count == 3) { foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                    var vMatch = FindFullLineMatch(boardManager, x, y, false);
                    if (vMatch.Count == 3) { foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                }
            }

            if (coordsToDestroy.Count > 0)
            {
                boardManager.MatchCombo.UpCombo();
                int score = 0;
                var damagedObstaclesThisMatch = new HashSet<Block>();

                foreach (var coord in coordsToDestroy)
                {
                    Block block = gameBoard.GetBlock(coord.x, coord.y);
                    if (block != null)
                    {
                        if (block.GemType < GemType.Milk)
                        {
                            InGameManager.AddIngredientSta(block.GemType);
                        }

                        score += 10;

                        ApplySplashDamageToNeighbors(boardManager, coord.x, coord.y, damagedObstaclesThisMatch);
                        ApplyDamageToOverlayBlock(boardManager, coord.x, coord.y, damagedObstaclesThisMatch);
                    }
                }

                int finalScore = CalculateScore(score);
                boardManager.UpdateUI(finalScore);

                if (specialToCreate.HasValue)
                {
                    var creation = specialToCreate.Value;
                    if (swapPosition.HasValue && coordsToDestroy.Contains(swapPosition.Value))
                    {
                        specialSpawnPos = swapPosition.Value;
                    }
                    else
                    {
                        int minX = int.MaxValue, maxY = int.MinValue;
                        foreach (var coord in coordsToDestroy) { if (coord.x < minX) minX = coord.x; if (coord.y > maxY) maxY = coord.y; }
                        specialSpawnPos = new Vector2Int(minX, maxY);
                        if (!coordsToDestroy.Contains(specialSpawnPos.Value)) specialSpawnPos = creation.pos;
                    }
                }
            }

            return (coordsToDestroy, specialToCreate, specialSpawnPos);
        }

        /// <summary>
        /// 지정된 좌표의 블록이 파괴될 때, 인접한 방해 블록에 스플래시 데미지를 적용합니다.
        /// </summary>
        /// <param name="boardManager"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="damagedObstacles"></param>
        private void ApplySplashDamageToNeighbors(BoardManager boardManager, int x, int y, HashSet<Block> damagedObstacles)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            Vector2Int[] neighborCoords = { new(x, y + 1), new(x, y - 1), new(x + 1, y), new(x - 1, y) };

            foreach (var coord in neighborCoords)
            {
                if (coord.x >= 0 && coord.x < gameBoard.Width && coord.y >= 0 && coord.y < gameBoard.Height)
                {
                    Block neighbor = gameBoard.GetBlock(coord.x, coord.y);
                    if (neighbor == null) continue;

                    ObstacleBlock obstacleToDamage = null;

                    if (neighbor is FlourBag_s flourBagS)
                    {
                        obstacleToDamage = flourBagS.Owner;
                    }
                    else if (neighbor is ObstacleBlock)
                    {
                        obstacleToDamage = neighbor as ObstacleBlock;
                    }

                    if (obstacleToDamage != null && !damagedObstacles.Contains(obstacleToDamage))
                    {
                        obstacleToDamage.SplashDamage();
                        damagedObstacles.Add(obstacleToDamage);
                    }
                }
            }
        }

        private void ApplyDamageToOverlayBlock(BoardManager boardManager, int x, int y, HashSet<Block> damagedObstacles)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            Block overlayBlock = gameBoard.GetOverlayBlock(x, y);
            if (overlayBlock is ObstacleBlock obstacle && !damagedObstacles.Contains(obstacle))
            {
                obstacle.TakeDamage();
                damagedObstacles.Add(obstacle);
            }
        }
        #endregion

        #region 유틸 함수
        /// <summary>
        /// 주어진 임시 배열에서 특정 위치의 블록이 매치되는지 확인합니다. (Shuffle 로직용)
        /// </summary>
        private bool CheckForMatchAt(BoardManager boardManager, int x, int y, Block[,] blockArray)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (!gameBoard.BlockPlate.BlockPlateArray[y, x]) return false;

            var block = blockArray[y, x];
            if (block == null || !block.IsNormal) return false;

            GemType gemType = block.GemType;
            int height = gameBoard.Height;
            int width = gameBoard.Width;

            // 가로 매치 확인
            int horizontalCount = 1;
            for (int i = x - 1; i >= 0; i--) { var nextBlock = blockArray[y, i]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) horizontalCount++; else break; }
            for (int i = x + 1; i < width; i++) { var nextBlock = blockArray[y, i]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) horizontalCount++; else break; }
            if (horizontalCount >= 3) return true;

            // 세로 매치 확인
            int verticalCount = 1;
            for (int i = y - 1; i >= 0; i--) { var nextBlock = blockArray[i, x]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) verticalCount++; else break; }
            for (int i = y + 1; i < height; i++) { var nextBlock = blockArray[i, x]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) verticalCount++; else break; }
            if (verticalCount >= 3) return true;

            return false;
        }

        /// <summary>
        /// 지정된 위치에서 2x2 큐브 매치가 완성되었는지 확인합니다.
        /// </summary>
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

        /// <summary>
        /// 지정된 위치에서 가로 또는 세로로 연속된 매치 블록 리스트를 찾습니다.
        /// </summary>
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
                // 왼쪽으로 탐색
                for (int x = startX - 1; x >= 0; x--) { var currentBlock = gameBoard.GetBlock(x, startY); if (currentBlock != null && currentBlock.IsNormal && currentBlock.GemType == startType) matches.Add(new Vector2Int(x, startY)); else break; }
                // 오른쪽으로 탐색
                for (int x = startX + 1; x < gameBoard.Width; x++) { var currentBlock = gameBoard.GetBlock(x, startY); if (currentBlock != null && currentBlock.IsNormal && currentBlock.GemType == startType) matches.Add(new Vector2Int(x, startY)); else break; }
            }
            else
            {
                // 위쪽으로 탐색
                for (int y = startY - 1; y >= 0; y--) { var currentBlock = gameBoard.GetBlock(startX, y); if (currentBlock != null && currentBlock.IsNormal && currentBlock.GemType == startType) matches.Add(new Vector2Int(startX, y)); else break; }
                // 아래쪽으로 탐색
                for (int y = startY + 1; y < gameBoard.Height; y++) { var currentBlock = gameBoard.GetBlock(startX, y); if (currentBlock != null && currentBlock.IsNormal && currentBlock.GemType == startType) matches.Add(new Vector2Int(startX, y)); else break; }
            }
            return matches;
        }

        /// <summary>
        /// 해당 좌표가 보드 범위 내에 있는지 확인합니다.
        /// </summary>
        public bool CheckInOfArray(BoardManager boardManager, int x, int y)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (gameBoard == null) return false;
            return y >= 0 && y < gameBoard.Height && x >= 0 && x < gameBoard.Width;
        }

        /// <summary>
        /// 해당 좌표에 유효한 블록이 있는지 확인합니다. (플레이트 위에 있으며, null이 아님)
        /// </summary>
        public bool CheckBlockIsValue(BoardManager boardManager, int x, int y)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (gameBoard == null) return false;
            return gameBoard.BlockPlate.BlockPlateArray[y, x] && gameBoard.GetBlock(x, y) != null;
        }

        /// <summary>
        /// 해당 좌표와 블록이 게임 로직상 유효한지 종합적으로 확인합니다.
        /// </summary>
        public bool CheckBlockIsAllValid(BoardManager boardManager, int x, int y)
        {
            return CheckInOfArray(boardManager, x, y) && CheckBlockIsValue(boardManager, x, y);
        }

        public int CalculateScore(int score)
        {
            int finalScore = (int)(score + BoardManager.Instance.MatchCombo.CurCombo * 0.5f * score);
            return finalScore;
        }

        #endregion
    }
}