using SCR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleBoardSO", menuName = "PangTastic/PuzzleBoard")]
public class PuzzleBoardSO : ScriptableObject
{
    [Header("보드ID")]
    public int BoardId;

    [Header("셀 데이터")]
    public List<CellData> Cells = new List<CellData>();

    [Header("스폰 포인트")]
    public List<Vector3Int> SpawnPoints = new List<Vector3Int>();

    [Header("예비 배열")]
    public List<Vector3Int> ReservePositions = new List<Vector3Int>();

    public CellData[] BuildMapArray()
    {
        CellData[] mapArray = new CellData[Cells.Count];

        for (int i = 0; i < Cells.Count; i++)
        {
            mapArray[i] = Cells[i]; 
        }

        return mapArray;
    }
}

[Serializable]
public class CellData
{
    public Vector3Int Position; // 칸 좌표
    public GemType gemType;     // 해당 칸 잼 타입
}