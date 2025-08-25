using SCR;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace KDJ
{
    public class BoardMatchChecker : MonoBehaviour
    {
        /// <summary>
        /// 모든 블럭의 매치 체크
        /// </summary>
        /// <param name="boardManager"></param>
        /// <returns>매칭되는 블럭이 있는지 여부 반환</returns>
        public bool AllBlockMatchCheck(BoardManager boardManager)
        {
            // 블럭 매치 체크 로직
            // x축부터 쭉 체크하고 y축도 체크

            bool isMatched = false;

            for (int y = 0; y < boardManager.Spawner.BlockPlate.BlockPlateHeight; y++)
            {
                int count = 1;
                int matchStartIndex = 0;
                GemType curGemType = (GemType)(-1);
                GemType prevGemType = (GemType)(-1);

                for (int x = 0; x < boardManager.Spawner.BlockPlate.BlockPlateWidth; x++)
                {
                    if (boardManager.Spawner.BlockPlate.BlockPlateArray[y, x] && boardManager.Spawner.BlockArray[y, x] != null)
                    {
                        curGemType = boardManager.Spawner.BlockArray[y, x].GemType;

                        if (curGemType == prevGemType)
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
                    isMatched = true;
                }
            }

            // y축 체크
            for (int x = 0; x < boardManager.Spawner.BlockPlate.BlockPlateWidth; x++)
            {
                int count = 1;
                int matchStartIndex = 0;
                GemType curGemType = (GemType)(-1);
                GemType prevGemType = (GemType)(-1);

                for (int y = 0; y < boardManager.Spawner.BlockPlate.BlockPlateHeight; y++)
                {

                    if (boardManager.Spawner.BlockPlate.BlockPlateArray[y, x] && boardManager.Spawner.BlockArray[y, x] != null)
                    {
                        curGemType = boardManager.Spawner.BlockArray[y, x].GemType;

                        if (curGemType == prevGemType)
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
                    isMatched = true;
                }
            }

            // 큐브 체크
            for (int y = 0; y < boardManager.Spawner.BlockPlate.BlockPlateHeight; y++)
            {
                for (int x = 0; x < boardManager.Spawner.BlockPlate.BlockPlateWidth; x++)
                {
                    if (boardManager.Spawner.BlockPlate.BlockPlateArray[y, x] && boardManager.Spawner.BlockArray[y, x] != null)
                    {
                        if (x + 1 < boardManager.Spawner.BlockPlate.BlockPlateWidth && y + 1 < boardManager.Spawner.BlockPlate.BlockPlateHeight)
                        {
                            if (boardManager.Spawner.BlockPlate.BlockPlateArray[y + 1, x] && boardManager.Spawner.BlockPlate.BlockPlateArray[y, x + 1]
                            && boardManager.Spawner.BlockArray[y + 1, x] != null && boardManager.Spawner.BlockArray[y, x + 1] != null
                            && boardManager.Spawner.BlockArray[y, x].BlockType == boardManager.Spawner.BlockArray[y + 1, x].BlockType
                            && boardManager.Spawner.BlockArray[y, x].BlockType == boardManager.Spawner.BlockArray[y, x + 1].BlockType)
                            {
                                if (IsCubeMatched(boardManager, x, y))
                                {
                                    isMatched = true;
                                }
                            }
                        }
                    }
                }
            }

            return isMatched;
        }


        /// <summary>
        /// 매치 여부를 체크하고 매치되었을 경우 파괴함
        /// </summary>
        /// <param name="boardManager"></param>
        public void AllMatchBlockDestroy(BoardManager boardManager)
        {
            int score = 0;
            // 블럭 매치 체크 로직
            // x축부터 쭉 체크하고 y축도 체크
            for (int y = 0; y < boardManager.Spawner.BlockPlate.BlockPlateHeight; y++)
            {
                int count = 1;
                int matchStartIndex = 0;
                GemType curGemType = (GemType)(-1);
                GemType prevGemType = (GemType)(-1);

                for (int x = 0; x < boardManager.Spawner.BlockPlate.BlockPlateWidth; x++)
                {

                    if (boardManager.Spawner.BlockPlate.BlockPlateArray[y, x] && boardManager.Spawner.BlockArray[y, x] != null)
                    {
                        curGemType = boardManager.Spawner.BlockArray[y, x].GemType;

                        if (curGemType == prevGemType)
                        {
                            count++;
                        }
                        else if (count < 3)
                        {
                            count = 1;
                            matchStartIndex = x;
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
                        if (boardManager.Spawner.BlockPlate.BlockPlateArray[y, i] && boardManager.Spawner.BlockArray[y, i].BlockInstance != null)
                        {
                            Debug.Log($"매칭 된 블럭 수: {count}");
                            Debug.Log($"X축 매치된 블록 파괴 위치: x={i}, y={y}");
                            Debug.Log($"매치된 블록 GemType: {boardManager.Spawner.BlockArray[y, i].GemType}");
                            Destroy(boardManager.Spawner.BlockArray[y, i].BlockInstance);
                            boardManager.Spawner.BlockArray[y, i].BlockInstance = null;
                            score += 10;
                        }
                    }

                    boardManager.MatchCombo.UpCombo();
                    if (boardManager.MatchCombo.CurCombo > 1)
                    {
                        boardManager.UpdateUI((int)(score * (0.5f * boardManager.MatchCombo.CurCombo)));
                    }
                    else
                    {
                        boardManager.UpdateUI(score);
                    }
                    score = 0;
                }
            }

            // y축 체크
            for (int x = 0; x < boardManager.Spawner.BlockPlate.BlockPlateWidth; x++)
            {
                int count = 1;
                int matchStartIndex = 0;
                GemType curGemType = (GemType)(-1);
                GemType prevGemType = (GemType)(-1);

                for (int y = 0; y < boardManager.Spawner.BlockPlate.BlockPlateHeight; y++)
                {
                    if (boardManager.Spawner.BlockPlate.BlockPlateArray[y, x] && boardManager.Spawner.BlockArray[y, x] != null)
                    {
                        curGemType = boardManager.Spawner.BlockArray[y, x].GemType;

                        if (curGemType == prevGemType)
                        {
                            count++;
                        }
                        else if (count < 3)
                        {
                            count = 1;
                            matchStartIndex = y;
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
                        if (boardManager.Spawner.BlockPlate.BlockPlateArray[i, x] && boardManager.Spawner.BlockArray[i, x] != null)
                        {
                            if (boardManager.Spawner.BlockArray[i, x].BlockInstance != null)
                            {
                                Debug.Log($"매칭 된 블럭 수: {count}");
                                Debug.Log($"Y축 매치된 블록 파괴 위치: x={x}, y={i}");
                                Debug.Log($"매치된 블록 GemType: {boardManager.Spawner.BlockArray[i, x].GemType}");
                                Destroy(boardManager.Spawner.BlockArray[i, x].BlockInstance);
                                boardManager.Spawner.BlockArray[i, x].BlockInstance = null;
                                score += 10;
                            }
                        }
                    }

                    boardManager.MatchCombo.UpCombo();
                    if (boardManager.MatchCombo.CurCombo > 1)
                    {
                        boardManager.UpdateUI((int)(score * (0.5f * boardManager.MatchCombo.CurCombo)));
                    }
                    else
                    {
                        boardManager.UpdateUI(score);
                    }
                    score = 0;
                }
            }

            // 큐브 체크
            for (int y = 0; y < boardManager.Spawner.BlockPlate.BlockPlateHeight; y++)
            {
                for (int x = 0; x < boardManager.Spawner.BlockPlate.BlockPlateWidth; x++)
                {
                    if (boardManager.Spawner.BlockArray[y, x] != null)
                    {
                        if (x + 1 < boardManager.Spawner.BlockPlate.BlockPlateWidth && y + 1 < boardManager.Spawner.BlockPlate.BlockPlateHeight)
                        {
                            if (boardManager.Spawner.BlockPlate.BlockPlateArray[y + 1, x] && boardManager.Spawner.BlockPlate.BlockPlateArray[y, x + 1]
                            && boardManager.Spawner.BlockArray[y + 1, x] != null && boardManager.Spawner.BlockArray[y, x + 1] != null
                            && boardManager.Spawner.BlockArray[y, x].BlockType == boardManager.Spawner.BlockArray[y + 1, x].BlockType
                            && boardManager.Spawner.BlockArray[y, x].BlockType == boardManager.Spawner.BlockArray[y, x + 1].BlockType)
                            {
                                CubeCheck(boardManager, x, y, 1);
                            }
                        }
                    }
                }
            }

            boardManager.UpdateUI(score);
        }

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
            if (xBlockBreakCount == 4)
            {
                boardManager.Spawner.SpawnBlock(blockGrid.x, blockGrid.y, 7);
            }
            else if (yBlockBreakCount == 4)
            {
                boardManager.Spawner.SpawnBlock(blockGrid.x, blockGrid.y, 6);
            }
        }

        public int XAxisMatchCheck(int x, int y, BoardManager boardManager)
        {
            // x축 매치 체크 로직
            //Debug.Log($"X축 매치 체크. 시작 좌표: x={x}, y={y}");

            int score = 0;
            int count = 1;
            int matchStartIndex = 0;
            GemType curGemType = (GemType)(-1);
            GemType prevGemType = (GemType)(-1);

            for (int i = -2; i < 3; i++)
            {
                int curIndex = x + i;

                //Debug.Log($"x축 매치 체크 인덱스: {curIndex}");

                if (boardManager.Spawner.BlockPlate.BlockPlateArray[y, x] && boardManager.Spawner.BlockArray.GetLength(1) > curIndex && curIndex >= 0 && boardManager.Spawner.BlockArray[y, curIndex] != null)
                {
                    curGemType = boardManager.Spawner.BlockArray[y, curIndex].GemType;

                    //Debug.Log($"x축 현재 GemType: {curGemType}, 이전 GemType: {prevGemType}");

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

            //Debug.Log("x축 매치 완료. 매치된 블록 수: " + count);

            if (count >= 3)
            {
                for (int i = matchStartIndex; i < matchStartIndex + count; i++)
                {
                    if (boardManager.Spawner.BlockPlate.BlockPlateArray[y, i] && boardManager.Spawner.BlockArray[y, i].BlockInstance != null)
                    {
                        Debug.Log($"매칭 된 블럭 수: {count}");
                        Debug.Log($"X축 매치된 블록 파괴 위치: x={i}, y={y}");
                        Debug.Log($"매치된 블록 GemType: {boardManager.Spawner.BlockArray[y, i].GemType}");
                        Destroy(boardManager.Spawner.BlockArray[y, i].BlockInstance);
                        boardManager.Spawner.BlockArray[y, i].BlockInstance = null;
                        score += 10;
                    }
                }

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

            return count;
        }

        public int YAxisMatchCheck(int x, int y, BoardManager boardManager)
        {
            // y축 매치 체크 로직
            //Debug.Log($"Y축 매치 체크. 시작 좌표: x={x}, y={y}");
            int score = 0;
            int count = 1;
            int matchStartIndex = 0;
            GemType curGemType = (GemType)(-1);
            GemType prevGemType = (GemType)(-1);

            for (int i = -2; i < 3; i++)
            {
                int curIndex = y + i;

                //Debug.Log($"y축 매치 체크 인덱스: {curIndex}");

                if (boardManager.Spawner.BlockPlate.BlockPlateArray[y, x] && boardManager.Spawner.BlockPlate.BlockPlateHeight > curIndex && curIndex >= 0 && boardManager.Spawner.BlockArray[curIndex, x] != null)
                {
                    curGemType = boardManager.Spawner.BlockArray[curIndex, x].GemType;

                    //Debug.Log($"y축 현재 GemType: {curGemType}, 이전 GemType: {prevGemType}");

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

            //Debug.Log("y축 매치 완료. 매치된 블록 수: " + count);

            if (count >= 3)
            {
                for (int i = matchStartIndex; i < matchStartIndex + count; i++)
                {
                    if (boardManager.Spawner.BlockPlate.BlockPlateArray[i, x] && boardManager.Spawner.BlockArray[i, x].BlockInstance != null)
                    {
                        Debug.Log($"매칭 된 블럭 수: {count}");
                        Debug.Log($"Y축 매치된 블록 파괴 위치: x={x}, y={i}");
                        Debug.Log($"매치된 블록 GemType: {boardManager.Spawner.BlockArray[i, x].GemType}");
                        Destroy(boardManager.Spawner.BlockArray[i, x].BlockInstance);
                        boardManager.Spawner.BlockArray[i, x].BlockInstance = null;
                        score += 10;
                    }
                }

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
                CubeCheck(boardManager, x, y, 1);
                CubeCheck(boardManager, x, y, 3);
            }
            else if (moveDirection.x == -1)
            {
                // 좌측 이동시 체크 로직
                CubeCheck(boardManager, x, y, 2);
                CubeCheck(boardManager, x, y, 4);
            }
            else if (moveDirection.y == 1)
            {
                // 상단 이동시 체크 로직
                CubeCheck(boardManager, x, y, 1);
                CubeCheck(boardManager, x, y, 2);
            }
            else if (moveDirection.y == -1)
            {
                // 하단 이동시 체크 로직
                CubeCheck(boardManager, x, y, 3);
                CubeCheck(boardManager, x, y, 4);
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
            int score = 0;
            int matchCount = 0;
            int offsetX = 0;
            int offsetY = 0;

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
                if (i < 0 || i >= boardManager.Spawner.BlockPlate.BlockPlateHeight)
                {
                    return false;
                }

                for (int j = offsetX; j < offsetX + 2; j++)
                {
                    if (j < 0 || j >= boardManager.Spawner.BlockPlate.BlockPlateWidth)
                    {
                        return false;
                    }

                    if (boardManager.Spawner.BlockPlate.BlockPlateArray[i, j] && boardManager.Spawner.BlockPlate.BlockPlateArray[y, x] && boardManager.Spawner.BlockArray[i, j] != null
                    && boardManager.Spawner.BlockArray[y, x] != null && boardManager.Spawner.BlockArray[i, j].BlockType == boardManager.Spawner.BlockArray[y, x].BlockType)
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
                        if (boardManager.Spawner.BlockArray[i, j] != null)
                        {
                            if (boardManager.Spawner.BlockArray[i, j].BlockInstance != null)
                            {
                                Destroy(boardManager.Spawner.BlockArray[i, j].BlockInstance);
                                boardManager.Spawner.BlockArray[i, j].BlockInstance = null;
                                score += 10;
                            }
                        }
                    }
                }

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

            return true;
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
            int matchCount = 0;
            bool isMatched = false;

            for (int i = y; i < y + 2; i++)
            {
                if (i < 0 || i >= boardManager.Spawner.BlockPlate.BlockPlateHeight)
                {
                    return isMatched;
                }

                for (int j = x; j < x + 2; j++)
                {
                    if (j < 0 || j >= boardManager.Spawner.BlockPlate.BlockPlateWidth)
                    {
                        return isMatched;
                    }

                    if (boardManager.Spawner.BlockPlate.BlockPlateArray[i, j] && boardManager.Spawner.BlockPlate.BlockPlateArray[y, x] && boardManager.Spawner.BlockArray[i, j] != null
                    && boardManager.Spawner.BlockArray[y, x] != null && boardManager.Spawner.BlockArray[i, j].BlockType == boardManager.Spawner.BlockArray[y, x].BlockType)
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
    }
}
