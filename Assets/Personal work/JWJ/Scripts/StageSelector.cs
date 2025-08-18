using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSelector : MonoBehaviour
{
    [SerializeField] private StageSetting _stageSetting;
    [SerializeField] private StageSO[] _stages;
    private int _currentIndex; 

    public void SetStageByIndex(int index) // UI에서 인덱스로 선택할 때 호출 
    {
        _currentIndex = index; // 현재 인덱스 갱신
        _stageSetting.SetStage(_stages[_currentIndex]); // 여기서 StageRecipeSet에 스테이지 주입
        Debug.Log("[스테이지 선택] " + _stages[_currentIndex].StageID.ToString());
    }
}
