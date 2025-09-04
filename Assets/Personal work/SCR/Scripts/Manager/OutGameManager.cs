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

    // 테스트용
    [SerializeField] private Button _settingBtn;
    [SerializeField] private TMP_InputField _settingInputField;

    void Awake()
    {
        instate = this;
        _settingBtn.onClick.AddListener(SetStage);
    }

    void Start()
    {
        Time.timeScale = 1f;
        UpdateCoinUI();
        UpdateStarUI();
        int index = Manager.Stage.CurrentStageIndex;
        _stageButtonText.text = $"Stage {index + 1}";
    }

    public static void UpdateStageStartButton()
    {
        int index = Manager.Stage.CurrentStageIndex;
        instate._stageButtonText.text = $"Stage {index + 1}";
    }

    /// <summary>
    /// UI(TextMeshPro) 업데이트
    /// </summary>
    public static void UpdateCoinUI()
    {
        if (instate._coinText != null)
            instate._coinText.text = Manager.Currency.GetCoins().ToString();
    }

    /// <summary>
    /// UI(TextMeshPro) 업데이트
    /// </summary>
    public static void UpdateStarUI()
    {
        if (instate._starText != null)
            instate._starText.text = Manager.Currency.GetStars().ToString();
    }

    public void SetStage()
    {
        Manager.Stage.SetStage(int.Parse(_settingInputField.text.ToString()) - 1);
        int index = Manager.Stage.CurrentStageIndex;
        _stageButtonText.text = $"Stage {index + 1}";
    }
}
