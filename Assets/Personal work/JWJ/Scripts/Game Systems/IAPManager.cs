using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Purchasing;
using UnityEngine.ResourceManagement.AsyncOperations;

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

        AsyncOperationHandle<ProductCatalogSO> handle = Addressables.LoadAssetAsync<ProductCatalogSO>("CatalogSO");
        handle.Completed += OnCatalogLoaded;
    }

    private void OnCatalogLoaded(AsyncOperationHandle<ProductCatalogSO> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _catalog = handle.Result;

            Debug.Log($"카탈로그 로드 완료.");
            InitializeIAP();
        }
        else
        {
            Debug.LogError($"카탈로그 로드 실패:{handle.OperationException}");
        }
    }

    public void RefreshOwnership()
    {
        if (_store != null && _isConnected && _isProductsReady)
        {
            _store.FetchPurchases();
            Debug.Log("구매내역 새로고침");
        }
        else
        {
            Debug.LogWarning("스토어 준비안됨 새로고침 실패");
        }
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

    public (string priceString, decimal priceValue, string currencyCode) GetPriceInfo(string productId)
    {
        if (_store == null) return ("", 0m, "");
        Product product = _store.GetProductById(productId);
        if (product == null || product.metadata == null) return ("", 0m, "");

        return (
            product.metadata.localizedPriceString,
            product.metadata.localizedPrice,
            product.metadata.isoCurrencyCode
        );
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
        _isProductsReady = true;
        OnProductsReady?.Invoke(true);
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
        //if (Manager.DB.GetUserRoot() == "guests" || Manager.DB.auth.CurrentUser.IsAnonymous)
        //{
        //    Debug.LogWarning("게스트 계정은 결제 불가");
        //    OutGameManager.Instance.OpenAccountLinkingPopup();
        //    return;
        //}

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

        if (CheckNonConsumableOwned(productId))
        {
            Debug.LogWarning("이미 구매한 아이템입니다.");
            OnNonConsumableOwned?.Invoke(productId);
            return;
        }

        _lastTriedProductId = productId;
        _store.PurchaseProduct(productId);
        Debug.Log($"구매 시도: {productId}");
    }

    public bool CheckNonConsumableOwned(string id)
    {
        Product product = _store.GetProductById(id);
        if (product != null && product.definition.type == ProductType.NonConsumable)
        {
            return _owned.Contains(id);
        }
        return false;
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
                case RewardType.RemovedAD:
                    Manager.Ad.BuyRemoveAD();
                    Debug.Log("광고제거 구매 성공");
                    break;
                case RewardType.Heart:
                    OutGameManager.AddIHReward(reward.RewardAmount);
                    break;
                case RewardType.Coin:
                    OutGameManager.AddReward(Goods.Gold, reward.RewardAmount);
                    break;
                case RewardType.UseItem:
                    OutGameManager.AddReward(Goods.Whisk, reward.RewardAmount);
                    OutGameManager.AddReward(Goods.Scissors, reward.RewardAmount);
                    OutGameManager.AddReward(Goods.DonutPan, reward.RewardAmount);
                    OutGameManager.AddReward(Goods.Coffee, reward.RewardAmount);
                    break;
                case RewardType.BoosterItem:
                    OutGameManager.AddReward(Goods.Roller, reward.RewardAmount);
                    OutGameManager.AddReward(Goods.DonutBox, reward.RewardAmount);
                    OutGameManager.AddReward(Goods.Oven, reward.RewardAmount);
                    break;

            }

        }
        OutGameManager.ShowRewardPopup();
    }

    public void TestPurchasing(string productId)
    {
        GiveReward(productId);
    }
}
