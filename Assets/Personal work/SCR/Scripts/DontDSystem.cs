using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class DontDSystem : MonoBehaviour
{
    public static DontDSystem Instance { get; private set; }
    [SerializeField] private GameObject _gameSystem;
    public GPGSManager GBGS;
    public DatabaseSystem DB;
    //public HeartSystem heartSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //public static void StatGame()  //JWJ 주석처리함
    //{
    //    Instance._gameSystem.SetActive(true);
    //    //Instance.heartSystem = new();
    //}

}
