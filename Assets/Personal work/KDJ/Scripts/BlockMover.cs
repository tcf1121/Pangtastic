using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlockMover : MonoBehaviour
{
    public Vector2 StartPos { get; set; }
    public Vector2 EndPos { get; set; }

    public Vector2Int StartBlockPos { get; private set; }
    public Vector2Int EndBlockPos { get; private set; }

    /// <summary>
    /// StartPos와 EndPos가 설정되어 있을 경우 블록을 이동
    /// </summary>
    public void MoveBlock(BoardManager boardManager)
    {
        // 블록 이동 로직
        if (StartPos == Vector2.zero || EndPos == Vector2.zero)
        {
            Debug.Log("StartPos 또는 EndPos가 설정되지 않았습니다.");
            return;
        }

        StartPos += new Vector2(boardManager.Spawner.BlockPlate.BlockPlateWidth / 2, boardManager.Spawner.BlockPlate.BlockPlateHeight / 2);
        EndPos += new Vector2(boardManager.Spawner.BlockPlate.BlockPlateWidth / 2, boardManager.Spawner.BlockPlate.BlockPlateHeight / 2);

        if (boardManager.Spawner.BlockPlate.BlockPlateHeight < StartPos.y || boardManager.Spawner.BlockPlate.BlockPlateWidth < StartPos.x || boardManager.Spawner.BlockPlate.BlockPlateHeight < EndPos.y || boardManager.Spawner.BlockPlate.BlockPlateWidth < EndPos.x)
        {
            Debug.Log("블록이 보드 영역을 벗어났습니다.");
            return;
        }

        // StartPos와 EndPos를 이용하여 블록을 이동하는 로직 구현
        Vector2Int startGrid = WorldToGrid(StartPos);
        StartBlockPos = startGrid; // 시작 위치 저장

        // 방향 구하기
        // Vector2 direction = EndPos - StartPos;
        // direction.Normalize(); // 방향 벡터를 정규화

        Vector2Int endGrid = WorldToGrid(EndPos);
        EndBlockPos = endGrid; // 끝 위치 저장

        if (boardManager.Spawner.BlockArray[startGrid.y, startGrid.x].GemType == GemType.Cloche || boardManager.Spawner.BlockArray[endGrid.y, endGrid.x].GemType == GemType.Cloche) return;

        // 이후 대각으로 가는 경우에 대한 예외처리 추가 필요
        Block tempBlock = boardManager.Spawner.BlockArray[endGrid.y, endGrid.x];
        boardManager.Spawner.BlockArray[endGrid.y, endGrid.x] = boardManager.Spawner.BlockArray[startGrid.y, startGrid.x]; // 시작 위치의 블록 제거
        boardManager.Spawner.BlockArray[startGrid.y, startGrid.x] = tempBlock;

        // 오브젝트 이동
        Vector2 moveStartPos = endGrid - startGrid;
        boardManager.Spawner.BlockArray[endGrid.y, endGrid.x].BlockInstance.transform.position += new Vector3(moveStartPos.x, moveStartPos.y, 0);
        Vector2 moveEndPos = startGrid - endGrid;
        boardManager.Spawner.BlockArray[startGrid.y, startGrid.x].BlockInstance.transform.position += new Vector3(moveEndPos.x, moveEndPos.y, 0);

    }

    public void ReturnBlock(BoardManager boardManager)
    {
        // 블록을 원래 위치로 되돌리는 로직
        // 블럭들의 시작 위치와 끝 위치를 저장해뒀기에 그걸 사용
        if (boardManager.Spawner.BlockArray[StartBlockPos.y, StartBlockPos.x].GemType == GemType.Cloche || boardManager.Spawner.BlockArray[EndBlockPos.y, EndBlockPos.x].GemType == GemType.Cloche) return;

        if (boardManager.Spawner.BlockPlate.BlockPlateHeight < StartPos.y || boardManager.Spawner.BlockPlate.BlockPlateWidth < StartPos.x || boardManager.Spawner.BlockPlate.BlockPlateHeight < EndPos.y || boardManager.Spawner.BlockPlate.BlockPlateWidth < EndPos.x)
        {
            Debug.Log("블록이 보드 영역을 벗어났습니다.");
            return;
        }

        Block tempBlock = boardManager.Spawner.BlockArray[EndBlockPos.y, EndBlockPos.x];
        boardManager.Spawner.BlockArray[EndBlockPos.y, EndBlockPos.x] = boardManager.Spawner.BlockArray[StartBlockPos.y, StartBlockPos.x];
        boardManager.Spawner.BlockArray[StartBlockPos.y, StartBlockPos.x] = tempBlock;

        // 오브젝트 위치도 원래대로 되돌리기
        Vector2 moveStartPos = EndBlockPos - StartBlockPos;
        boardManager.Spawner.BlockArray[EndBlockPos.y, EndBlockPos.x].BlockInstance.transform.position += new Vector3(moveStartPos.x, moveStartPos.y, 0);
        Vector2 moveEndPos = StartBlockPos - EndBlockPos;
        boardManager.Spawner.BlockArray[StartBlockPos.y, StartBlockPos.x].BlockInstance.transform.position += new Vector3(moveEndPos.x, moveEndPos.y, 0);
    }

    /// <summary>
    /// 월드 좌표를 그리드로 전환. 현재 블럭 보드의 한칸의 길이가 1인 정사각형이기에 그냥 소숫점을 버리고 반환함. 이후에 변경될 수도 있음.
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        // 월드 좌표를 그리드 좌표로 변환
        int x = (int)worldPosition.x;
        int y = (int)worldPosition.y;
        return new Vector2Int(x, y);
    }
}
