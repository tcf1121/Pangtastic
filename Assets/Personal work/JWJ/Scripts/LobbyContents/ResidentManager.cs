using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResidentManager : MonoBehaviour
{
    private Dictionary<DestinationType, Transform> _destByType = new Dictionary<DestinationType, Transform>();
    private List<ResidentController> _residents = new List<ResidentController>();

    [SerializeField] private Sprite _greet;
    [SerializeField] private Sprite _workout;
    [SerializeField] private Sprite _lookAround;
    [SerializeField] private Sprite _talking;

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

    public void RegisterResident(ResidentController controller)
    {
        if (controller == null)
        {
            return;
        }
        if (_residents.Contains(controller) == false)
        {
            _residents.Add(controller);
        }
    }

    public Transform GetRandomDestination()
    {
        if (_destByType.Count == 0)
        {
            return null;
        }

        Transform[] transforms = new Transform[_destByType.Count];
        _destByType.Values.CopyTo(transforms, 0);

        int rand = UnityEngine.Random.Range(0, transforms.Length);
        return transforms[rand];
    }

    public Transform GetDestinationByType(DestinationType type)
    {
        if (_destByType.ContainsKey(type) == true)
        {
            return _destByType[type];
        }
        return null;
    }

    public Transform PickNextDestination(ResidentSO resident)
    {
        if (resident == null)
        {
            return GetRandomDestination();
        }

        if (_config != null) //추가됨!!! 설정 로드 완료 시
        {
            bool pickPreferred = UnityEngine.Random.value <= _config.PreferredWeight;

            if (pickPreferred == true)
            {
                Transform fav = GetDestinationByType(resident.FavoriteDestination);
                if (fav != null)
                {
                    //Debug.Log($"{resident.ResidentName} 선호장소 {resident.FavoriteDestination}로 이동");
                    return fav;
                }
            }
        }
        
        return GetRandomDestination();
    }

    public Sprite GetSpriteByState(ResidentState state)
    {
        switch (state)
        {
            case ResidentState.Idle:
                return _greet;

            case ResidentState.Move:
                return _greet;

            case ResidentState.Interact:
                return _lookAround;

            case ResidentState.Greet:
                return _greet;

            case ResidentState.Talk:
                return _talking;

            case ResidentState.Workout:
                return _workout;

            case ResidentState.Touched:
                return _greet;
        }
        return _greet;
    }
}