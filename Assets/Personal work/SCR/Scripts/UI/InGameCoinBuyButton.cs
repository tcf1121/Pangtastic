using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameCoinBuyButton : MonoBehaviour
{
    [SerializeField] private ItemType _itemType;
    [SerializeField] int _coin;
    [SerializeField] TMP_Text _currentCoin;
    private Button _button;
    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(BuyItem);
    }

    private void BuyItem()
    {
        if (Manager.User.CanUseCoin(_coin))
        {
            Manager.User.UseCoin(_coin);
            Manager.User.AddItem(_itemType);
            _currentCoin.text = UserInfoUI.Instance.SetNum(Manager.User.GetCoin());
        }
    }
}
