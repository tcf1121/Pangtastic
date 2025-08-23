using UnityEngine;
using UnityEngine.UI;

public class CoinBuy : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private int _spendAmount;

    private void Start()
    {
        if (_button == null) _button = GetComponent<Button>();
        
        // CoinManager 자동 검색
        var CoinSystem = FindObjectOfType<CoinSystem>();
        if (CoinSystem != null)
        {
            _button.onClick.AddListener(() => CoinSystem.Instance.SpendCoin(_spendAmount));
        }
    }
}
