using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPanel : MonoBehaviour
{
    [SerializeField] List<GameObject> _panels;
    [SerializeField] List<GameObject> _offObj;
    [SerializeField] List<Button> _buttons;

    void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            int index = i;
            _buttons[i].onClick.AddListener(() => ClickToggle(index));
        }
    }

    private void ClickToggle(int index)
    {
        for (int i = 0; i < 2; i++)
        {
            if (i == index)
            {
                _panels[i].SetActive(true);
                _offObj[i].SetActive(false);
            }
            else
            {
                _panels[i].SetActive(false);
                _offObj[i].SetActive(true);
            }
        }
    }
}
