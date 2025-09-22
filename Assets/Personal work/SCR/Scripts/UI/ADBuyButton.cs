using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ADBuyButton : MonoBehaviour
{
    [SerializeField] TMP_Text _text;
    [SerializeField] GameObject _todayfin;
    [SerializeField] Slider _slider;
    [SerializeField] GameObject _adIcon;
    [SerializeField] Button _buyButton;

    private int count;

    void Awake()
    {
        count = Manager.Date.GetPurchaseIndex();
        SetADButton();
        _buyButton.onClick.AddListener(BuyFree);
    }

    private void SetADButton()
    {
        _slider.value = count;
        _text.text = $"{count}/5";
        if (count == 0) _adIcon.SetActive(false);
        else _adIcon.SetActive(true);

        if (count == 5)
        {
            _todayfin.SetActive(true);
            _buyButton.interactable = false;
        }
    }

    private void BuyFree()
    {
        if (count == 0)
        {
            ItemType item = Manager.Date.LoadNumbers(count);
            OutGameManager.AddReward((Goods)Enum.Parse(typeof(Goods), item.ToString()), 1);
            GetADReward();
        }
        else if (count < 4)
        {
            Manager.Ad.OnRewardAdClosed += GetADReward;
            ItemType item = Manager.Date.LoadNumbers(count);
            OutGameManager.AddReward((Goods)Enum.Parse(typeof(Goods), item.ToString()), 1);
            Manager.Ad.ShowAD();
        }
        else if (count == 4)
        {
            Manager.Ad.OnRewardAdClosed += GetADReward;
            OutGameManager.AddIHReward(0.5f);
            Manager.Ad.ShowAD();
        }
        else
        {
            Debug.Log("오늘 횟수 끝");
        }
    }

    private void GetADReward()
    {
        OutGameManager.ShowRewardPopup();
        Manager.Date.Purchase();
        count = Manager.Date.GetPurchaseIndex();
        SetADButton();
    }
}
