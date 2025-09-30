using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TEST_AD_Buy : MonoBehaviour
{
    [SerializeField] private Button _button;
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(BuyAdRemove);

    }

    private void BuyAdRemove()
    {
        Manager.IAP.TestPurchasing("noads");
        OutGameManager.CloseBannerAd();
    }
}
