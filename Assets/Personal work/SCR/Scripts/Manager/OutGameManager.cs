using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OutGameManager : MonoBehaviour
{
    private static OutGameManager instate;

    [SerializeField] private TMP_Text _coinText; // Coin 갯수
    [SerializeField] private TMP_Text _starText; // 별 갯수
    [SerializeField] private Button _stageStartButton;
    [SerializeField] private TMP_Text _stageButtonText;

    [SerializeField] private Button StartBtn;

    // 테스트용
    [SerializeField] private Button _settingBtn;
    [SerializeField] private TMP_InputField _settingInputField;

    void Awake()
    {
        instate = this;
        StartBtn.onClick.AddListener(OnStageStartButtonClicked);
        _settingBtn.onClick.AddListener(SetStage);
    }

    void Start()
    {
        Time.timeScale = 1f;
        UpdateCoinUI();
        UpdateStarUI();
        int index = StageManager.Instance.CurrentStageIndex;
        _stageButtonText.text = $"Stage {index + 1}";
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

    private void OnStageStartButtonClicked()
    {
        if (HeartSystem.Instance.TryStartStage())
            SceneManager.LoadScene("InGameTest Scene");
    }

    public void SetStage()
    {
        StageManager.Instance.SetStage(int.Parse(_settingInputField.text.ToString()) - 1);
        int index = StageManager.Instance.CurrentStageIndex;
        _stageButtonText.text = $"Stage {index + 1}";
    }
}
