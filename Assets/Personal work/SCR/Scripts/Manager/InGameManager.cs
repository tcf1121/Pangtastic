using SCR;
using SCR_B;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    [SerializeField] private PrefabList _blockList;
    [SerializeField] private List<IngredientSO> _ingredientSOs;
    [SerializeField] OrderStateController _orderStateController;
    [SerializeField] CustomerFlowController _customerFlowController;
    [SerializeField] GameObject _clearUI;
    [SerializeField] GameObject _continueUI;
    [SerializeField] GameObject _retryUI;
    [SerializeField] TMP_Text _winCoinText;
    [SerializeField] TMP_Text _useCoinText;
    [SerializeField] Button _useCoinContinueButton;
    [SerializeField] Button _watchAddContinueButton;
    [SerializeField] List<Button> _gameButtons;

    private static InGameManager instate;
    private int _score;
    private int _coin;
    private bool _firstfail = true;
    private Action _finishAD;
    [SerializeField] private int _useCoin;

    void Awake()
    {
        Manager.Ad.LoadAD();
        Manager.User.UseHeart();
        instate = this;
        _customerFlowController.OnStageCleared += StageClear;
        _customerFlowController.OnStageFailed += StageFail;
        _finishAD += ContinueGame;
        _useCoinContinueButton.onClick.AddListener(GoldContinueGame);
        _watchAddContinueButton.onClick.AddListener(AdContinueGame);
        _score = 0;
        _coin = 0;
        Manager.Audio.PlayPuzzleBGM();
        foreach (var btn in _gameButtons)
            btn.onClick.AddListener(PushButton);
    }

    public static void SetPuzzleSize(int xPos, int yPos, int xSize, int ySize)
    {
        if (instate == null) GameObject.Find("InGameManager").GetComponent<InGameManager>();
        Camera.main.orthographicSize = xSize + 1;
        float puzzleXpos = 0;
        float puzzleYpos = 1f;
        if (xSize == 6)
        {
            if (xPos == -4)
                puzzleXpos = 1;
            else if (xPos == -2)
                puzzleXpos = -1;
        }
        else if (xSize == 7)
        {
            if (xPos == -4)
                puzzleXpos = -0.5f;
            else if (xPos == -3)
                puzzleXpos = 0.5f;
        }
        if (ySize == 6)
        {
            if (yPos == -4)
                puzzleYpos = 0f;
            else if (yPos == -2)
                puzzleYpos = 2f;
        }
        if (ySize == 7)
        {
            if (yPos == -4)
                puzzleYpos = 0.5f;
            else if (yPos == -3)
                puzzleYpos = 1.5f;
        }
        Camera.main.gameObject.transform.position = new Vector3(puzzleXpos, puzzleYpos, -10);

    }

    public static void AddScore(int score)
    {
        if (instate == null) GameObject.Find("InGameManager").GetComponent<InGameManager>();
        instate._score += score;

        Debug.Log($"현재 점수: {instate._score}");

    }

    public static void ResetScore()
    {
        if (instate == null) instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        instate._score = 0;
        Debug.Log($"점수 초기화. 현재 점수: {instate._score}");
    }

    public static int GetScore()
    {
        if (instate == null) instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        return instate._score;
    }

    public static void AddCoin(int num)
    {
        if (instate == null) instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        instate._coin += num;
    }

    public static GameObject GetPrefab(GemType gemType)
    {
        return instate._blockList.GetPrefab(gemType);
    }

    public static int GetCoin()
    {
        if (instate == null) instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        return instate._coin;
    }

    public static void AddIngredientSta(GemType ingredient)
    {
        if (instate == null) instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        Debug.Log(ingredient);
        instate._orderStateController.AddIngredientSta(instate._ingredientSOs[(int)ingredient]);
    }

    public static void SpawnCustomer()
    {
        if (instate == null) instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        instate._customerFlowController.SpawnCustomer();
    }

    public static void StageClear()
    {
        if (instate == null) instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        Manager.Audio.PlaySFX("Stage_Clear");
        AddCoin(GetScore() / 10);
        KDJ.BoardManager.SetTouch(false);
        instate._winCoinText.text = $"{GetCoin()}";
        instate._clearUI.SetActive(true);
    }

    public static void StageFail()
    {
        if (instate == null) instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        Manager.Audio.PlaySFX("Stage_Fail");
        KDJ.BoardManager.SetTouch(false);
        if (instate._firstfail)
        {
            instate._firstfail = false;
            instate._continueUI.SetActive(true);
        }
        else instate._retryUI.SetActive(true);
    }

    private void AdContinueGame()
    {
        Manager.Ad.ShowAD(_finishAD);
    }

    private void GoldContinueGame()
    {
        if (Manager.User.CanUseCoin(_useCoin))
        {
            Manager.User.UseCoin(_useCoin);
            ContinueGame();
        }
    }

    private void ContinueGame()
    {
        _orderStateController.AddPatience(50f);
        _continueUI.SetActive(false);
        KDJ.BoardManager.SetTouch(true);
    }

    public void PauseGame()
    {
        Debug.Log("게임 중지");
        Time.timeScale = 0f;
        KDJ.BoardManager.SetTouch(false);
    }

    public void ResumeGame()
    {
        Debug.Log("게임 재실행");
        Time.timeScale = 1f;
        KDJ.BoardManager.SetTouch(true);
    }

    public static void RewardGem(List<GemType> gemList)
    {
        //보드판에 추가해라
    }

    public static List<GemType> GetTagetGem()
    {
        List<GemType> gemTypes = instate._orderStateController.GetRequiredGem();

        return gemTypes;
    }

    public static void ClearGame()
    {
        SceneManager.LoadScene(2/*로비씬*/);
    }

    public void QuitGame()
    {
        SceneManager.LoadScene(2/*로비씬*/);
    }

    public void PushButton()
    {
        Manager.Audio.PlaySFX("Touch");
    }
}
