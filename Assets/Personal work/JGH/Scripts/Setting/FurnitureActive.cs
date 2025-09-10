using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FurnitureActive : MonoBehaviour
{
    [Header("부모 오브젝트 이름 (예: Donuts_16 Variant)")]
    [SerializeField] private string parentName;

    [Header("활성화할 자식 이름들")]
    [SerializeField] private string[] targetNames;

    [ContextMenu("Activate Selected")]
    public void ActivateSelected()
    {
        if (string.IsNullOrEmpty(parentName))
        {
            Debug.LogWarning("Parent 이름이 비어있습니다.");
            return;
        }

        GameObject parentObject = GameObject.Find(parentName);
        if (parentObject == null)
        {
            Debug.LogWarning($"{parentName} 오브젝트를 찾을 수 없습니다.");
            return;
        }

        foreach (string name in targetNames)
        {
            Transform child = parentObject.transform.Find(name);
            if (child != null)
            {
                child.gameObject.SetActive(true);
                Debug.Log($"{child.name} 활성화 완료!");

                FurnitureSaveActiveDB(parentName, name);
            }
            else
            {
                Debug.LogWarning($"{name} 을(를) {parentName} 하위에서 찾을 수 없습니다.");
            }
        }
    }

    /// <summary>
    /// 코드에서 직접 이름을 지정해 실행하는 함수
    /// </summary>
    public void ActivateByNames(string parent, params string[] names)
    {
        parentName = parent;
        targetNames = names;
        ActivateSelected();
    }

    // private void Start()
    // {
        // 코드에서 실행:
        // FindObjectOfType<FurnitureActive>()
            // .ActivateByNames("Donuts_16 Variant", "Donut_1", "Donut_2");
    // }

    private void FurnitureLoadActiveDB(string name)
    {
        string authJson = Manager.DB.GetAuthInfo();
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);

        Manager.DB.dbRef
            .Child(info.type)
            .Child(info.uid)
            .Child("Furniture")
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    var s = task.Result;
                    foreach (var child in s.Children)
                    {
                        Debug.Log($"가구 ID: {child.Key}, 데이터: {child.GetRawJsonValue()}");
                    }
                }

            });
        // .Child("Donuts")
    }

    private void FurnitureSaveActiveDB(string parent, string name)
    {
        string authJson = Manager.DB.GetAuthInfo();
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);

        Manager.DB.dbRef
            .Child(info.type)
            .Child(info.uid)
            .Child("Furniture")
            .Child($"{parent}")
            .Child($"{name}")
            .SetValueAsync(1);
    }
}
