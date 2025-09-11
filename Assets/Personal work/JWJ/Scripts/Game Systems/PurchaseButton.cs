using System;
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

    private bool isSubscribe = false;

    private void Awake()
    {
        if(_button == null)
        {
            _button = GetComponent<Button>();
        }
        if (_buttonText == null)
        {
            _buttonText = _button.GetComponentInChildren<TMP_Text>();
        }

        if(!isSubscribe)
        {
            _button.onClick.AddListener(OnClickBuy);
            Manager.IAP.OnProductsReady += OnProductsReady;
            Manager.IAP.OnNonConsumableOwned += OnNonConsumableOwned;
            isSubscribe = true;
        }
        _button.interactable = false;
        //_buttonText.text = "";
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(_productId))
        {
            Debug.LogWarning($"{gameObject.name} : 상품이름 없어서 버튼 비활성화");
            _button.interactable = false;

            if(isSubscribe)
            {
                _button.onClick.RemoveListener(OnClickBuy);
                Manager.IAP.OnProductsReady -= OnProductsReady;
                Manager.IAP.OnNonConsumableOwned -= OnNonConsumableOwned;
                isSubscribe = false;
            }
            return;
        }

        OnProductsReady(Manager.IAP.IsReady());
    }

    private void OnDestroy()
    {
        if (isSubscribe)
        {
            _button.onClick.RemoveListener(OnClickBuy);
            Manager.IAP.OnProductsReady -= OnProductsReady;
            Manager.IAP.OnNonConsumableOwned -= OnNonConsumableOwned;
            isSubscribe = false;
        }
    }

    private void OnProductsReady(bool isProductsReady)
    {
        _button.interactable = isProductsReady;
        Debug.Log($"{_productId} 준비완료: {isProductsReady}");

        if (Manager.DB.auth.CurrentUser.IsAnonymous)
        {
            Debug.Log("게스트 계정은 구매목록 적용 안함");
            return;
        }

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
        Manager.IAP.TryPurchase(_productId);
        Debug.Log($"구매 시도: {_productId}");
    }
}
