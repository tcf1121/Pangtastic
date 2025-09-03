using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class DontDSystem : MonoBehaviour
{
    public static DontDSystem Instance { get; private set; }
    [SerializeField] private GameObject _gameSystem;
    // Start is called before the first frame update
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

    public static void StatGame()
    {
        Instance._gameSystem.SetActive(true);
    }

}
