using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Place : MonoBehaviour
{
    [SerializeField] MissonPlace _place;
    [SerializeField] GameObject openObject;
    [SerializeField] List<GameObject> missionObjects;

    public void MissionClear(int index)
    {
        if (openObject != null)
        {
            if (index == 1) openObject.SetActive(false);
            else missionObjects[index - 2].SetActive(true);
        }
        else
        {
            missionObjects[index - 1].SetActive(true);
        }

    }

    public void AllClear()
    {
        if (openObject != null) openObject.SetActive(false);
        foreach (var go in missionObjects)
        {
            go.SetActive(true);
        }

    }

    public void CurrentClear()
    {
        List<bool> curclear = Manager.User.GetCurMisson().ToList();
        for (int i = 0; i < curclear.Count; i++)
        {
            if (curclear[i]) MissionClear(i + 1);
        }
    }
}
