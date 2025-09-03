using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public static class Manager
{
    public static StageManager Stage => StageManager.GetInstance();


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        StageManager.CreateManager();
    }
}
