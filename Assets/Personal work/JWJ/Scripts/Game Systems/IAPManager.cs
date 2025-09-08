using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : Singleton<IAPManager>
{
    [Header("카탈로그SO")]
    [SerializeField] private ProductCatalogSO _catalog;

    private StoreController _store;
    private bool _isConnected;
    private bool _isProductsReady;
    private string _lastTriedProductId = null;

    public event Action<bool> OnProductsReady;
    public event Action<string> OnNonConsumableOwned;

    private HashSet<string> _owned = new HashSet<string>();

    protected override void Awake()
    {
        base.Awake();
        InitializeIAP();
    }

    private async void InitializeIAP()
    {
        _store = UnityIAPServices.StoreController();

        _store.OnPurchasePending += OnPurchasePending;
        _store.OnPurchaseConfirmed += OnPurchaseConfirmed;
        _store.OnPurchaseFailed += OnPurchaseFailed;

        try
        {
            await _store.Connect();
            _isConnected = true;
        }
        catch (SystemException exception)
        {
            _isConnected = false;
            Debug.LogError("IAP 연결실패 " + exception.Message);
            return;
        }

        _store.OnProductsFetched += OnProductsFetched; // 상품 정보 수신 콜백
        _store.OnProductsFetchFailed += OnProductsFetchFailed; // 상품 정보 수신 실패 콜백
        _store.OnPurchasesFetched += OnPurchasesFetched; // 구매 내역 수신 콜백 등록
        _store.OnStoreDisconnected += OnStoreDisconnected; // 연결 끊김 콜백 등록

        if (_catalog == null)
        {
            Debug.LogError("카탈로그 없음");
            return;
        }

        List<ProductDefinition> defs = _catalog.GetProductDefinitions();
        if (defs == null || defs.Count == 0)
        {
            Debug.LogError("상품 없음");
            return;
        }

        _store.FetchProducts(defs, null); // 상품 정보 초기화 
        //_store.FetchPurchases(); // 구매내역 조회
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription description)
    {
        _isConnected = false;
        Debug.LogWarning($"스토어 연결 끊김. 이유 : {description.message}");
        // 재연결 시도나 UI 비활성화 등 대응 로직을 여기
        OnProductsReady?.Invoke(false);
    }


    private void OnProductsFetched(List<Product> products)
    {
        _isProductsReady = true;
        _store.FetchPurchases();
        Debug.Log($"상품 정보 새로고침. 총 {products.Count}개");
        OnProductsReady?.Invoke(true);
    }

    private void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        _isProductsReady = false;
        Debug.Log($"상품 패치 실패. 이유: {failure.FailureReason}");
        OnProductsReady?.Invoke(false);
    }
    private void OnPurchasesFetched(Orders orders)
    {
        _owned.Clear();
        for (int i = 0; i < orders.ConfirmedOrders.Count; i++)
        {
            ConfirmedOrder confirmOrder = orders.ConfirmedOrders[i];
            var items = confirmOrder.CartOrdered.Items();

            if (items != null && items.Count > 0)
            {
                for (int j = 0; j < items.Count; j++)
                {
                    string id = items[j].Product.definition.id;
                    _owned.Add(id);
                    Debug.Log($"구매한 상품 ID: {id}");

                    Product product = _store.GetProductById(id);
                    if (product != null && product.definition.type == ProductType.NonConsumable)
                    {
                        OnNonConsumableOwned?.Invoke(id);
                    }
                }
            }
        }
    }


    private void OnPurchasePending(PendingOrder order)
    {
        Debug.Log($"구매 팬딩중: {order.Info.PurchasedProductInfo}");
        _store.ConfirmPurchase(order);
    }
    private void OnPurchaseConfirmed(Order order)
    {
        Debug.Log($"구매 완료 리십트: {order.Info.Receipt}");

        if (string.IsNullOrEmpty(_lastTriedProductId) == false)
        {
            _owned.Add(_lastTriedProductId);
            Debug.Log($"구매 완료 ID: {_lastTriedProductId}");

            GiveReward(_lastTriedProductId); //보상 지급

            //논컨슈머블 처리
            Product product = _store.GetProductById(_lastTriedProductId);
            if (product != null && product.definition.type == ProductType.NonConsumable)
            {
                OnNonConsumableOwned?.Invoke(_lastTriedProductId);
            }

            _lastTriedProductId = null;

            
        }
    }
    private void OnPurchaseFailed(FailedOrder order)
    {
        Debug.LogError($"구매 실패. 이유 : {order.FailureReason}");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_store != null)
        {
            _store.OnPurchasePending -= OnPurchasePending;
            _store.OnPurchaseConfirmed -= OnPurchaseConfirmed;
            _store.OnPurchaseFailed -= OnPurchaseFailed;
            _store.OnProductsFetched -= OnProductsFetched;
            _store.OnProductsFetchFailed -= OnProductsFetchFailed;
            _store.OnPurchasesFetched -= OnPurchasesFetched;
            _store.OnStoreDisconnected -= OnStoreDisconnected;
        }
    }

    public bool IsReady()
    {
        return _isConnected && _isProductsReady;
    }

    public bool IsPurchasable(string productId)
    {
        if (_store == null)
        {
            Debug.Log($"스토어매니저 없음. false 반환");
            return false;
        }

        Product product = _store.GetProductById(productId);
        if (product == null)
        {
            Debug.Log($"{product} 없음. false 반환");
            return false;
        }

        Debug.Log($"{product} 구매 가능");
        return product.availableToPurchase;
    }

    public void TryPurchase(string productId)
    {
        if (_store == null)
        {
            Debug.LogError("스토어매니저 없음");
            return;
        }
        if (IsReady() == false)
        {
            Debug.LogWarning("상품 아직 준비 안됨");
            return;
        }
        if (IsPurchasable(productId) == false)
        {
            Debug.LogWarning($"구매 불가 상품: {productId}");
            return;
        }
        _lastTriedProductId = productId;
        _store.PurchaseProduct(productId);
        Debug.Log($"구매 시도: {productId}");
    }

    public bool CheckNonConsumable(string id)
    {
        return _owned.Contains(id);
    }

    private void GiveReward(string productId)
    {
        ProductCatalogSO.Entry entry = _catalog.GetEntryById(productId);

        if (entry == null)
        {
            return;
        }

        for (int i = 0; i < entry.Rewards.Count; i++)
        {
            ProductCatalogSO.RewardEntry reward = entry.Rewards[i];
            Debug.Log($"보상 지급: {reward.RewardType} x{reward.RewardAmount}");

            //보상 예시
            switch (reward.RewardType)
            {
                case RewardType.Heart:
                    Manager.User.AddHeart(); //수량 있어야할듯
                    break;
                case RewardType.Coin:
                    Manager.User.AddCoin(reward.RewardAmount);
                    break;
                case RewardType.Star:
                    Manager.User.AddStar(reward.RewardAmount);
                    break;
            }

        }
    }
}
