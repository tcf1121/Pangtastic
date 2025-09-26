using LHJ;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameShopPopupUI : MonoBehaviour
{
    [SerializeField] TMP_Text _currentCoin;
    [SerializeField] List<ItemButton> itemButtons;

    void OnEnable()
    {
        _currentCoin.text = UserInfoUI.Instance.SetNum(Manager.User.GetCoin());
        InGameManager.Instate.PauseGame();
    }

    void OnDisable()
    {
        foreach (var ib in itemButtons)
            ib.SetItemState();
    }
}
