using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopup : MonoBehaviour
{
    [SerializeField] List<GoodsIcon> goodsIcons;
    [SerializeField] GoodsIHIcon goodsIHIcon;
    [SerializeField] GameObject musicIcon;
    [SerializeField] Button button;

    void Awake()
    {
        button.onClick.AddListener(ClickReward);
    }

    private void OnEnable()
    {
        Manager.Audio.PlaySFX("Shop_Buy");
    }

    private void OnDisable()
    {
        for (int i = 0; i < goodsIcons.Count; i++)
        {
            goodsIcons[i].gameObject.SetActive(false);
        }
        goodsIHIcon.gameObject.SetActive(false);
        musicIcon.SetActive(false);
    }


    public void AddReward(Goods goods, int index)
    {
        int num = (int)goods;
        goodsIcons[num].gameObject.SetActive(true);
        goodsIcons[num].SetIndex(index);
    }

    public void AddIHReward(float index)
    {
        goodsIHIcon.gameObject.SetActive(true);
        goodsIHIcon.SetIndex(index);
    }

    public void AddMusic()
    {
        musicIcon.SetActive(true);
    }

    private void ClickReward()
    {
        for (int i = 0; i < goodsIcons.Count; i++)
        {
            if (goodsIcons[i].gameObject.activeSelf)
            {
                GetReward((Goods)i, goodsIcons[i].Index);
            }
        }
        if (goodsIHIcon.gameObject.activeSelf)
        {
            GetIHReward(goodsIHIcon.Index);
        }
        gameObject.SetActive(false);
    }

    private void GetReward(Goods goods, int index)
    {
        switch (goods)
        {
            case Goods.Gold:
                Manager.User.AddCoin(index);
                break;
            case Goods.Whisk:
                Manager.User.AddItem(ItemType.Whisk, index);
                break;
            case Goods.Scissors:
                Manager.User.AddItem(ItemType.Scissors, index);
                break;
            case Goods.DonutPan:
                Manager.User.AddItem(ItemType.DonutPan, index);
                break;
            case Goods.Coffee:
                Manager.User.AddItem(ItemType.Coffee, index);
                break;
            case Goods.Roller:
                Manager.User.AddItem(ItemType.Roller, index);
                break;
            case Goods.DonutBox:
                Manager.User.AddItem(ItemType.DonutBox, index);
                break;
            case Goods.Oven:
                Manager.User.AddItem(ItemType.Oven, index);
                break;
            case Goods.Heart:
                Manager.User.AddHeart(index);
                break;
        }
    }

    private void GetIHReward(float index)
    {
        Manager.User.InfinityHeart(index);
    }
}

public enum Goods
{
    Gold,
    Whisk,
    Scissors,
    DonutPan,
    Coffee,
    Roller,
    DonutBox,
    Oven,
    Heart,
    InfinityHeart
}