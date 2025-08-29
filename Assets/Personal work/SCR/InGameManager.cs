using SCR;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    [SerializeField] OrderStateController orderStateController;
    [SerializeField] CustomerFlowController customerFlowController;
    [SerializeField] GameObject doNotTouch;
    [SerializeField] GameObject clearUI;
    [SerializeField] GameObject failUI;
    [SerializeField] TMP_Text WinScore;
    [SerializeField] TMP_Text WinCoin;
    [SerializeField] TMP_Text LoseScore;

    private static InGameManager instate;
    private int _score;
    private int _coin;

    void Awake()
    {
        instate = this;
        customerFlowController.OnStageCleared += StageClear;
        customerFlowController.OnStageFailed += StageFail;
        _score = 0;
        _coin = 0;
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
        if (instate == null) GameObject.Find("InGameManager").GetComponent<InGameManager>();
        instate._score = 0;
        Debug.Log($"점수 초기화. 현재 점수: {instate._score}");
    }

    public static int GetScore()
    {
        if (instate == null) GameObject.Find("InGameManager").GetComponent<InGameManager>();
        return instate._score;
    }

    public static void AddCoin(int num)
    {
        if (instate == null) GameObject.Find("InGameManager").GetComponent<InGameManager>();
        instate._coin += num;
    }

    public static int GetCoin()
    {
        if (instate == null) GameObject.Find("InGameManager").GetComponent<InGameManager>();
        return instate._coin;
    }

    public static void AddIngredientSta(IngredientSO ingredient)
    {
        if (instate == null) GameObject.Find("InGameManager").GetComponent<InGameManager>();
        instate.orderStateController.AddIngredientSta(ingredient);
    }

    public static void SpawnCustomer()
    {
        if (instate == null) GameObject.Find("InGameManager").GetComponent<InGameManager>();
        instate.customerFlowController.SpawnCustomer();
    }

    public static void StageClear()
    {
        if (instate == null) GameObject.Find("InGameManager").GetComponent<InGameManager>();
        instate.doNotTouch.SetActive(true);
        AddCoin(GetScore() / 10);
        instate.WinCoin.text = $"{GetCoin()}";
        instate.WinScore.text = $"{GetScore()}";
        instate.clearUI.SetActive(true);
    }

    public static void StageFail()
    {
        if (instate == null) GameObject.Find("InGameManager").GetComponent<InGameManager>();
        instate.doNotTouch.SetActive(true);
        //instate.LoseCoin.text = $"{GetCoin()}";
        instate.LoseScore.text = $"{GetScore()}";
        HeartSystem.Instance.UseHearts();
        instate.failUI.SetActive(true);
    }

    public static void RewardGem(List<GemType> gemList)
    {
        //보드판에 추가해라
    }

}
