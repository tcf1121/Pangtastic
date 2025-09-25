using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ADBuyButton : MonoBehaviour
{
    [SerializeField] TMP_Text _text;
    [SerializeField] GameObject _usualText;
    [SerializeField] GameObject _finishText;
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
            _usualText.SetActive(false);
            _finishText.SetActive(true);
            _buyButton.interactable = false;
        }
        else
        {
            _usualText.SetActive(true);
            _finishText.SetActive(false);
        }
    }

    private void BuyFree()
    {
        if (count == 0)
        {
            Goods goods = Manager.Date.LoadNumbers(count);
            int index = 1;
            if (goods == Goods.Gold) index = 1000;
            OutGameManager.AddReward(goods, index);
            GetADReward();
        }
        else if (count < 4)
        {
            Manager.Ad.OnRewardAdClosed += GetADReward;
            Goods goods = Manager.Date.LoadNumbers(count);
            int index = 1;
            if (goods == Goods.Gold) index = 1000;
            OutGameManager.AddReward(goods, index);
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
