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
            Manager.User.AddCoin(1000);
        }
        else if (_itemType == TestBuyType.Heart)
        {
            Manager.User.InfinityHeart(0.5f);
        }
        else if (_itemType == TestBuyType.ItemPack)
        {
            Manager.User.AddItem(ItemType.Roller);
            Manager.User.AddItem(ItemType.DonutBox);
            Manager.User.AddItem(ItemType.Oven);
            Manager.User.AddItem(ItemType.Whisk);
            Manager.User.AddItem(ItemType.Scissors);
            Manager.User.AddItem(ItemType.DonutPan);
            Manager.User.AddItem(ItemType.Coffee);
        }
        else if (_itemType == TestBuyType.Star)
        {
            Manager.User.AddStar(1);
        }
    }
    private enum TestBuyType
    {
        Coin,
        Heart,
        ItemPack,
        Star
    }
}
