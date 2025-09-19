using System;
using UnityEngine;

public class ADBuyButton : MonoBehaviour
{
    private int count;

    void Awake()
    {
        count = Manager.User.GetPurchaseIndex();
        Manager.Ad.OnRewardAdClosed += OutGameManager.ShowRewardPopup;
    }

    private void BuyFree()
    {
        if (count == 0)
        {
            ItemType item = Manager.User.LoadNumbers(count);
            OutGameManager.AddReward((Goods)Enum.Parse(typeof(Goods), item.ToString()), 1);
            OutGameManager.ShowRewardPopup();
        }
        else if (count < 4)
        {
            ItemType item = Manager.User.LoadNumbers(count);
            OutGameManager.AddReward((Goods)Enum.Parse(typeof(Goods), item.ToString()), 1);
            Manager.Ad.ShowAD();
        }
        else
        {

        }
    }
}
