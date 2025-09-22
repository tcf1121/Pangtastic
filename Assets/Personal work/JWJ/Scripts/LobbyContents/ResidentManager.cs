using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResidentManager : MonoBehaviour
{
    private Dictionary<DestinationType, Transform> _destByType = new Dictionary<DestinationType, Transform>();

    //[SerializeField] private Sprite _greet;
    [SerializeField] private Sprite _workout;
    //[SerializeField] private Sprite _lookAround;
    [SerializeField] private Sprite _talking;
    [SerializeField] private Sprite _favStore;

    [SerializeField] private ResidentConfigSO _config;
    public ResidentConfigSO Config { get { return _config; } }

    private void Awake()
    {
        BuildDestinationMap();
        LoadConfig();
    }

    private void LoadConfig()
    {
        AsyncOperationHandle<ResidentConfigSO> handle = Addressables.LoadAssetAsync<ResidentConfigSO>("ResidentConfigSO");
        handle.Completed += OnConfigLoaded;
    }
    private void OnConfigLoaded(AsyncOperationHandle<ResidentConfigSO> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) //추가됨!!! 성공 여부 체크
        {
            _config = handle.Result;
            Debug.Log("ResidentConfigSO 로드 완료");
        }
        else
        {
            Debug.LogError($"ResidentConfigSO 로드 실패: {handle.OperationException}");
        }
    }

    private void BuildDestinationMap()
    {
        _destByType.Clear();

        DestinationPoint[] pointsInScene;

        pointsInScene = GameObject.FindObjectsOfType<DestinationPoint>(true);
        Debug.Log($"목적지 {pointsInScene.Length}개");
        foreach (DestinationPoint dp in pointsInScene)
        {
            if (dp != null)
            {
                DestinationType type = dp.Type;
                if (_destByType.ContainsKey(type) == false)
                {
                    _destByType.Add(type, dp.transform);
                }
                else
                {
                    Debug.LogWarning($"중복된 목적지: {type}, {dp.name}");
                }
            }
        }
    }

    public void CheckResident(ResidentController resident)
    {
        Debug.Log("주민 체크 시작");

        if (resident == null)
        {
            Debug.LogError("주민없음");
            return;
        }

        int curStageLevel = Manager.Stage.CurrentStageIndex + 1;

        if (resident.Resident.UnlockStage > curStageLevel)
        {
            Debug.Log($"{resident.Resident.ResidentName}의 해금 레벨 {resident.Resident.UnlockStage}, 현재 스테이지 레벨 {curStageLevel}. 주민 비활성화");
            
            resident.gameObject.SetActive(false);
        }
    }

    public Transform PickNextDestination(ResidentSO resident, DestinationType lastDestination)
    {
        List<Transform> candidates = new List<Transform>();

        if (resident == null)
        {
            Debug.Log("주민이 없음");
            return null;
        }
        if (_config == null)
        {
            Debug.Log("config 없음");
            return null;
        }

        bool pickPreferred = UnityEngine.Random.value <= _config.PreferredWeight;

        if (pickPreferred)
        {
            foreach (DestinationType type in resident.FavoriteDestinations)
            {
                if (_destByType.ContainsKey(type) && type != lastDestination)
                {
                    candidates.Add(_destByType[type]);
                }
            }
        }

        if (candidates.Count == 0 || !pickPreferred)
        {
            foreach (KeyValuePair<DestinationType, Transform> kvp in _destByType)
            {
                if (kvp.Key != lastDestination)
                {
                    if (kvp.Value != null)
                    {
                        candidates.Add(kvp.Value);
                    }
                }
            }
        }

        int rand = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[rand];
    }

    public Sprite GetSpriteByState(ResidentState state)
    {
        switch (state)
        {
            case ResidentState.Idle:
                return null;

            case ResidentState.Move:
                return null;

            case ResidentState.Interact:
                return _favStore;

            case ResidentState.Talk:
                return _talking;

            case ResidentState.Workout:
                return _workout;

            case ResidentState.Touched:
                return null;
        }
        return null;
    }
}