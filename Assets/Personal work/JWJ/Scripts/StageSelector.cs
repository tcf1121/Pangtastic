using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSelector : MonoBehaviour
{
    [SerializeField] private StageSetting _stageSetting;
    [SerializeField] private StageSO[] _stages;
    private int _currentIndex; 

    public void SetStageByIndex(int index) // UI���� �ε����� ������ �� ȣ�� 
    {
        _currentIndex = index; // ���� �ε��� ����
        _stageSetting.SetStage(_stages[_currentIndex]); // ���⼭ StageRecipeSet�� �������� ����
        Debug.Log("[�������� ����] " + _stages[_currentIndex].StageID.ToString());
    }
}
