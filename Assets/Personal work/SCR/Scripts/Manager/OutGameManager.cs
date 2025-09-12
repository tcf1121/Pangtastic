using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class OutGameManager : MonoBehaviour
{
    private static OutGameManager instate;
    public static OutGameManager Instance => instate;

    [SerializeField] private TMP_Text _coinText; // Coin 갯수
    [SerializeField] private TMP_Text _starText; // 별 갯수
    [SerializeField] private Button _stageStartButton;
    [SerializeField] private TMP_Text _stageButtonText;
    [SerializeField] private GameObject _accountLinkingPopup;

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
        UserInfoUI.Instance.SetActive(true);
        int index = Manager.Stage.CurrentStageIndex;
        _stageButtonText.text = $"Stage {index + 1}";
        Manager.Audio.SetLobbyPlace(Manager.User.GetCurPlace());
        Manager.Audio.PlayLobbyBGM();
    }

    public static void UpdateStageStartButton()
    {
        int index = Manager.Stage.CurrentStageIndex;
        instate._stageButtonText.text = $"Stage {index + 1}";
    }

    public void SetStage()
    {
        Manager.Stage.SetStage(int.Parse(_settingInputField.text.ToString()) - 1);
        int index = Manager.Stage.CurrentStageIndex;
        _stageButtonText.text = $"Stage {index + 1}";
    }

    public void OpenAccountLinkingPopup()
    {
        _accountLinkingPopup.SetActive(true);
    }
}
