using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

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
                if (boardManager.Spawner.BlockArray[y, x] != null)
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
                // for (int i = matchStartIndex; i < matchStartIndex + count; i++)
                // {
                //     Destroy(boardManager.Spawner.BlockArray[y, i].BlockInstance);
                //     boardManager.Spawner.BlockArray[y, i].BlockInstance = null;
                // }
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

                if (boardManager.Spawner.BlockArray[y, x] != null)
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
                // for (int i = matchStartIndex; i < matchStartIndex + count; i++)
                // {
                //     Destroy(boardManager.Spawner.BlockArray[i, x].BlockInstance);
                //     boardManager.Spawner.BlockArray[i, x].BlockInstance = null;
                // }
                isMatched = true;
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

                if (boardManager.Spawner.BlockArray[y, x] != null)
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
                for (int i = matchStartIndex; i < matchStartIndex + count; i++)
                {
                    Destroy(boardManager.Spawner.BlockArray[y, i].BlockInstance);
                    boardManager.Spawner.BlockArray[y, i].BlockInstance = null;
                }
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
                if (boardManager.Spawner.BlockArray[y, x] != null)
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
                for (int i = matchStartIndex; i < matchStartIndex + count; i++)
                {
                    Destroy(boardManager.Spawner.BlockArray[i, x].BlockInstance);
                    boardManager.Spawner.BlockArray[i, x].BlockInstance = null;
                }
            }
        }
    }

    public void BlockMatchCheck(Vector2Int blockGrid, BoardManager boardManager)
    {
        // 블럭 매치 체크 로직
        int xBlockBreakCount = XAxisMatchCheck(blockGrid.x, blockGrid.y, boardManager);
        int yBlockBreakCount = YAxisMatchCheck(blockGrid.x, blockGrid.y, boardManager);
        bool isCubeMatched = CubeMatchCheck(blockGrid.x, blockGrid.y, boardManager);
    }

    public int XAxisMatchCheck(int x, int y, BoardManager boardManager)
    {
        // x축 매치 체크 로직
        Debug.Log($"X축 매치 체크. 시작 좌표: x={x}, y={y}");

        int count = 1;
        int matchStartIndex = 0;
        GemType curGemType = (GemType)(-1);
        GemType prevGemType = (GemType)(-1);

        for (int i = -2; i < 3; i++)
        {
            int curIndex = x + i;

            Debug.Log($"x축 매치 체크 인덱스: {curIndex}");

            if (boardManager.Spawner.BlockArray.GetLength(1) > curIndex && curIndex >= 0 && boardManager.Spawner.BlockArray[y, curIndex] != null)
            {
                curGemType = boardManager.Spawner.BlockArray[y, curIndex].GemType;

                Debug.Log($"x축 현재 GemType: {curGemType}, 이전 GemType: {prevGemType}");

                if (curGemType == prevGemType)
                {
                    count++;
                }
                else if (count < 3)
                {
                    count = 1;
                    matchStartIndex = curIndex;
                }
            }
            prevGemType = curGemType;
        }

        Debug.Log("x축 매치 완료. 매치된 블록 수: " + count);

        if (count >= 3)
        {
            for (int i = matchStartIndex; i < matchStartIndex + count; i++)
            {
                Destroy(boardManager.Spawner.BlockArray[y, i].BlockInstance);
                boardManager.Spawner.BlockArray[y, i].BlockInstance = null;
            }
        }

        return count;
    }

    public int YAxisMatchCheck(int x, int y, BoardManager boardManager)
    {
        // y축 매치 체크 로직
        Debug.Log($"Y축 매치 체크. 시작 좌표: x={x}, y={y}");
        int count = 1;
        int matchStartIndex = 0;
        GemType curGemType = (GemType)(-1);
        GemType prevGemType = (GemType)(-1);

        for (int i = -2; i < 3; i++)
        {
            int curIndex = y + i;

            Debug.Log($"y축 매치 체크 인덱스: {curIndex}");

            if (boardManager.Spawner.BlockPlate.BlockPlateHeight > curIndex && curIndex >= 0 && boardManager.Spawner.BlockArray[curIndex, x] != null)
            {
                curGemType = boardManager.Spawner.BlockArray[curIndex, x].GemType;

                Debug.Log($"y축 현재 GemType: {curGemType}, 이전 GemType: {prevGemType}");

                if (curGemType == prevGemType)
                {
                    count++;
                }
                else if (count < 3)
                {
                    count = 1;
                    matchStartIndex = curIndex;
                }
            }
            prevGemType = curGemType;
        }

        Debug.Log("y축 매치 완료. 매치된 블록 수: " + count);

        if (count >= 3)
        {
            for (int i = matchStartIndex; i < matchStartIndex + count; i++)
            {
                Destroy(boardManager.Spawner.BlockArray[i, x].BlockInstance);
                boardManager.Spawner.BlockArray[i, x].BlockInstance = null;
            }
        }

        return count;
    }

    public bool CubeMatchCheck(int x, int y, BoardManager boardManager)
    {
        // 3x3 매치 체크 로직
        bool isMatched = false;
        return isMatched;
    }
}
