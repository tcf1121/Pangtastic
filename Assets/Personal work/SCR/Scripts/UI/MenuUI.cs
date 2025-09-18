using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private List<Button> menuBtns;
    [SerializeField] private List<GameObject> menuPanels;
    [SerializeField] private GameObject shopMenu;
    [SerializeField] private MapMover mapMover;
    [SerializeField] private List<Transform> _roomCameraPosList;

    void Awake()
    {
        for (int i = 0; i < menuBtns.Count; i++)
        {
            int buttonIndex = i; // 클로저 문제 방지를 위해 인덱스 복사
            menuBtns[i].onClick.AddListener(() => ClickMenu(buttonIndex));

        }
        menuBtns[1].onClick.AddListener(GoHome);
        menuBtns[2].onClick.AddListener(GoRoom);
    }

    private void ClickMenu(int index)
    {
        for (int i = 0; i < menuBtns.Count; i++)
        {
            if (i == 0) shopMenu.SetActive(true);
            else shopMenu.SetActive(false);

            if (i == index)
            {
                menuPanels[i].SetActive(true);
                menuBtns[i].gameObject.transform.localScale = new Vector3(1.5f, 1, 1);
                menuBtns[i].gameObject.transform.GetChild(0).gameObject.SetActive(true);
                menuBtns[i].gameObject.transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                menuPanels[i].SetActive(false);
                menuBtns[i].gameObject.transform.localScale = new Vector3(1, 1, 1);
                menuBtns[i].gameObject.transform.GetChild(0).gameObject.SetActive(false);
                menuBtns[i].gameObject.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    public void GoHome(Action onComplete = null)
    {
        MoveCameraDOTween(new Vector3(0, 20, -6.5f), Quaternion.Euler(45, 0, 0), 10, 1f, onComplete);
    }

    private void GoHome()
    {
        mapMover.isMove = true;
        MoveCameraDOTween(new Vector3(0, 20, -6.5f), Quaternion.Euler(45, 0, 0), 10, 1f, () => mapMover.isMove = false);
    }

    public void GoRoom(Action onComplete = null)
    {
        int index = (int)Manager.User.GetCurPlace();
        MoveCameraDOTween(_roomCameraPosList[index].position, _roomCameraPosList[index].rotation, 6, 1f, onComplete);
    }

    private void GoRoom()
    {
        int index = (int)Manager.User.GetCurPlace();
        MoveCameraDOTween(_roomCameraPosList[index].position, _roomCameraPosList[index].rotation, 6, 1f);
    }


    private void MoveCameraDOTween(Vector3 pos, Quaternion rot, float size, float duration, Action onComplete = null)
    {
        Camera cam = Camera.main;
        cam.transform.DOMove(pos, duration).SetEase(Ease.InOutSine);
        cam.transform.DORotateQuaternion(rot, duration).SetEase(Ease.InOutSine);
        cam.DOOrthoSize(size, duration).SetEase(Ease.InOutSine).OnComplete(() => onComplete?.Invoke());
    }
}
