using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinBuyBtn : MonoBehaviour
{
    [SerializeField] private ItemType _itemType;
    [SerializeField] private bool _isSet;
    [SerializeField] private bool _isBakery;
    [SerializeField] int _coin;
    private Button _button;
    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(BuyItem);
    }

    private void BuyItem()
    {
        if (Manager.UserInfoSystem.CanUseCoin(_coin))
        // if (Manager.User.CanUseCoin(_coin))
        {
            // Manager.User.UseCoin(_coin);
            Manager.UserInfoSystem.UseCoin(_coin);
            if (_isBakery)
            {
                OutGameManager.AddReward(Goods.Whisk, 3);
                OutGameManager.AddReward(Goods.Scissors, 3);
                OutGameManager.AddReward(Goods.DonutPan, 3);
                OutGameManager.AddReward(Goods.Coffee, 3);
                OutGameManager.AddReward(Goods.Roller, 3);
                OutGameManager.AddReward(Goods.DonutBox, 3);
                OutGameManager.AddReward(Goods.Oven, 3);
            }
            else
            {
                OutGameManager.AddReward(
                (Goods)Enum.Parse(typeof(Goods), _itemType.ToString()), _isSet ? 10 : 1);
            }

            OutGameManager.ShowRewardPopup();
        }
    }

}
