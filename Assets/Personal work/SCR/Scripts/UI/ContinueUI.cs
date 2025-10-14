using System.Collections.Generic;
using UnityEngine;

public class ContinueUI : MonoBehaviour
{
    [SerializeField] GameObject _clearingText;
    [SerializeField] List<GameObject> _buttons;

    void OnEnable()
    {
        if (Manager.User.GetClearing() > 0)
            _clearingText.SetActive(true);
        else
            _clearingText.SetActive(false);

        if (Manager.Ad.RemovedAD)
        {
            _buttons[0].SetActive(false);
            _buttons[1].SetActive(false);
            _buttons[2].SetActive(true);
        }
        else
        {
            _buttons[0].SetActive(true);
            _buttons[1].SetActive(true);
            _buttons[2].SetActive(false);
        }
    }
}
