using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseButton : MonoBehaviour
{
    [SerializeField] private string _productId;
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _buttonText;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClickBuy);
        Manager.IAP.OnProductsReady += OnProductsReady;
        Manager.IAP.OnNonConsumableOwned += OnNonConsumableOwned;
        _buttonText.text = "";
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClickBuy);
        Manager.IAP.OnProductsReady -= OnProductsReady;
        Manager.IAP.OnNonConsumableOwned -= OnNonConsumableOwned;
    }

    private void OnProductsReady(bool isProductsReady)
    {
        _button.interactable = isProductsReady;

        if (isProductsReady && Manager.IAP.CheckNonConsumableOwned(_productId))
        {
            _button.interactable = false;
            Debug.Log($"{_productId} 이미 구매함. 버튼 비활성화");
            _buttonText.text = "Purchased";
        }
    }

    private void OnNonConsumableOwned(string id)
    {
        if (id == _productId)
        {
            _button.interactable = false;
            _buttonText.text = "Purchased";
            Debug.Log($"{id} 구매 완료. 버튼 비활성화");
        }
    }

    private void OnClickBuy()
    {
        if (Manager.IAP.CheckNonConsumableOwned(_productId))
        {
            Debug.LogWarning("이미 구매한 아이템입니다.");
            _button.interactable = false;
            _buttonText.text = "Purchased";
            return;
        }

        Manager.IAP.TryPurchase(_productId);
        Debug.Log($"구매 시도: {_productId}");
    }
}
