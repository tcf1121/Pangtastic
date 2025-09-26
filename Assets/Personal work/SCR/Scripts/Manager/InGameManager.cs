using SCR;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [SerializeField] GameObject _popupUI;
    [SerializeField] GameObject _clearUI;
    [SerializeField] GameObject _continueUI;
    [SerializeField] GameObject _retryUI;
    [SerializeField] TMP_Text _winCoinText;
    [SerializeField] TMP_Text _useCoinText;
    [SerializeField] Button _useCoinContinueButton;
    [SerializeField] Button _watchAddContinueButton;
    [SerializeField] List<Button> _gameButtons;
    [SerializeField] GameObject _adPanel;

    public static InGameManager Instate;
    private int _score;
    private int _coin;
    private bool _firstfail = true;
    [SerializeField] private bool _showInterstitialAd;
    [SerializeField] private int _useCoin;

    void Awake()
    {
        _showInterstitialAd = false;
        if (!Manager.Ad.RemovedAD)
        {
            _adPanel.SetActive(true);
            Manager.Ad.BannerCreateView();
            Manager.Ad.LoadAD();
            // 2일 이상부터만 적용
            if (Manager.Date.LoginStreak > 1)
            {
                Manager.Ad.LoadInterstitialAd();
                Manager.Timer.ADFin += ReadyShowAD;
                Manager.Timer.StartGame();
                Manager.Ad.OnInterstitialAdClosed += GoLobby;
            }
        }
        Manager.User.UseHeart();
        Instate = this;
        _customerFlowController.OnStageCleared += StageClear;
        _customerFlowController.OnStageFailed += StageFail;
        _useCoinContinueButton.onClick.AddListener(GoldContinueGame);
        _watchAddContinueButton.onClick.AddListener(AdContinueGame);
        _score = 0;
        _coin = 0;
        Manager.Audio.PlayPuzzleBGM();
        foreach (var btn in _gameButtons)
            btn.onClick.AddListener(PushButton);
    }

    void OnDestroy()
    {
        if (!Manager.Ad.RemovedAD)
        {
            Manager.Timer.ADFin -= ReadyShowAD;
            Manager.Ad.OnRewardAdClosed -= ContinueGame;
            Manager.Ad.OnInterstitialAdClosed -= GoLobby;
        }
    }

    private void ReadyShowAD()
    {
        _showInterstitialAd = true;
    }

    public static void SetPuzzleSize(int xPos, int yPos, int xSize, int ySize)
    {
        if (Instate == null) GameObject.Find("InGameManager").GetComponent<InGameManager>();
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

    public static bool GetStageClear()
    {
        if (Instate == null) Instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        return Instate._orderStateController.IsAllComplete();
    }

    public static void AddScore(int score)
    {
        if (Instate == null) Instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        Instate._score += score;

        Debug.Log($"현재 점수: {Instate._score}");

    }

    public static void ResetScore()
    {
        if (Instate == null) Instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        Instate._score = 0;
        Debug.Log($"점수 초기화. 현재 점수: {Instate._score}");
    }

    public static int GetScore()
    {
        if (Instate == null) Instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        return Instate._score;
    }

    public static void AddCoin(int num)
    {
        if (Instate == null) Instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        Instate._coin += num;
        Debug.Log($"현재 코인: {Instate._coin}");
    }

    public static GameObject GetPrefab(GemType gemType)
    {
        return Instate._blockList.GetPrefab(gemType);
    }

    public static int GetCoin()
    {
        if (Instate == null) Instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        return Instate._coin;
    }

    public static void AddIngredientSta(GemType ingredient)
    {
        if (Instate == null) Instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        Debug.Log(ingredient);
        Instate._orderStateController.AddIngredientSta(Instate._ingredientSOs[(int)ingredient]);
    }

    public static void SpawnCustomer()
    {
        if (Instate == null) Instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        Instate._customerFlowController.SpawnCustomer();
    }

    public static void StageClear()
    {
        if (Instate == null) Instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        Manager.Audio.PlaySFX("Stage_Clear");
        AddCoin(GetScore() / 100);
        KDJ.BoardManager.SetTouch(false);
        Instate._winCoinText.text = $"{GetCoin()}";
        Instate._clearUI.SetActive(true);
    }

    public static void StageFail()
    {
        if (Instate == null) Instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        Manager.Audio.PlaySFX("Stage_Fail");
        KDJ.BoardManager.SetTouch(false);
        if (Instate._firstfail)
        {
            Instate._firstfail = false;
            Manager.Ad.OnRewardAdClosed = null;
            Manager.Ad.OnRewardAdClosed += Instate.ContinueGame;
            Instate._continueUI.SetActive(true);
        }
        else Instate._retryUI.SetActive(true);
    }

    private void AdContinueGame()
    {
        if (!Manager.Ad.RemovedAD)
            Manager.Ad.ShowAD();
        else ContinueGame();
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
        // 팝업 UI의 자식개체 중 켜져있는게 있으면 반환
        foreach (Transform child in _popupUI.transform)
        {
            if (child.gameObject.activeSelf)
            {
                return;
            }
        }

        Debug.Log("게임 재실행");
        Time.timeScale = 1f;
        KDJ.BoardManager.SetTouch(true);
    }

    public static async Task RewardGem(List<GemType> gemList)
    {
        //보드판에 추가해라
        var tcs = new TaskCompletionSource<bool>();
        Instate.StartCoroutine(Instate.RewardGemWrapperRoutine(gemList, tcs));
        Debug.Log("보상 종료");
        await tcs.Task;
    }

    private IEnumerator RewardGemWrapperRoutine(List<GemType> gemList, TaskCompletionSource<bool> tcs)
    {
        yield return StartCoroutine(KDJ.BoardManager.Instance.RewardRoutine(gemList));
        tcs.SetResult(true);
    }

    public static List<GemType> GetTagetGem()
    {
        List<GemType> gemTypes = Instate._orderStateController.GetRequiredGem();

        return gemTypes;
    }

    public static void ClearGame()
    {
        if (Instate == null) Instate = GameObject.Find("InGameManager").GetComponent<InGameManager>();
        if (Instate._showInterstitialAd) Manager.Ad.ShowInterstitialAd();
        else Instate.GoLobby();
    }

    public void QuitGame()
    {
        if (_showInterstitialAd) Manager.Ad.ShowInterstitialAd();
        else GoLobby();
    }

    private void GoLobby()
    {
        Manager.Timer.EndGame();
        SceneManager.LoadScene(2/*로비씬*/);
    }

    public void PushButton()
    {
        Manager.Audio.PlaySFX("Touch");
    }

    public void LockButton()
    {
        Manager.Audio.PlaySFX("Item_fail");
    }
}
