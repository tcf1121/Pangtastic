using SCR;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Unity.Mathematics;

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
        /// 힌트를 위해 최적의 매치 가능성을 찾아 리스트로 좌표를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public List<Vector2Int> OptimalMatchFind()
        {
            List<Vector2Int> result = new List<Vector2Int>();

            // 중복된 매치 가능성을 제외하기 위해 HashSet 사용
            HashSet<Vector2Int> matchPos = new HashSet<Vector2Int>();
            var gameBoard = BoardManager.Instance.Spawner.GameBoardData;

            if (gameBoard == null) return null;

            int height = gameBoard.Height;
            int width = gameBoard.Width;

            // 원본 배열을 건드리지 않기 위해 임시 배열 복사
            Block[,] tempBlockArray = (Block[,])gameBoard.BlockArray.Clone();
            // 비교할 매치들을 담을 리스트를 포함한 튜플 리스트
            List<(List<Vector2Int> matchCoords, int matchWeight)> matchList = new List<(List<Vector2Int> matchCoords, int matchWeight)>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!gameBoard.BlockPlate.BlockPlateArray[y, x] || tempBlockArray[y, x] == null) continue;

                    // 현재 블록이 특수 블록일 경우 상하좌우 탐색 후 해당 위치가 특수블록일 경우 최우선순위로 설정
                    if (tempBlockArray[y, x].GemType > GemType.Sugar && tempBlockArray[y, x].GemType < GemType.Dust)
                    {
                        if (CheckCrossPosIsSpecial(tempBlockArray, x, y, out List<Vector2Int> specialPositions))
                        {
                            // 특수 블록끼리 인접해있다면 최우선순위로 설정
                            matchList.Add((specialPositions, 1));
                        }
                        else
                        {
                            // 특수 블록이지만 인접한 특수 블록이 없다면, 4번째 우선순위로 설정
                            matchList.Add((new List<Vector2Int> { new Vector2Int(x, y) }, 4));
                        }
                    }

                    // 가로 방향으로 인접한 블록과 스왑하여 매치 확인
                    if (x + 1 < width && gameBoard.BlockPlate.BlockPlateArray[y, x + 1] && tempBlockArray[y, x + 1] != null)
                    {
                        // 임시 스왑
                        var temp = tempBlockArray[y, x];
                        tempBlockArray[y, x] = tempBlockArray[y, x + 1];
                        tempBlockArray[y, x + 1] = temp;

                        if (CheckForMatchAt(BoardManager.Instance, x, y, tempBlockArray, out int count, out int type, out HashSet<Vector2Int> matchedCoords))
                        {
                            matchedCoords.Remove(new Vector2Int(x, y));
                            matchedCoords.Add(new Vector2Int(x + 1, y));
                            matchList.Add(CalculateBlockMatchWeight(x, y, type, tempBlockArray, new List<Vector2Int>(matchedCoords)));
                        }

                        if (CheckForMatchAt(BoardManager.Instance, x + 1, y, tempBlockArray, out count, out type, out matchedCoords))
                        {
                            matchedCoords.Remove(new Vector2Int(x + 1, y));
                            matchedCoords.Add(new Vector2Int(x, y));
                            matchList.Add(CalculateBlockMatchWeight(x + 1, y, type, tempBlockArray, new List<Vector2Int>(matchedCoords)));
                        }

                        if (CheckCubeMatchesAround(x, y, tempBlockArray, out matchedCoords))
                        {
                            matchedCoords.Remove(new Vector2Int(x, y));
                            matchedCoords.Add(new Vector2Int(x + 1, y));
                            matchList.Add(CalculateBlockMatchWeight(x, y, 2, tempBlockArray, new List<Vector2Int>(matchedCoords)));
                        }

                        if (CheckCubeMatchesAround(x + 1, y, tempBlockArray, out matchedCoords))
                        {
                            matchedCoords.Remove(new Vector2Int(x + 1, y));
                            matchedCoords.Add(new Vector2Int(x, y));
                            matchList.Add(CalculateBlockMatchWeight(x + 1, y, 2, tempBlockArray, new List<Vector2Int>(matchedCoords)));
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

                        if (CheckForMatchAt(BoardManager.Instance, x, y, tempBlockArray, out int count, out int type, out HashSet<Vector2Int> matchedCoords))
                        {
                            matchedCoords.Remove(new Vector2Int(x, y)); // 현재 블록 좌표는 제외
                            matchedCoords.Add(new Vector2Int(x, y + 1)); // 스왑된 블록 좌표도 포함
                            matchList.Add(CalculateBlockMatchWeight(x, y, type, tempBlockArray, new List<Vector2Int>(matchedCoords)));
                        }

                        if (CheckForMatchAt(BoardManager.Instance, x, y + 1, tempBlockArray, out count, out type, out matchedCoords))
                        {
                            matchedCoords.Remove(new Vector2Int(x, y + 1)); // 현재 블록 좌표는 제외
                            matchedCoords.Add(new Vector2Int(x, y)); // 스왑된 블록 좌표도 포함
                            matchList.Add(CalculateBlockMatchWeight(x, y + 1, type, tempBlockArray, new List<Vector2Int>(matchedCoords)));
                        }

                        if (CheckCubeMatchesAround(x, y, tempBlockArray, out matchedCoords))
                        {
                            matchedCoords.Remove(new Vector2Int(x, y));
                            matchedCoords.Add(new Vector2Int(x, y + 1));
                            matchList.Add(CalculateBlockMatchWeight(x, y, 2, tempBlockArray, new List<Vector2Int>(matchedCoords)));
                        }

                        if (CheckCubeMatchesAround(x, y + 1, tempBlockArray, out matchedCoords))
                        {
                            matchedCoords.Remove(new Vector2Int(x, y + 1));
                            matchedCoords.Add(new Vector2Int(x, y));
                            matchList.Add(CalculateBlockMatchWeight(x, y + 1, 2, tempBlockArray, new List<Vector2Int>(matchedCoords)));
                        }

                        // 스왑 원상 복구
                        temp = tempBlockArray[y, x];
                        tempBlockArray[y, x] = tempBlockArray[y + 1, x];
                        tempBlockArray[y + 1, x] = temp;
                    }
                }
            }

            // 가중치를 토대로 최적의 매치 리스트를 선택
            if (matchList.Count > 0)
            {
                var optimalMatch = matchList.OrderBy(m => m.matchWeight).First();
                result = optimalMatch.matchCoords;
                return result;
            }

            return result;
        }

        /// <summary>
        /// 매치 가능한 블록이 요구재료인지 확인하고 가중치를 계산합니다.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="tempBlockArray"></param>
        /// <param name="checkedPositions"></param>
        /// <returns></returns>
        private (List<Vector2Int> matchCoords, int matchWeight) CalculateBlockMatchWeight(int x, int y, int type, Block[,] tempBlockArray, List<Vector2Int> checkedPositions)
        {
            (List<Vector2Int> matchCoords, int matchWeight) matchResult = (new List<Vector2Int>(), 0);
            List<GemType> targetTypes = InGameManager.GetTagetGem();

            if (targetTypes != null && targetTypes.Contains(tempBlockArray[y, x].GemType))
            {
                matchResult.matchWeight = 2; // 요구 재료에 있다면 2순위
            }
            else
            {
                if (type == 1)
                {
                    if (checkedPositions.Count > 3)
                        matchResult.matchWeight = 3; // 특수 블록이 생성 가능한 매치의 경우 3순위
                    else if (checkedPositions.Count == 3)
                        matchResult.matchWeight = 5; // 일반 3줄 매치의 경우 최하순위
                }
                else
                {
                    // 나머지 L/T, 2x2 매치의 경우 항상 특수 블록이 생성 되기에 3순위
                    matchResult.matchWeight = 3;
                }

            }

            matchResult.matchCoords = checkedPositions;
            return matchResult;
        }

        /// <summary>
        /// 매치 우선순위에 따른 가중치를 계산합니다.
        /// 5줄 > L/T > 2x2 > 4줄 > 3줄
        /// </summary>
        /// <param name="type">1: 가로, 세로매치, 2: 2x2 매치, 3: L/T매치</param>
        /// <param name="count"></param>
        private int WeightCalculate(int type, int count)
        {
            int weight = 0;

            if (type == 1)
            {
                if (count >= 5) weight = 1;
                else if (count == 4) weight = 4;
                else if (count == 3) weight = 5;
            }
            else if (type == 2)
            {
                // 2x2 매치가 되었다면 항상 3의 가중치 부여
                weight = 3;
            }
            else if (type == 3)
            {
                // L/T 매치가 되었다면 항상 2의 가중치 부여
                weight = 2;
            }

            return weight;
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
                    if (!block.IsNormal) continue;

                    // 가로, 세로, 2x2 큐브 매치 확인
                    if (FindFullLineMatch(boardManager, x, y, true).Count >= 3) return true;
                    if (FindFullLineMatch(boardManager, x, y, false).Count >= 3) return true;
                    if (x < width - 1 && y < height - 1 && IsCubeMatched(boardManager, x, y)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 보드의 모든 매치를 찾아 처리하고, 그 결과를 반환합니다.
        /// 이 함수는 매치 우선순위(5줄 > L/T > 2x2 > 4줄 > 3줄)에 따라 매치를 처리합니다.
        /// </summary>
        /// <param name="boardManager">게임 보드 관리자</param>
        /// <param name="swapPosition">플레이어 스왑이 발생한 경우, 해당 위치</param>
        /// <returns>파괴할 블록 좌표, 생성할 특수 블록 정보, 특수 블록 생성 위치를 담은 튜플을 반환합니다.</returns>
        public (HashSet<Vector2Int> coordsToDestroy, (Vector2Int pos, int type)? specialToCreate, Vector2Int? specialSpawnPos) ProcessMatches(BoardManager boardManager, Vector2Int? swapPosition = null)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            var coordsToDestroy = new HashSet<Vector2Int>();
            (Vector2Int pos, int type)? specialToCreate = null;
            Vector2Int? specialSpawnPos = null;
            List<Vector2Int> specialMatchCoords = null; // 특수 블록 매치 좌표 저장

            if (gameBoard == null) return (coordsToDestroy, specialToCreate, specialSpawnPos);

            int height = gameBoard.Height;
            int width = gameBoard.Width;
            bool[,] visited = new bool[height, width];

            // 우선순위 1: 5개짜리 직선 매치 (가로/세로)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                    var hMatch = FindFullLineMatch(boardManager, x, y, true);
                    if (hMatch.Count >= 5) { if (!specialToCreate.HasValue) { specialToCreate = (new Vector2Int(x, y), 10); specialMatchCoords = hMatch; } foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                    var vMatch = FindFullLineMatch(boardManager, x, y, false);
                    if (vMatch.Count >= 5) { if (!specialToCreate.HasValue) { specialToCreate = (new Vector2Int(x, y), 10); specialMatchCoords = vMatch; } foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
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
                    if (hMatch.Count >= 3 && vMatch.Count >= 3)
                    {
                        if (!specialToCreate.HasValue)
                        {
                            specialToCreate = (new Vector2Int(x, y), 9);
                            specialMatchCoords = new List<Vector2Int>();
                            specialMatchCoords.AddRange(hMatch);
                            specialMatchCoords.AddRange(vMatch);
                        }
                        foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                        foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                    }
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
                        if (!specialToCreate.HasValue)
                        {
                            specialToCreate = (new Vector2Int(x, y), 6);
                            specialMatchCoords = new List<Vector2Int>(coords);
                        }
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
                    if (hMatch.Count == 4) { if (!specialToCreate.HasValue) { specialToCreate = (new Vector2Int(x, y), 7); specialMatchCoords = hMatch; } foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
                    var vMatch = FindFullLineMatch(boardManager, x, y, false);
                    if (vMatch.Count == 4) { if (!specialToCreate.HasValue) { specialToCreate = (new Vector2Int(x, y), 8); specialMatchCoords = vMatch; } foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } }
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
                var damagedObstaclesThisMatch = new HashSet<Block>();

                foreach (var coord in coordsToDestroy)
                {
                    Block block = gameBoard.GetBlock(coord.x, coord.y);
                    if (block != null)
                    {
                        ApplySplashDamageToNeighbors(boardManager, coord.x, coord.y, damagedObstaclesThisMatch);
                        ApplyDamageToOverlayBlock(boardManager, coord.x, coord.y, damagedObstaclesThisMatch);
                    }
                }

                if (specialToCreate.HasValue)
                {
                    var creation = specialToCreate.Value;
                    if (swapPosition.HasValue && coordsToDestroy.Contains(swapPosition.Value))
                    {
                        specialSpawnPos = swapPosition.Value;
                    }
                    else
                    {
                        if (specialMatchCoords != null && specialMatchCoords.Count > 0)
                        {
                            int minX = int.MaxValue, minY = int.MaxValue;
                            foreach (var coord in specialMatchCoords)
                            {
                                if (coord.x < minX) minX = coord.x;
                                if (coord.y < minY) minY = coord.y;
                            }
                            specialSpawnPos = new Vector2Int(minX, minY);
                        }
                        else
                        {
                            specialSpawnPos = creation.pos;
                        }
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
                    Block overlayNeighbor = gameBoard.GetOverlayBlock(coord.x, coord.y);
                    if (overlayNeighbor is Ice ice)
                    {
                        ice.SplashDamage(gameBoard.GetBlock(x, y).GemType);
                        damagedObstacles.Add(ice);
                        continue;
                    }

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
        /// 주어진 배열에서 특정 위치의 블록이 매치되는지 확인합니다.
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

        private bool CheckCrossPosIsSpecial(Block[,] blockArray, int x, int y, out List<Vector2Int> specialPositions)
        {
            var block = blockArray[y, x];
            if (block == null) { specialPositions = null; return false; }

            specialPositions = new List<Vector2Int>();
            List<Vector2Int> adjacentPositions = new List<Vector2Int>
            {
                new Vector2Int(x + 1, y),
                new Vector2Int(x - 1, y),
                new Vector2Int(x, y + 1),
                new Vector2Int(x, y - 1)
            };

            specialPositions.Add(new Vector2Int(x, y)); // 현재 블록 위치 추가

            foreach (var pos in adjacentPositions)
            {
                if (pos.x < 0 || pos.x >= blockArray.GetLength(1) || pos.y < 0 || pos.y >= blockArray.GetLength(0))
                    continue;

                var adjacentBlock = blockArray[pos.y, pos.x];
                if (adjacentBlock != null && adjacentBlock.GemType > GemType.Sugar && adjacentBlock.GemType < GemType.Dust)
                {
                    specialPositions.Add(pos);
                }
            }
            return specialPositions.Count > 1;
        }

        /// <summary>
        /// 주어진 배열에서 특정 위치의 블록이 매치되는지 확인합니다.
        /// 매치된 블록의 수를 Count로 반환하고, 매치 유형은 Type, 매치된 좌표들을 matchedCoords로 반환합니다.
        /// </summary>
        private bool CheckForMatchAt(BoardManager boardManager, int x, int y, Block[,] blockArray, out int count, out int type, out HashSet<Vector2Int> matchedCoords)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (!gameBoard.BlockPlate.BlockPlateArray[y, x])
            {
                count = 0;
                type = 0;
                matchedCoords = null;
                return false;
            }

            var block = blockArray[y, x];
            if (block == null || !block.IsNormal)
            {
                count = 0;
                type = 0;
                matchedCoords = null;
                return false;
            }

            GemType gemType = block.GemType;
            int height = gameBoard.Height;
            int width = gameBoard.Width;
            bool isVMatched = false;
            bool isHMatched = false;
            matchedCoords = new HashSet<Vector2Int>();
            count = 0;
            type = 0;

            // 가로 매치 확인
            HashSet<Vector2Int> matchedHCoords = new HashSet<Vector2Int> { new Vector2Int(x, y) };
            for (int i = x - 1; i >= 0; i--) { var nextBlock = blockArray[y, i]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) matchedHCoords.Add(new Vector2Int(i, y)); else break; }
            for (int i = x + 1; i < width; i++) { var nextBlock = blockArray[y, i]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) matchedHCoords.Add(new Vector2Int(i, y)); else break; }
            if (matchedHCoords.Count >= 3)
            {
                isHMatched = true;
                foreach (var coord in matchedHCoords) matchedCoords.Add(coord);
            }

            // 세로 매치 확인
            HashSet<Vector2Int> matchedVCoords = new HashSet<Vector2Int> { new Vector2Int(x, y) };
            for (int i = y - 1; i >= 0; i--) { var nextBlock = blockArray[i, x]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) matchedVCoords.Add(new Vector2Int(x, i)); else break; }
            for (int i = y + 1; i < height; i++) { var nextBlock = blockArray[i, x]; if (nextBlock != null && nextBlock.IsNormal && nextBlock.GemType == gemType) matchedVCoords.Add(new Vector2Int(x, i)); else break; }
            if (matchedVCoords.Count >= 3)
            {
                isVMatched = true;
                foreach (var coord in matchedVCoords) matchedCoords.Add(coord);
            }

            if (!isHMatched && !isVMatched)
            {
                count = 0;
                type = 0;
                matchedCoords = null;
                return false;
            }

            if (isHMatched && isVMatched)
            {
                type = 3; // L/T 매치
            }
            else
            {
                type = 1; // 한줄 매치
            }

            count = matchedCoords.Count;
            return true;
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

        private bool CheckCubeMatchesAround(int x, int y, Block[,] blockArray, out HashSet<Vector2Int> matchedCoords)
        {
            // (x,y)가 포함될 수 있는 4개의 2x2 영역을 확인합니다.
            if (IsCubeMatched(BoardManager.Instance, blockArray, x, y, out matchedCoords)) return true;
            if (IsCubeMatched(BoardManager.Instance, blockArray, x - 1, y, out matchedCoords)) return true;
            if (IsCubeMatched(BoardManager.Instance, blockArray, x, y - 1, out matchedCoords)) return true;
            if (IsCubeMatched(BoardManager.Instance, blockArray, x - 1, y - 1, out matchedCoords)) return true;

            matchedCoords = new HashSet<Vector2Int>(); // No match found
            return false;
        }

        /// <summary>
        /// 지정된 위치에서 2x2 큐브 매치가 완성되었는지 확인합니다.
        /// 매치된 블록의 수를 Count로 반환하고, 매치 유형은 Type, 매치된 좌표들을 matchedCoords로 반환합니다.
        /// </summary>
        private bool IsCubeMatched(BoardManager boardManager, Block[,] blockArray, int x, int y, out HashSet<Vector2Int> matchedCoords)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            matchedCoords = new HashSet<Vector2Int>();

            // Check bounds to ensure we don't go out of the array
            if (x < 0 || y < 0 || x >= gameBoard.Width - 1 || y >= gameBoard.Height - 1)
            {
                return false;
            }

            var startBlock = blockArray[y, x];
            if (startBlock == null || !startBlock.IsNormal) return false;

            var startGemType = startBlock.GemType;


            for (int i = y; i < y + 2; i++)
            {
                for (int j = x; j < x + 2; j++)
                {
                    var currentBlock = blockArray[i, j];
                    if (currentBlock == null || !currentBlock.IsNormal || currentBlock.GemType != startGemType)
                    {
                        return false;
                    }
                }
            }

            for (int i = y; i < y + 2; i++)
            {
                for (int j = x; j < x + 2; j++)
                {
                    matchedCoords.Add(new Vector2Int(j, i));
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
            int finalScore = 0;

            if (BoardManager.Instance.MatchCombo.CurCombo > 1)
            {
                finalScore = (int)(score + BoardManager.Instance.MatchCombo.CurCombo * 0.5f * score);
            }
            else
            {
                finalScore = score;
            }

            return finalScore;
        }

        #endregion
    }
}