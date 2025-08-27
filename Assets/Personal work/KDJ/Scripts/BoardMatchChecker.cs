using LHJ;
using SCR;
using System.Collections.Generic;
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
            int height = boardManager.Spawner.BlockPlate.BlockPlateHeight;
            int width = boardManager.Spawner.BlockPlate.BlockPlateWidth;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (blockArray[y, x].GemType > GemType.Sugar && blockArray[y, x].GemType < GemType.Dough)
                    {
                        // 특수 블럭이 있는 경우 매치 가능 횟수에 1회 더함. 해당 좌표를 키로 추가 후 continue
                        string key = $"({x},{y})";
                        possibleMoves.Add(key);
                        continue;
                    }

                    // 오른쪽 블록과 교환 테스트
                        if (x + 1 < width)
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

                    // 아래쪽 블록과 교환 테스트
                    if (y + 1 < height)
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
            var blockArray = boardManager.Spawner.BlockArray;
            var blockPlateArray = boardManager.Spawner.BlockPlate.BlockPlateArray;

            bool isMatched = false;
            
            // x축 체크
            for (int y = 0; y < height; y++)
            {
                int matchStartIndex = 0;
                int count = 1;
                GemType curGemType = (GemType)(-1);
                GemType prevGemType = (GemType)(-1);

                for (int x = 0; x < width; x++)
                {
                    if (blockPlateArray[y, x] && blockArray[y, x] != null)
                    {
                        curGemType = blockArray[y, x].GemType;

                        if (curGemType == prevGemType && !blockArray[y, x].IsObstacle)
                        {
                            count++;
                        }
                        else if (count < 3)
                        {
                            count = 1;
                            matchStartIndex = x;
                        }
                    }
                    prevGemType = curGemType;
                }

                if (count >= 3)
                {
                    Debug.Log($"X축 매치 감지");
                    Debug.Log($"매치된 블록 위치: x={matchStartIndex}~{matchStartIndex + count - 1}, y={y}");
                    Debug.Log($"매치된 블록들 GemType: {blockArray[y, matchStartIndex].GemType}, {blockArray[y, matchStartIndex + 1].GemType}, {blockArray[y, matchStartIndex + 2].GemType}");
                    isMatched = true;
                }
            }

            // y축 체크
            for (int x = 0; x < width; x++)
            {
                int matchStartIndex = 0;
                int count = 1;
                GemType curGemType = (GemType)(-1);
                GemType prevGemType = (GemType)(-1);

                for (int y = 0; y < height; y++)
                {

                    if (blockPlateArray[y, x] && blockArray[y, x] != null)
                    {
                        curGemType = blockArray[y, x].GemType;

                        if (curGemType == prevGemType && !blockArray[y, x].IsObstacle)
                        {
                            count++;
                        }
                        else if (count < 3)
                        {
                            count = 1;
                            matchStartIndex = y;
                        }
                    }
                    prevGemType = curGemType;
                }

                if (count >= 3)
                {
                    Debug.Log($"Y축 매치 감지");
                    Debug.Log($"매치된 블록 위치: x={x}, y={matchStartIndex}~{matchStartIndex + count - 1}");
                    Debug.Log($"매치된 블록들 GemType: {blockArray[matchStartIndex, x].GemType}, {blockArray[matchStartIndex + 1, x].GemType}, {blockArray[matchStartIndex + 2, x].GemType}");
                    isMatched = true;
                }
            }

            // 큐브 체크
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (blockPlateArray[y, x] && blockArray[y, x] != null)
                    {
                        if (x + 1 < width && y + 1 < height)
                        {
                            if (blockPlateArray[y + 1, x] && blockPlateArray[y, x + 1]
                            && blockArray[y + 1, x] != null && blockArray[y, x + 1] != null
                            && blockArray[y, x].BlockType == blockArray[y + 1, x].BlockType
                            && blockArray[y, x].BlockType == blockArray[y, x + 1].BlockType)
                            {
                                if (IsCubeMatched(boardManager, x, y) && !blockArray[y, x].IsObstacle)
                                {
                                    Debug.Log($"큐브 매치 감지");
                                    isMatched = true;
                                }
                            }
                        }
                    }
                }
            }

            Debug.Log($"매칭 여부: {isMatched}");
            return isMatched;
        }


        /// <summary>
        /// 매치 여부를 체크하고 매치되었을 경우 파괴함
        /// </summary>
        /// <param name="boardManager"></param>
        public void AllMatchBlockDestroy(BoardManager boardManager)
        {
            int height = boardManager.Spawner.BlockPlate.BlockPlateHeight;
            int width = boardManager.Spawner.BlockPlate.BlockPlateWidth;
            var blockArray = boardManager.Spawner.BlockArray;
            var blockPlateArray = boardManager.Spawner.BlockPlate.BlockPlateArray;

            // x,y 통합 체크
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (blockPlateArray[y, x] && blockArray[y, x] != null && !blockArray[y, x].IsObstacle)
                    {
                        int xMatchCount = XAxisMatchCheck(x, y, boardManager);
                        int yMatchCount = YAxisMatchCheck(x, y, boardManager);
                        bool isCubeMatched = CubeCheck(boardManager, x, y, 1);

                        if (xMatchCount == 5 || yMatchCount == 5)
                        {
                            boardManager.Spawner.SpawnBlock(x, y, 10);
                        }
                        else if (5 > xMatchCount && xMatchCount >= 3 && 5 > yMatchCount && yMatchCount >= 3)
                        {
                            boardManager.Spawner.SpawnBlock(x, y, 11);
                        }
                        else if (isCubeMatched)
                        {
                            boardManager.Spawner.SpawnBlock(x, y, 9);
                        }
                        else if (xMatchCount == 4)
                        {
                            boardManager.Spawner.SpawnBlock(x, y, 7);
                        }
                        else if (yMatchCount == 4)
                        {
                            boardManager.Spawner.SpawnBlock(x, y, 8);
                        }
                    }
                }
            }
        }

        #endregion


        #region 블럭 매치 체크
        /// <summary>
        /// 블럭 매치 체크 로직. x,y,큐브 형태의 매치를 전부 체크함
        /// </summary>
        /// <param name="blockGrid"></param>
        /// <param name="boardManager"></param>
        public void BlockMatchCheck(Vector2Int blockGrid, BoardManager boardManager)
        {
            // 블럭 매치 체크 로직
            int xBlockBreakCount = XAxisMatchCheck(blockGrid.x, blockGrid.y, boardManager);
            int yBlockBreakCount = YAxisMatchCheck(blockGrid.x, blockGrid.y, boardManager);
            bool isCubeMatched = CubeMatchCheck(blockGrid.x, blockGrid.y, boardManager);
            Debug.Log("큐브 매치 결과: " + isCubeMatched);
            if (xBlockBreakCount == 5 || yBlockBreakCount == 5)
            {
                boardManager.Spawner.SpawnBlock(blockGrid.x, blockGrid.y, 10);
            }
            else if (5 > xBlockBreakCount && xBlockBreakCount >= 3 && 5 > yBlockBreakCount && yBlockBreakCount >= 3)
            {
                boardManager.Spawner.SpawnBlock(blockGrid.x, blockGrid.y, 11);
            }
            else if (isCubeMatched)
            {
                boardManager.Spawner.SpawnBlock(blockGrid.x, blockGrid.y, 9);
            }
            else if (xBlockBreakCount == 4)
            {
                boardManager.Spawner.SpawnBlock(blockGrid.x, blockGrid.y, 7);
            }
            else if (yBlockBreakCount == 4)
            {
                boardManager.Spawner.SpawnBlock(blockGrid.x, blockGrid.y, 8);
            }
        }

        public int XAxisMatchCheck(int x, int y, BoardManager boardManager)
        {
            var blockArray = boardManager.Spawner.BlockArray;
            var blockPlateArray = boardManager.Spawner.BlockPlate.BlockPlateArray;
            int width = boardManager.Spawner.BlockPlate.BlockPlateWidth;
            int height = boardManager.Spawner.BlockPlate.BlockPlateHeight;

            int score = 0;
            int count = 1;
            int matchStartIndex = 0;
            bool isMatched = false;
            GemType curGemType = (GemType)(-1);
            GemType prevGemType = (GemType)(-1);

            for (int i = -2; i < 3; i++)
            {
                int curIndex = x + i;

                if (blockPlateArray[y, x] && width > curIndex && curIndex >= 0 && blockArray[y, curIndex] != null)
                {
                    curGemType = blockArray[y, curIndex].GemType;

                    if (curGemType == prevGemType)
                    {
                        count++;
                    }
                    else if (count < 3)
                    {
                        count = 1;
                        matchStartIndex = curIndex;
                    }
                    else if (curGemType != prevGemType && count >= 3)
                    {
                        break; // 매치가 끊기고 이전에 3개 이상 매치된 경우 루프 종료
                    }
                }
                prevGemType = curGemType;
            }

            if (count >= 3)
            {
                for (int i = matchStartIndex; i < matchStartIndex + count; i++)
                {
                    if (blockPlateArray[y, i] && blockArray[y, i] != null && blockArray[y, i].BlockInstance != null)
                    {
                        Debug.Log($"매칭 된 블럭 수: {count}");
                        Debug.Log($"X축 매치된 블록 파괴 위치: x={i}, y={y}");
                        Debug.Log($"매치된 블록 GemType: {blockArray[y, i].GemType}");
                        Destroy(blockArray[y, i].BlockInstance);
                        blockArray[y, i].BlockInstance = null;
                        if (y + 1 < height && blockArray[y + 1, i] is Cloche)
                        {
                            (blockArray[y + 1, i] as Cloche).TakeDamage(boardManager);
                        }
                        if (y - 1 >= 0 && blockArray[y - 1, i] is Cloche)
                        {
                            (blockArray[y - 1, i] as Cloche).TakeDamage(boardManager);
                        }
                        if (i + 1 < width && blockArray[y, i + 1] is Cloche)
                        {
                            (blockArray[y, i + 1] as Cloche).TakeDamage(boardManager);
                        }
                        if (i - 1 >= 0 && blockArray[y, i - 1] is Cloche)
                        {
                            (blockArray[y, i - 1] as Cloche).TakeDamage(boardManager);
                        }
                        score += 10;
                        isMatched = true;
                    }
                }

                if (isMatched)
                {
                    boardManager.MatchCombo.UpCombo();
                    if (boardManager.MatchCombo.CurCombo > 1)
                    {
                        boardManager.UpdateUI((int)(score * (0.5f * boardManager.MatchCombo.CurCombo)));
                    }
                    else
                    {
                        boardManager.UpdateUI(score);
                    }
                }
            }

            return count;
        }

        public int YAxisMatchCheck(int x, int y, BoardManager boardManager)
        {
            var blockArray = boardManager.Spawner.BlockArray;
            var blockPlateArray = boardManager.Spawner.BlockPlate.BlockPlateArray;
            int width = boardManager.Spawner.BlockPlate.BlockPlateWidth;
            int height = boardManager.Spawner.BlockPlate.BlockPlateHeight;

            int score = 0;
            int count = 1;
            int matchStartIndex = 0;
            bool isMatched = false;
            GemType curGemType = (GemType)(-1);
            GemType prevGemType = (GemType)(-1);

            for (int i = -2; i < 3; i++)
            {
                int curIndex = y + i;

                if (blockPlateArray[y, x] && height > curIndex && curIndex >= 0 && blockArray[curIndex, x] != null)
                {
                    curGemType = blockArray[curIndex, x].GemType;

                    if (curGemType == prevGemType)
                    {
                        count++;
                    }
                    else if (count < 3)
                    {
                        count = 1;
                        matchStartIndex = curIndex;
                    }
                    else if (curGemType != prevGemType && count >= 3)
                    {
                        break; // 매치가 끊기고 이전에 3개 이상 매치된 경우 루프 종료
                    }
                }
                prevGemType = curGemType;
            }

            if (count >= 3)
            {
                for (int i = matchStartIndex; i < matchStartIndex + count; i++)
                {
                    if (blockPlateArray[i, x] && blockArray[i, x] != null && blockArray[i, x].BlockInstance != null)
                    {
                        Debug.Log($"매칭 된 블럭 수: {count}");
                        Debug.Log($"Y축 매치된 블록 파괴 위치: x={x}, y={i}");
                        Debug.Log($"매치된 블록 GemType: {blockArray[i, x].GemType}");
                        Destroy(blockArray[i, x].BlockInstance);
                        blockArray[i, x].BlockInstance = null;
                        if (i + 1 < height && blockArray[i + 1, x] is Cloche)
                        {
                            (blockArray[i + 1, x] as Cloche).TakeDamage(boardManager);
                        }
                        if (i - 1 >= 0 && blockArray[i - 1, x] is Cloche)
                        {
                            (blockArray[i - 1, x] as Cloche).TakeDamage(boardManager);
                        }
                        if (x + 1 < width && blockArray[i, x + 1] is Cloche)
                        {
                            (blockArray[i, x + 1] as Cloche).TakeDamage(boardManager);
                        }
                        if (x - 1 >= 0 && blockArray[i, x - 1] is Cloche)
                        {
                            (blockArray[i, x - 1] as Cloche).TakeDamage(boardManager);
                        }
                        score += 10;
                        isMatched = true;
                    }
                }

                if (isMatched)
                {
                    boardManager.MatchCombo.UpCombo();
                    if (boardManager.MatchCombo.CurCombo > 1)
                    {
                        boardManager.UpdateUI((int)(score * (0.5f * boardManager.MatchCombo.CurCombo)));
                    }
                    else
                    {
                        boardManager.UpdateUI(score);
                    }
                }
            }

            return count;
        }

        public bool CubeMatchCheck(int x, int y, BoardManager boardManager)
        {
            // 2x2 매치 체크 로직
            bool isMatched = false;
            // 이동 방향을 기준으로 6칸만 체크하면됨.
            Vector2Int moveDirection = boardManager.BlockMover.EndBlockPos - boardManager.BlockMover.StartBlockPos;

            //대각선은 나오지 않고 무조건 x,y중 하나만 값이 들어가므로 4방향에서의 조건만 체크
            if (moveDirection.x == 1)
            {
                // 우측 이동시 체크 로직
                if (CubeCheck(boardManager, x, y, 1) || CubeCheck(boardManager, x, y, 3))
                {
                    isMatched = true;
                }
            }
            else if (moveDirection.x == -1)
            {
                // 좌측 이동시 체크 로직
                if (CubeCheck(boardManager, x, y, 2) || CubeCheck(boardManager, x, y, 4))
                {
                    isMatched = true;
                }
            }
            else if (moveDirection.y == 1)
            {
                // 상단 이동시 체크 로직
                if (CubeCheck(boardManager, x, y, 1) || CubeCheck(boardManager, x, y, 2))
                {
                    isMatched = true;
                }
            }
            else if (moveDirection.y == -1)
            {
                // 하단 이동시 체크 로직
                if (CubeCheck(boardManager, x, y, 3) || CubeCheck(boardManager, x, y, 4))
                {
                    isMatched = true;
                }
            }

            return isMatched;
        }

        /// <summary>
        /// startarray값으로 위치를 지정하면 해당 위치에서 2x2 매치 체크, startArray에는 1~4의 값을 넣어야 함
        /// </summary>
        /// <param name="boardManager"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="startArray">1 = 좌측 하단, 2 = 우측 하단, 3 = 좌측 상단, 4 = 우측 상단</param>
        private bool CubeCheck(BoardManager boardManager, int x, int y, int startArray = 1)
        {
            int height = boardManager.Spawner.BlockPlate.BlockPlateHeight;
            int width = boardManager.Spawner.BlockPlate.BlockPlateWidth;
            var blockArray = boardManager.Spawner.BlockArray;
            var blockPlateArray = boardManager.Spawner.BlockPlate.BlockPlateArray;

            int score = 0;
            int matchCount = 0;
            int offsetX = 0;
            int offsetY = 0;
            bool isMatched = false;

            switch (startArray)
            {
                case 1: // 좌측 하단
                    offsetX = x;
                    offsetY = y;
                    break;
                case 2: // 우측 하단
                    offsetX = x - 1;
                    offsetY = y;
                    break;
                case 3: // 좌측 상단
                    offsetX = x;
                    offsetY = y - 1;
                    break;
                case 4: // 우측 상단
                    offsetX = x - 1;
                    offsetY = y - 1;
                    break;
                default:
                    Debug.LogError("CubeCheck의 startArray 파라미터는 1~4 사이의 값이어야 합니다.");
                    return false;
            }

            // 매치 체크
            for (int i = offsetY; i < offsetY + 2; i++)
            {
                if (i < 0 || i >= height)
                {
                    return false;
                }

                for (int j = offsetX; j < offsetX + 2; j++)
                {
                    if (j < 0 || j >= width)
                    {
                        return false;
                    }

                    if (blockPlateArray[i, j] && blockPlateArray[y, x] && blockArray[i, j] != null
                    && blockArray[y, x] != null && blockArray[i, j].BlockType == blockArray[y, x].BlockType)
                    {
                        matchCount++;
                    }
                }
            }

            if (matchCount == 4)
            {
                // 2x2 블록 매치 처리
                for (int i = offsetY; i < offsetY + 2; i++)
                {
                    for (int j = offsetX; j < offsetX + 2; j++)
                    {
                        if (blockArray[i, j] != null)
                        {
                            if (blockArray[i, j].BlockInstance != null)
                            {
                                Destroy(blockArray[i, j].BlockInstance);
                                blockArray[i, j].BlockInstance = null;
                                if (i + 1 < height && blockArray[i + 1, j] is Cloche)
                                {
                                    (blockArray[i + 1, j] as Cloche).TakeDamage(boardManager);
                                }
                                if (i - 1 >= 0 && blockArray[i - 1, j] is Cloche)
                                {
                                    (blockArray[i - 1, j] as Cloche).TakeDamage(boardManager);
                                }
                                if (j + 1 < width && blockArray[i, j + 1] is Cloche)
                                {
                                    (blockArray[i, j + 1] as Cloche).TakeDamage(boardManager);
                                }
                                if (j - 1 >= 0 && blockArray[i, j - 1] is Cloche)
                                {
                                    (blockArray[i, j - 1] as Cloche).TakeDamage(boardManager);
                                }
                                score += 10;
                                isMatched = true;
                            }
                        }
                    }
                }

                if (isMatched)
                {
                    boardManager.MatchCombo.UpCombo();
                    if (boardManager.MatchCombo.CurCombo > 1)
                    {
                        boardManager.UpdateUI((int)(score * (0.5f * boardManager.MatchCombo.CurCombo)));
                    }
                    else
                    {
                        boardManager.UpdateUI(score);
                    }
                }
            }

            return isMatched;
        }
        #endregion

        #region 유틸 함수
        /// <summary>
        /// 특정 위치의 블록을 기준으로 가로/세로 3개 이상 매치가 있는지 확인하는 도우미 함수
        /// </summary>
        private bool CheckForMatchAt(BoardManager boardManager, int x, int y)
        {
            var block = boardManager.Spawner.BlockArray[y, x];
            if (block == null || block.IsObstacle)
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
                var nextBlock = boardManager.Spawner.BlockArray[y, i];
                if (nextBlock != null && !nextBlock.IsObstacle && nextBlock.GemType == gemType)
                    horizontalCount++;
                else
                    break;
            }
            // 오른쪽으로 체크
            for (int i = x + 1; i < width; i++)
            {
                var nextBlock = boardManager.Spawner.BlockArray[y, i];
                if (nextBlock != null && !nextBlock.IsObstacle && nextBlock.GemType == gemType)
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
                var nextBlock = boardManager.Spawner.BlockArray[i, x];
                if (nextBlock != null && !nextBlock.IsObstacle && nextBlock.GemType == gemType)
                    verticalCount++;
                else
                    break;
            }
            // 아래쪽으로 체크
            for (int i = y + 1; i < height; i++)
            {
                var nextBlock = boardManager.Spawner.BlockArray[i, x];
                if (nextBlock != null && !nextBlock.IsObstacle && nextBlock.GemType == gemType)
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

                    if (blockPlateArray[i, j] && blockPlateArray[y, x] && blockArray[i, j] != null
                    && blockArray[y, x] != null && blockArray[i, j].BlockType == blockArray[y, x].BlockType)
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
