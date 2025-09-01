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
    private StageStartButton startbtn;
    private Button btn;
    private TMP_InputField tMP_InputField;
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

    public void SetStartBtn(StageStartButton startbtn, Button btn, TMP_InputField tMP_InputField)
    {
        this.startbtn = startbtn;
        this.btn = btn;
        this.tMP_InputField = tMP_InputField;
        btn.onClick.AddListener(SetStage);
    }
}
