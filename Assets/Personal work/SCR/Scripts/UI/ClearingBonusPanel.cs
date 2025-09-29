using System.Collections.Generic;
using UnityEngine;

public class ClearingBonusPanel : MonoBehaviour
{
    [SerializeField] List<GameObject> onObj;
    [SerializeField] List<GameObject> offObj;

    void OnEnable()
    {
        foreach (var obj in onObj)
        {
            obj.SetActive(false);
        }
        foreach (var obj in offObj)
        {
            obj.SetActive(false);
        }
        int clearing = Manager.Stage.GetClearing();
        for (int i = 0; i < 3; i++)
        {
            if (i < clearing) onObj[i].SetActive(true);
            else offObj[i].SetActive(true);
        }
    }
}
