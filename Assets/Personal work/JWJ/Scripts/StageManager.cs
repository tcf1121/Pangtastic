using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [SerializeField] private List<StageSO> stages;
    public int CurrentStageIndex { get; private set; } = 0;
    public StageSO CurrentStage => stages[CurrentStageIndex];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }    
        else
        {
            Destroy(gameObject);
        }
    }

    public void AdvanceStage()
    {
        if (CurrentStageIndex < stages.Count - 1)
        {
            CurrentStageIndex++;
        }
    }
    public void ResetStage()
    {
        CurrentStageIndex = 0;
    }

    public void SaveStage()
    {
        Debug.Log("스테이지 세이브");
    }

    public void LoadStage()
    {
        Debug.Log("스테이지 로드");
    }
}
