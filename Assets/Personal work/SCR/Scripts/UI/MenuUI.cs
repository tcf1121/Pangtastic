using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private List<Button> menuBtns;
    [SerializeField] private List<GameObject> menuPanels;

    void Awake()
    {
        for (int i = 0; i < menuBtns.Count; i++)
        {
            int buttonIndex = i; // 클로저 문제 방지를 위해 인덱스 복사
            menuBtns[i].onClick.AddListener(() => ClickMenu(buttonIndex));
        }
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
}
