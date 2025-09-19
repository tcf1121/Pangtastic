using UnityEngine;
using UnityEngine.UI;

public class TestBuyBtn : MonoBehaviour
{
    [SerializeField] private TestBuyType _itemType;
    [SerializeField] int _coin;
    private Button _button;
    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(BuyItem);
    }

    private void BuyItem()
    {
        if (_itemType == TestBuyType.Coin)
        {
            OutGameManager.AddReward(Goods.Gold, 1000);
        }
        else if (_itemType == TestBuyType.Heart)
        {
            OutGameManager.AddIHReward(0.5f);
        }
        else if (_itemType == TestBuyType.ItemPack)
        {
            OutGameManager.AddReward(Goods.Roller, 1);
            OutGameManager.AddReward(Goods.DonutBox, 1);
            OutGameManager.AddReward(Goods.Oven, 1);
            OutGameManager.AddReward(Goods.Whisk, 1);
            OutGameManager.AddReward(Goods.Scissors, 1);
            OutGameManager.AddReward(Goods.DonutPan, 1);
            OutGameManager.AddReward(Goods.Coffee, 1);
        }
        else if (_itemType == TestBuyType.Star)
        {
            Manager.User.AddStar(1);
        }

        OutGameManager.ShowRewardPopup();
    }
    private enum TestBuyType
    {
        Coin,
        Heart,
        ItemPack,
        Star
    }
}
