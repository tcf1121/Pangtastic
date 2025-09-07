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
        UserInfoUI.Instance.SetActive(true);
        int index = Manager.Stage.CurrentStageIndex;
        _stageButtonText.text = $"Stage {index + 1}";
        
        // TODO: SCRIPT TEST
        //TODO: TEST 제거 해도됨 
        // Manager.Scripting.DialogLoadSO(6002);
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
}
