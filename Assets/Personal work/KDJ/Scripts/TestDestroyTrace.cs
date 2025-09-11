using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDestroyTrace : MonoBehaviour
{
    void OnDestroy()
    {
        Debug.LogError("TestDestroyTrace 오브젝트 파괴됨", gameObject);
    }
}
