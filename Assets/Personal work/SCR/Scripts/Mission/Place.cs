using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using System;

public class Place : MonoBehaviour
{
    [SerializeField] MissonPlace _place;
    [SerializeField] GameObject openObject;
    [SerializeField] List<GameObject> missionObjects;

    public void MissionClear(int index, Action openPopup = null)
    {
        GameObject furniture;
        if (openObject != null)
        {
            if (index == 1) furniture = openObject;
            else furniture = missionObjects[index - 2];
        }
        else
        {
            furniture = missionObjects[index - 1];
        }
        if (furniture != null)
        {
            furniture.transform.localScale = Vector3.zero; // 시작 스케일
            if (openObject != null && index == 1) furniture.SetActive(false);
            else furniture.SetActive(true);
            furniture.transform.DOScale(Vector3.one, 0.5f)   // 0.5초 동안 스케일 0 → 1
                .SetEase(Ease.OutBack)                       // 부드러운 튀는 느낌
                .OnComplete(() =>
                {
                    openPopup?.Invoke();
                });
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
