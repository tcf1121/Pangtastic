using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private List<Button> menuBtns;
    [SerializeField] private List<GameObject> menuPanels;
    [SerializeField] private List<Transform> _roomCameraPosList;
    [SerializeField] int curRoom;

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

    private void GoHome()
    {
        Camera.main.transform.position = new Vector3(0, 9, 5);
        Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);
        Camera.main.orthographicSize = 10;
    }

    private void GoRoom()
    {
        Camera.main.transform.position = _roomCameraPosList[curRoom].position;
        Camera.main.transform.rotation = _roomCameraPosList[curRoom].rotation;
        Camera.main.orthographicSize = 6;
    }
}
