using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinBuyBtn : MonoBehaviour
{
    [SerializeField] private ItemType _itemType;
    [SerializeField] int _coin;
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
            OutGameManager.AddReward((Goods)Enum.Parse(typeof(Goods), _itemType.ToString()), 1);
            OutGameManager.ShowRewardPopup();
        }
    }

}
