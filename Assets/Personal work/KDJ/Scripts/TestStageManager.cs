using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStageManager : MonoBehaviour
{
    [SerializeField] List<PuzzleBoardSO> testBoards = new List<PuzzleBoardSO>();
    private int currentIndex = 0;
    public PuzzleBoardSO CurrentBoard => testBoards[currentIndex];

    public void SetStage(int index)
    {
        if (index >= 0 && index < testBoards.Count)
        {
            currentIndex = index;
            Debug.Log($"테스트 스테이지 설정: {index}");
        }
        else
        {
            Debug.LogError("잘못된 스테이지 인덱스");
        }
    }
}
