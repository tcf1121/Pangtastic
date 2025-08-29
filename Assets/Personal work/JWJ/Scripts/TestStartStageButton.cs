using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestStartStageButton : MonoBehaviour
{
    [SerializeField] private Button _stageStartButton;
    [SerializeField] private TMP_Text _stageButtonText;

    private void Start()
    {
        int index = StageManager.Instance.CurrentStageIndex;
        _stageButtonText.text = $"Stage {index + 1}";

        _stageStartButton.onClick.AddListener(OnStageStartButtonClicked);
    }

    public void SetStage()
    {
        int index = StageManager.Instance.CurrentStageIndex;
        _stageButtonText.text = $"Stage {index + 1}";
    }

    private void OnStageStartButtonClicked()
    {
        SceneManager.LoadScene("JWJ_InGameTest Scene");
    }
}
