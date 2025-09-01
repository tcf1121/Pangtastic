using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    public static CoroutineManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public static Coroutine StartGlobalCoroutine(IEnumerator routine)
    {
        if (Instance != null)
        {
            return Instance.StartCoroutine(routine);
        }
        return null;
    }
}
