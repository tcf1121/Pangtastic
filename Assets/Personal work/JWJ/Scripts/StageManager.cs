using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

[DefaultExecutionOrder(-20)]
public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [SerializeField] private List<StageSO> stages = new List<StageSO>(); //잘 들어가는지 인스팩터에서 확인하려고 [SerializeField]로 만들어놓음

    [SerializeField] private StageStartButton startbtn;
    [SerializeField] private Button btn;
    [SerializeField] private TMP_InputField tMP_InputField;


    public int CurrentStageIndex { get; private set; } = 0;
    public StageSO CurrentStage => stages[CurrentStageIndex];

    private void Awake()
    {
        Debug.Log("스테이지 매니저 실행");
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

        LoadAllStages();
    }

    private void LoadAllStages()
    {
        stages.Clear();

        AsyncOperationHandle<IList<StageSO>> handle = Addressables.LoadAssetsAsync<StageSO>("StageSO", null);
        handle.Completed += OnStagesLoaded;
    }

    private void OnStagesLoaded(AsyncOperationHandle<IList<StageSO>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            IList<StageSO> loadedList = handle.Result;
            for (int i = 0; i < loadedList.Count; i++)
            {
                stages.Add(loadedList[i]);
            }

            stages.Sort((a, b) => a.StageID.CompareTo(b.StageID)); // 스테이지 정렬

            Debug.Log($"StageSO 로드 완료. 총 {stages.Count}개");
        }
        else
        {
            Debug.LogError($"StageSO 로드 실패:{handle.OperationException}");
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
}
