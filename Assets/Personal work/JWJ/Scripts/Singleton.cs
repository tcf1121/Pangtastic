using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static T CreateManager()
    {
        if (instance != null)
        {
            return instance;
        }

        T found = Object.FindObjectOfType<T>(true);
        if (found != null)
        {
            instance = found;
            DontDestroyOnLoad(instance.gameObject);
            return instance;
        }

        GameObject go = new GameObject(typeof(T).Name);
        instance = go.AddComponent<T>();
        DontDestroyOnLoad(go);
        return instance;
    }

    public static void ReleaseManager()
    {
        if (instance == null)
        {
            return;
        }
        GameObject go = instance.gameObject;
        instance = null;
        if (go != null)
        {
            Object.Destroy(go);
        }
    }

    public static T GetInstance()
    {
        return instance;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this as T)
        {
            instance = null;
        }
    }
}
