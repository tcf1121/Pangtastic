using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[DefaultExecutionOrder(-20)]
public class StageManager : Singleton<StageManager>
{
    //public static StageManager Instance;

    [SerializeField] private List<StageSO> stages = new List<StageSO>(); //잘 들어가는지 인스팩터에서 확인하려고 [SerializeField]로 만들어놓음

    public int CurrentStageIndex { get { return Manager.User.GetStage(); } }
    public StageSO CurrentStage => stages[CurrentStageIndex];
    private List<bool> UseItem = new();
    private int _clearing { get { return Manager.User.GetClearing(); } }
    private List<RecipeSO> _stageRecipes = new();
    // 레시피 변수 추가

    protected override void Awake()
    {
        base.Awake();
        Debug.Log($"스테이지 매니저 준비");
        LoadAllStages();
        UseItem.Add(false);
        UseItem.Add(false);
        UseItem.Add(false);
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
        if (Manager.User.GetStage() < stages.Count - 1)
        {
            Manager.User.ClearStage();
        }
    }

    public void SetStage(int num)
    {
        Manager.User.SetStage(num);
    }

    public void ResetStage()
    {
        Manager.User.SetStage(0);
    }

    public void SetUseItem(int index)
    {
        UseItem[index] = true;
    }

    public void ResetUseItem()
    {
        for (int i = 0; i < 3; i++)
            UseItem[i] = false;
    }

    public List<bool> GetUseItem()
    {
        return UseItem;
    }

    public void SetStageRecipe(List<RecipeSO> recipeList)
    {
        _stageRecipes.Clear();
        _stageRecipes.AddRange(recipeList);
    }

    public List<RecipeSO> GetStageRecipes()
    {
        return _stageRecipes;
    }

    public void StageClear()
    {
        Manager.User.AddClearing();
    }

    public void StageFail()
    {
        Manager.User.ResetClearing();
    }

    // public void SaveStage()
    // {
    //     StartCoroutine(SaveStageRoutine());
    //     Debug.Log("스테이지 세이브");
    // }

    // public void LoadStage()
    // {
    //     StartCoroutine(LoadStageRoutine());
    //     Debug.Log("스테이지 로드");
    // }
    // private IEnumerator SaveStageRoutine()
    // {
    //     if (Manager.DB == null)
    //     {
    //         Debug.LogError("DB 매니저가 없음");
    //         yield break;
    //     }

    //     string authJson = Manager.DB.GetAuthInfo();
    //     if (string.IsNullOrEmpty(authJson) == true)
    //     {
    //         Debug.LogError("로그인 정보 없음");
    //         yield break;
    //     }

    //     DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson); //제이슨 to 구조체

    //     if (string.IsNullOrEmpty(info.uid) == true || string.IsNullOrEmpty(info.type) == true)
    //     {
    //         Debug.LogError("AuthInfo가 비었음");
    //         yield break;
    //     }

    //     string curStageValue = (CurrentStageIndex + 1).ToString();
    //     var path = Manager.DB.dbRef
    //         .Child(info.type)
    //         .Child(info.uid)
    //         .Child("curStage")
    //         .Child("stage");

    //     var setTask = path.SetValueAsync(curStageValue);
    //     yield return new WaitUntil(IsSetDone);

    //     bool IsSetDone()
    //     {
    //         return setTask.IsCompleted;
    //     }

    //     if (setTask.Exception != null)
    //     {
    //         Debug.LogError($"스테이지 저장 실패: {setTask.Exception}");
    //         yield break;
    //     }

    //     Debug.Log($"스테이지 저장 완료:{curStageValue}");
    //     yield break;

    // }

    // private IEnumerator LoadStageRoutine()
    // {
    //     if (Manager.DB == null)
    //     {
    //         Debug.LogError("DB 매니저가 없음");
    //         yield break;
    //     }

    //     string authJson = Manager.DB.GetAuthInfo();
    //     if (string.IsNullOrEmpty(authJson) == true)
    //     {
    //         Debug.LogError("로그인 정보가 없음");
    //         yield break;
    //     }

    //     DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson); // 제이슨 > 구조체 변환

    //     if (string.IsNullOrEmpty(info.uid) == true || string.IsNullOrEmpty(info.type) == true)
    //     {
    //         Debug.LogError("AuthInfo가 비었음");
    //         yield break;
    //     }

    //     var path = Manager.DB.dbRef
    //         .Child(info.type)
    //         .Child(info.uid)
    //         .Child("curStage")
    //         .Child("stage");

    //     var getTask = path.GetValueAsync();
    //     yield return new WaitUntil(IsGetDone);

    //     bool IsGetDone()
    //     {
    //         return getTask.IsCompleted;
    //     }

    //     if (getTask.Exception != null)
    //     {
    //         Debug.LogError($"스테이지 로드 실패: {getTask.Exception}");
    //         yield break;
    //     }

    //     var snapshot = getTask.Result;
    //     if (snapshot != null && snapshot.Exists == true)
    //     {
    //         string value = snapshot.Value != null ? snapshot.Value.ToString() : "0"; //값있으면 넣고 없으면 0
    //         int loadedIndex = 0;
    //         bool parsed = int.TryParse(value, out loadedIndex); //정수 변환

    //       if (parsed == true)
    //       {
    //           CurrentStageIndex = loadedIndex - 1;
    //           Debug.Log($"스테이지 로드 완료: {CurrentStageIndex + 1}");
    //           OutGameManager.UpdateStageStartButton(); // 버튼 업데이트
    //       }
    //       else
    //       {
    //           Debug.LogWarning($"Stage 값이 정수가 아님: {value}");
    //       }
    //   }
    //   else
    //   {
    //       Debug.Log("Stage 값이 없음");
    //  }

    //     yield break;

    // }
}