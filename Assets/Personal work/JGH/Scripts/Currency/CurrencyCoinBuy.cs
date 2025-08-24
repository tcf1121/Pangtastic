using UnityEngine;
using UnityEngine.UI;

public class CurrencyCoinBuy : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private int _spendAmount;

    private void Start()
    {
        if (_button == null) _button = GetComponent<Button>();
        
        // CoinManager 자동 검색
        var RewardSystem = FindObjectOfType<CurrencySystem>();
        if (RewardSystem != null)
        {
            _button.onClick.AddListener(() => CurrencySystem.Instance.SpendCoin(_spendAmount));
        }
    }
}
