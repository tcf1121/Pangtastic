using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutGameManager : MonoBehaviour
{
    private static OutGameManager instate;
    public static OutGameManager Instance => instate;

    [SerializeField] private Button _stageStartButton;
    [SerializeField] private TMP_Text _stageButtonText;
    [SerializeField] private GameObject _accountLinkingPopup;
    [SerializeField] private List<Button> _lobbyButtons;
    [SerializeField] private GameObject _adPanel;
    private string DROPDOWN_KEY = "BGM_Setting";
    private int _currentPlace;

    [SerializeField] RewardPopup _rewardPopup;

    // 테스트용
    [SerializeField] private Button _settingBtn;
    [SerializeField] private TMP_InputField _settingInputField;

    void Awake()
    {
        instate = this;


        _settingBtn.onClick.AddListener(SetStage);
        foreach (var btn in _lobbyButtons)
            btn.onClick.AddListener(PushButton);

    }

    void Start()
    {
        if (!Manager.Ad.RemovedAD)
        {
            if (Manager.IAP.CheckNonConsumableOwned("noads"))
            {
                Debug.Log("광고제거 구매이력 발견. 배너 광고제거 적용");
                Manager.Ad.BuyRemoveAD();
            }
            else
            {
                Debug.Log("배너 광고활성화");
                _adPanel.SetActive(true);
                Manager.Ad.LoadAD();
                Manager.Ad.BannerCreateView();
            }
        }
        Time.timeScale = 1f;
        UserInfoUI.Instance.SetActive(true);
        int index = Manager.Stage.CurrentStageIndex;
        _stageButtonText.text = $"Stage {index + 1}";
        if (PlayerPrefs.HasKey(DROPDOWN_KEY) == false) _currentPlace = 0;
        else _currentPlace = PlayerPrefs.GetInt(DROPDOWN_KEY);
        Manager.Audio.SetLobbyPlace((MissionPlace)_currentPlace);
        Manager.Audio.PlayLobbyBGM();

        // TODO: 광고 로그 쌓기
        // Manager.UserInfoSystem.SetLogAccessDates();
        // Manager.UserInfoSystem.GetLogAccessDates();

        // TODO: 주간 퀘스트
        // Manager.UserInfoSystem.CheckAndResetIfNewWeek();
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

    public void PushButton()
    {
        Manager.Audio.PlaySFX("Touch");
    }

    public static void AddReward(Goods goods, int index)
    {
        instate._rewardPopup.AddReward(goods, index);
    }

    public static void AddIHReward(float index)
    {
        instate._rewardPopup.AddIHReward(index);
    }

    public static void AddMusic()
    {
        instate._rewardPopup.AddMusic();
    }

    public static void ShowRewardPopup()
    {
        instate._rewardPopup.gameObject.SetActive(true);
    }

    public static void CloseBannerAd()
    {
        instate._adPanel.SetActive(false);
        Debug.Log("배너 광고 비활성화");
        Manager.Ad.BannerDestroyAd();
    }
}
