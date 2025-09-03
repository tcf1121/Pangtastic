using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnterInGamePanel : MonoBehaviour
{
    [SerializeField] TMP_Text _level;
    [SerializeField] TMP_Text _whistNum;
    [SerializeField] TMP_Text _scissorsNum;
    [SerializeField] TMP_Text _donutPanNum;
    [SerializeField] TMP_Text _coffeeNum;
    [SerializeField] Toggle whistToggle;
    [SerializeField] Toggle scissorsToggle;
    [SerializeField] Toggle donutPanToggle;
    [SerializeField] Toggle coffeeToggle;
    [SerializeField] Button enterBtn;

    void Awake()
    {

    }

    void OnEnable()
    {
        _level.text = $"{Manager.Stage.CurrentStageIndex + 1}레벨";
        _whistNum.text = $"거품기 갯수";
        _scissorsNum.text = $"가위 갯수";
        _donutPanNum.text = $"도넛팬 갯수";
        _coffeeNum.text = $"커피 갯수";
        SetToggles();
        enterBtn.onClick.AddListener(EnterGame);
    }

    void SetToggles()
    {
        // if (거품기가 0이면)
        // {
        //     whistToggle.isOn = false;
        //     whistToggle.interactable = false;
        // }
        // if (가위가 0이면)
        // {
        //     whistToggle.isOn = false;
        //     scissorsToggle.interactable = false;
        // }
        // if (도넛팬이 0이면)
        // {
        //     whistToggle.isOn = false;
        //     donutPanToggle.interactable = false;
        // }
        // if (커피가 0이면)
        // {
        //     whistToggle.isOn = false;
        //     coffeeToggle.interactable = false;
        // }
    }

    void EnterGame()
    {
        if (Manager.Heart.TryStartStage())
        {

            // 아이템 사용 구문 추가
            /*
if(whistToggle.isOn)
if(scissorsToggle.isOn)
if(donutPanToggle.isOn)
if(coffeeToggle.isOn)

            */
            SceneManager.LoadScene("InGameTest Scene");
        }

    }
}
