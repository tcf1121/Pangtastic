using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestPurchase : MonoBehaviour
{
    [SerializeField] private Button consumableBtton;
    [SerializeField] private Button nonConsumableBotton;

    [SerializeField] private string _consumableID = "testitem.unlimit";
    [SerializeField] private string _nonConsumableID = "testitem.onetime";

    private void Awake()
    {
        consumableBtton.onClick.AddListener(OnClickBuyConsumable);
        nonConsumableBotton.onClick.AddListener(OnClickBuyNonConsumable);

        Manager.IAP.OnProductsReady += OnProductsReady;
        Manager.IAP.OnNonConsumableOwned += OnNonConsumableOwned;
    }

    private void OnDestroy()
    {
        consumableBtton.onClick.RemoveListener(OnClickBuyConsumable);
        nonConsumableBotton.onClick.RemoveListener(OnClickBuyNonConsumable);

        Manager.IAP.OnProductsReady -= OnProductsReady;
        Manager.IAP.OnNonConsumableOwned -= OnNonConsumableOwned;
    }

    private void OnProductsReady(bool isProductsReady)
    {
        consumableBtton.interactable = isProductsReady;
        nonConsumableBotton.interactable = isProductsReady;
        Debug.Log($"상품 버튼 활성화: {isProductsReady}");

        if (isProductsReady == true)
        {
            if (Manager.IAP.CheckNonConsumableOwned(_nonConsumableID))
            {
                nonConsumableBotton.interactable = false;
                Debug.Log($"{_nonConsumableID} 이미 구매함. 버튼 비활성화");
            }
        }
    }

    private void OnNonConsumableOwned(string productId)
    {
        nonConsumableBotton.interactable = false;
        Debug.Log($"{productId} 논컨슈머블 버튼 비활성화");
    }

    private void OnClickBuyConsumable()
    {
        Debug.Log("컨슈머블 버튼 눌림");
        Manager.IAP.TryPurchase(_consumableID);
    }

    private void OnClickBuyNonConsumable()
    {

        Debug.Log("논컨슈머블 버튼 눌림");
        if(Manager.IAP.CheckNonConsumableOwned(_nonConsumableID))
        {
            Debug.LogWarning("이미 구매한 아이템입니다.");
            return;
        }

        Debug.LogWarning("구매 시도.");
        Manager.IAP.TryPurchase(_nonConsumableID);
    }

}
