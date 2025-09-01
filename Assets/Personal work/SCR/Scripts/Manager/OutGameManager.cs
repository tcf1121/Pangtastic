using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutGameManager : MonoBehaviour
{
    private static OutGameManager instate;

    [SerializeField] private TMP_Text _coinText; // Coin 갯수
    [SerializeField] private TMP_Text _starText; // 별 갯수
    [SerializeField] private StageStartButton startbtn;
    [SerializeField] private Button btn;
    [SerializeField] private TMP_InputField tMP_InputField;

    void Awake()
    {
        instate = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        UpdateCoinUI();
        UpdateStarUI();
        StageManager.Instance.SetStartBtn(startbtn, btn, tMP_InputField);
    }

    /// <summary>
    /// UI(TextMeshPro) 업데이트
    /// </summary>
    public static void UpdateCoinUI()
    {
        if (instate._coinText != null)
            instate._coinText.text = CurrencySystem.Instance.GetCoins().ToString();
    }

    /// <summary>
    /// UI(TextMeshPro) 업데이트
    /// </summary>
    public static void UpdateStarUI()
    {
        if (instate._starText != null)
            instate._starText.text = CurrencySystem.Instance.GetStars().ToString();
    }
}
