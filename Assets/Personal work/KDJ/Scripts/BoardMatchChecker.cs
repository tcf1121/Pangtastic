using LHJ;
using SCR;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KDJ
{
    public class BoardMatchChecker : MonoBehaviour
    {
        #region 배열 체크

        /// <summary>
        /// 모든 배열의 매치 가능성 체크 (가상 교환 방식)
        /// </summary>
        public bool AllBlockMatchPossibilityCheck(BoardManager boardManager, out int matchCount)
        {
            // 찾은 매치 가능성을 중복 없이 저장하기 위한 HashSet
            // 예: "(0,0):(0,1)" -> 0,0과 0,1 블록을 바꾸면 매치 가능
            var possibleMoves = new HashSet<string>();
            var blockArray = boardManager.Spawner.BlockArray;
            var blockPlate = boardManager.Spawner.BlockPlate;
            int height = blockPlate.BlockPlateHeight;
            int width = blockPlate.BlockPlateWidth;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 블럭판이 없거나 블럭이 없는 경우
                    if (!blockPlate.BlockPlateArray[y, x] || blockArray[y, x] == null) continue;

                    if (blockArray[y, x].GemType > GemType.Sugar && blockArray[y, x].GemType < GemType.Dust)
                    {
                        // 특수 블럭이 있는 경우 매치 가능 횟수에 1회 더함. 해당 좌표를 키로 추가 후 continue
                        string key = $"({x},{y})";
                        possibleMoves.Add(key);
                        continue;
                    }


                    // 오른쪽 블록과 교환 테스트 (상대방이 null이 아닌지 체크 추가)
                    if (x + 1 < width && blockPlate.BlockPlateArray[y, x + 1] && blockArray[y, x + 1] != null)
                    {
                        // 가상 교환
                        var temp = blockArray[y, x];
                        blockArray[y, x] = blockArray[y, x + 1];
                        blockArray[y, x + 1] = temp;

                        // 교환된 위치 주변에서 매치가 발생하는지 확인
                        if (CheckForMatchAt(boardManager, x, y) || CheckForMatchAt(boardManager, x + 1, y))
                        {
                            // 중복 저장을 막기 위해 항상 좌표를 정렬하여 키로 사용
                            string key = $"({x},{y}):({x + 1},{y})";
                            possibleMoves.Add(key);
                        }

                        // 교환 원상 복구
                        temp = blockArray[y, x];
                        blockArray[y, x] = blockArray[y, x + 1];
                        blockArray[y, x + 1] = temp;
                    }

                    // 아래쪽 블록과 교환 테스트 (상대방이 null이 아닌지 체크 추가)
                    if (y + 1 < height && blockPlate.BlockPlateArray[y + 1, x] && blockArray[y + 1, x] != null)
                    {
                        // 가상 교환
                        var temp = blockArray[y, x];
                        blockArray[y, x] = blockArray[y + 1, x];
                        blockArray[y + 1, x] = temp;

                        // 교환된 위치 주변에서 매치가 발생하는지 확인
                        if (CheckForMatchAt(boardManager, x, y) || CheckForMatchAt(boardManager, x, y + 1))
                        {
                            // 중복 저장을 막기 위해 항상 좌표를 정렬하여 키로 사용
                            string key = $"({x},{y}):({x},{y + 1})";
                            possibleMoves.Add(key);
                        }

                        // 교환 원상 복구
                        temp = blockArray[y, x];
                        blockArray[y, x] = blockArray[y + 1, x];
                        blockArray[y + 1, x] = temp;
                    }
                }
            }

            matchCount = possibleMoves.Count;
            return matchCount > 0;
        }

        /// <summary>
        /// 모든 블럭의 매치 체크
        /// </summary>
        /// <param name="boardManager"></param>
        /// <returns>매칭되는 블럭이 있는지 여부 반환</returns>
        public bool AllBlockMatchCheck(BoardManager boardManager)
        {
            int height = boardManager.Spawner.BlockPlate.BlockPlateHeight;
            int width = boardManager.Spawner.BlockPlate.BlockPlateWidth;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!CheckBlockIsAllValid(boardManager, x, y)) continue;
                    
                    var block = boardManager.Spawner.BlockArray[y, x];
                    if (block.GemType > GemType.Sugar && block.GemType < GemType.Dust) continue;

                    // 3개 이상의 가로 매치 확인
                    if (FindFullLineMatch(boardManager, x, y, true).Count >= 3)
                    {
                        return true;
                    }

                    // 3개 이상의 세로 매치 확인
                    if (FindFullLineMatch(boardManager, x, y, false).Count >= 3)
                    {
                        return true;
                    }
                    
                    // 2x2 큐브 매치 확인
                    if (x < width - 1 && y < height - 1)
                    {
                        if (IsCubeMatched(boardManager, x, y)) return true;
                    }
                }
            }

            return false;
        }

        public bool ProcessMatches(BoardManager boardManager, Vector2Int? swapPosition = null)
        {
            int height = boardManager.Spawner.BlockPlate.BlockPlateHeight;
            int width = boardManager.Spawner.BlockPlate.BlockPlateWidth;
            var blockArray = boardManager.Spawner.BlockArray;

            HashSet<Vector2Int> coordsToDestroy = new HashSet<Vector2Int>();
            (Vector2Int pos, int type)? specialToCreate = null;
            bool[,] visited = new bool[height, width];

            // 우선순위: 1x5 > L/T > 2x2 > 1x4

            // 1. 1x5 매치 탐색
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                    var hMatch = FindFullLineMatch(boardManager, x, y, true);
                    if (hMatch.Count >= 5)
                    {
                        if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 10);
                        foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                    }
                    var vMatch = FindFullLineMatch(boardManager, x, y, false);
                    if (vMatch.Count >= 5)
                    {
                        if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 10);
                        foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                    }
                }
            }

            // 2. L/T 매치 탐색
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                    var hMatch = FindFullLineMatch(boardManager, x, y, true);
                    var vMatch = FindFullLineMatch(boardManager, x, y, false);
                    if (hMatch.Count >= 3 && vMatch.Count >= 3)
                    {
                        if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 11);
                        foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                        foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                    }
                }
            }

            // 3. 2x2 매치 탐색
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

                        if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 9);
                        foreach (var c in coords) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                    }
                }
            }

            // 4. 1x4 매치 탐색
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                    var hMatch = FindFullLineMatch(boardManager, x, y, true);
                    if (hMatch.Count == 4)
                    {
                        if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 7);
                        foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                    }
                    var vMatch = FindFullLineMatch(boardManager, x, y, false);
                    if (vMatch.Count == 4)
                    {
                        if (!specialToCreate.HasValue) specialToCreate = (new Vector2Int(x, y), 8);
                        foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                    }
                }
            }

            // 5. 3매치 탐색
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[y, x] || !CheckBlockIsAllValid(boardManager, x, y)) continue;
                    var hMatch = FindFullLineMatch(boardManager, x, y, true);
                    if (hMatch.Count == 3)
                    {
                        foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                    }
                    var vMatch = FindFullLineMatch(boardManager, x, y, false);
                    if (vMatch.Count == 3)
                    {
                        foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; }
                    }
                }
            }

            // --- 파괴 및 생성 ---
            if (coordsToDestroy.Count > 0)
            {
                boardManager.MatchCombo.UpCombo();
                int score = 0;
                foreach (var coord in coordsToDestroy)
                {
                    if (CheckBlockIsAllValid(boardManager, coord.x, coord.y) && blockArray[coord.y, coord.x].BlockInstance != null)
                    {
                        Destroy(blockArray[coord.y, coord.x].BlockInstance);
                        blockArray[coord.y, coord.x].BlockInstance = null;
                        score += 10;
                    }
                }
                boardManager.UpdateUI(score);

                if (specialToCreate.HasValue)
                {
                    var creation = specialToCreate.Value;
                    Vector2Int spawnPos;

                    if (swapPosition.HasValue && coordsToDestroy.Contains(swapPosition.Value))
                    {
                        spawnPos = swapPosition.Value;
                    }
                    else
                    {
                        int minX = int.MaxValue;
                        int maxY = int.MinValue;
                        foreach (var coord in coordsToDestroy)
                        {
                            if (coord.x < minX) minX = coord.x;
                            if (coord.y > maxY) maxY = coord.y;
                        }
                        spawnPos = new Vector2Int(minX, maxY);

                        if (!coordsToDestroy.Contains(spawnPos))
                        {
                            spawnPos = creation.pos;
                        }
                    }
                    boardManager.Spawner.SpawnBlock(spawnPos.x, spawnPos.y, creation.type);
                }
                return true; 
            }

            return false; 
        }
        #endregion


        #region 유틸 함수
        /// <summary>
        /// 특정 위치의 블록을 기준으로 가로/세로 3개 이상 매치가 있는지 확인하는 도우미 함수
        /// </summary>
        private bool CheckForMatchAt(BoardManager boardManager, int x, int y)
        {
            var blockPlate = boardManager.Spawner.BlockPlate;
            if (!blockPlate.BlockPlateArray[y, x])
            {
                return false;
            }

            var block = boardManager.Spawner.BlockArray[y, x];
            if (block == null || block.IsObstacle || (block.GemType > GemType.Sugar && block.GemType < GemType.Dust))
            {
                return false;
            }

            GemType gemType = block.GemType;
            int height = boardManager.Spawner.BlockPlate.BlockPlateHeight;
            int width = boardManager.Spawner.BlockPlate.BlockPlateWidth;

            // 가로 체크
            int horizontalCount = 1;
            // 왼쪽으로 체크
            for (int i = x - 1; i >= 0; i--)
            {
                if (!blockPlate.BlockPlateArray[y, i]) break;
                var nextBlock = boardManager.Spawner.BlockArray[y, i];
                if (nextBlock != null && !nextBlock.IsObstacle && !(nextBlock.GemType > GemType.Sugar && nextBlock.GemType < GemType.Dust) && nextBlock.GemType == gemType)
                    horizontalCount++;
                else
                    break;
            }
            // 오른쪽으로 체크
            for (int i = x + 1; i < width; i++)
            {
                if (!blockPlate.BlockPlateArray[y, i]) break;
                var nextBlock = boardManager.Spawner.BlockArray[y, i];
                if (nextBlock != null && !nextBlock.IsObstacle && !(nextBlock.GemType > GemType.Sugar && nextBlock.GemType < GemType.Dust) && nextBlock.GemType == gemType)
                    horizontalCount++;
                else
                    break;
            }

            if (horizontalCount >= 3) return true;

            // 세로 체크
            int verticalCount = 1;
            // 위쪽으로 체크
            for (int i = y - 1; i >= 0; i--)
            {
                if (!blockPlate.BlockPlateArray[i, x]) break;
                var nextBlock = boardManager.Spawner.BlockArray[i, x];
                if (nextBlock != null && !nextBlock.IsObstacle && !(nextBlock.GemType > GemType.Sugar && nextBlock.GemType < GemType.Dust) && nextBlock.GemType == gemType)
                    verticalCount++;
                else
                    break;
            }
            // 아래쪽으로 체크
            for (int i = y + 1; i < height; i++)
            {
                if (!blockPlate.BlockPlateArray[i, x]) break;
                var nextBlock = boardManager.Spawner.BlockArray[i, x];
                if (nextBlock != null && !nextBlock.IsObstacle && !(nextBlock.GemType > GemType.Sugar && nextBlock.GemType < GemType.Dust) && nextBlock.GemType == gemType)
                    verticalCount++;
                else
                    break;
            }

            if (verticalCount >= 3) return true;

            return false;
        }

        /// <summary>
        /// 배열에 매칭가능한 2x2 배열이 있는지 체크
        /// </summary>
        /// <param name="boardManager"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsCubeMatched(BoardManager boardManager, int x, int y)
        {
            int height = boardManager.Spawner.BlockPlate.BlockPlateHeight;
            int width = boardManager.Spawner.BlockPlate.BlockPlateWidth;
            var blockArray = boardManager.Spawner.BlockArray;
            var blockPlateArray = boardManager.Spawner.BlockPlate.BlockPlateArray;

            int matchCount = 0;
            bool isMatched = false;
            
            var startBlock = blockArray[y,x];
            if(startBlock == null || (startBlock.GemType > GemType.Sugar && startBlock.GemType < GemType.Dust))
            {
                return false;
            }

            for (int i = y; i < y + 2; i++)
            {
                if (i < 0 || i >= height)
                {
                    return isMatched;
                }

                for (int j = x; j < x + 2; j++)
                {
                    if (j < 0 || j >= width)
                    {
                        return isMatched;
                    }
                    
                    var currentBlock = blockArray[i,j];
                    if (blockPlateArray[i, j] && currentBlock != null && !(currentBlock.GemType > GemType.Sugar && currentBlock.GemType < GemType.Dust) 
                        && currentBlock.BlockType == startBlock.BlockType)
                    {
                        matchCount++;
                    }
                }
            }

            if (matchCount == 4)
            {
                isMatched = true;
            }

            return isMatched;
        }

        private List<Vector2Int> FindFullLineMatch(BoardManager boardManager, int startX, int startY, bool isHorizontal)
        {
            var matches = new List<Vector2Int>();
            var startBlock = boardManager.Spawner.BlockArray[startY, startX];

            if (!CheckBlockIsAllValid(boardManager, startX, startY) || startBlock.IsObstacle || (startBlock.GemType > GemType.Sugar && startBlock.GemType < GemType.Dust))
                return matches;

            var startType = startBlock.GemType;
            matches.Add(new Vector2Int(startX, startY));

            if (isHorizontal)
            {
                // 왼쪽 탐색
                for (int x = startX - 1; x >= 0; x--)
                {
                    var currentBlock = boardManager.Spawner.BlockArray[startY, x];
                    if (CheckBlockIsAllValid(boardManager, x, startY) && !currentBlock.IsObstacle && !(currentBlock.GemType > GemType.Sugar && currentBlock.GemType < GemType.Dust) && currentBlock.GemType == startType)
                        matches.Add(new Vector2Int(x, startY));
                    else break;
                }
                // 오른쪽 탐색
                for (int x = startX + 1; x < boardManager.Spawner.BlockPlate.BlockPlateWidth; x++)
                {
                    var currentBlock = boardManager.Spawner.BlockArray[startY, x];
                    if (CheckBlockIsAllValid(boardManager, x, startY) && !currentBlock.IsObstacle && !(currentBlock.GemType > GemType.Sugar && currentBlock.GemType < GemType.Dust) && currentBlock.GemType == startType)
                        matches.Add(new Vector2Int(x, startY));
                    else break;
                }
            }
            else // 세로
            {
                // 위쪽 탐색
                for (int y = startY - 1; y >= 0; y--)
                {
                    var currentBlock = boardManager.Spawner.BlockArray[y, startX];
                    if (CheckBlockIsAllValid(boardManager, startX, y) && !currentBlock.IsObstacle && !(currentBlock.GemType > GemType.Sugar && currentBlock.GemType < GemType.Dust) && currentBlock.GemType == startType)
                        matches.Add(new Vector2Int(startX, y));
                    else break;
                }
                // 아래쪽 탐색
                for (int y = startY + 1; y < boardManager.Spawner.BlockPlate.BlockPlateHeight; y++)
                {
                    var currentBlock = boardManager.Spawner.BlockArray[y, startX];
                    if (CheckBlockIsAllValid(boardManager, startX, y) && !currentBlock.IsObstacle && !(currentBlock.GemType > GemType.Sugar && currentBlock.GemType < GemType.Dust) && currentBlock.GemType == startType)
                        matches.Add(new Vector2Int(startX, y));
                    else break;
                }
            }
            return matches;
        }

        /// <summary>
        /// 배열 인덱스가 유효한지 체크
        /// </summary>
        /// <param name="boardManager"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool CheckInOfArray(BoardManager boardManager, int x, int y)
        {
            int height = boardManager.Spawner.BlockPlate.BlockPlateHeight;
            int width = boardManager.Spawner.BlockPlate.BlockPlateWidth;

            if (y >= 0 && y < height && x >= 0 && x < width)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 배열 인덱스의 위치에 블럭이 존재하는지 체크
        /// </summary>
        /// <param name="boardManager"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool CheckBlockIsValue(BoardManager boardManager, int x, int y)
        {
            if (boardManager.Spawner.BlockPlate.BlockPlateArray[y, x] && boardManager.Spawner.BlockArray[y, x] != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 배열 인덱스가 유효하고 해당 위치에 블럭이 존재하는지 체크
        /// </summary>
        /// <param name="boardManager"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool CheckBlockIsAllValid(BoardManager boardManager, int x, int y)
        {
            return CheckInOfArray(boardManager, x, y) && CheckBlockIsValue(boardManager, x, y);
        }

        #endregion
    }
}
