using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-20)]
public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [SerializeField] private List<StageSO> stages;
    [SerializeField] private StageStartButton startbtn;
    [SerializeField] private Button btn;
    [SerializeField] private TMP_InputField tMP_InputField;
    public int CurrentStageIndex { get; private set; } = 0;
    public StageSO CurrentStage => stages[CurrentStageIndex];

    private void Awake()
    {
        Debug.Log("스테이지");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        btn.onClick.AddListener(SetStage);
    }

    public void AdvanceStage()
    {
        if (CurrentStageIndex < stages.Count - 1)
        {
            CurrentStageIndex++;
        }
    }

    public void SetStage()
    {
        CurrentStageIndex = int.Parse(tMP_InputField.text.ToString()) - 1;
        startbtn.SetStage();
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
