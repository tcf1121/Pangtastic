using KDJ;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomerFlowController : MonoBehaviour
{
    [SerializeField] private CustomerOrderController _customerOrder;
    //[SerializeField] private BoardManager _boardManager;
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private Image _customerImage; // 손님 이미지

    private CustomerSO _curCustomer;

    public event Action<CustomerSO> OnCustomerSpawn;
    public event Action OnStageCleared;
    public event Action OnStageFailed;

    [SerializeField] private Button spawnButton; //테스트용 버튼

    private void Awake()
    {
        if (_customerOrder == null)
        {
            _customerOrder = FindObjectOfType<CustomerOrderController>();
        }
        //if (_boardManager == null)
        //{
        //    _boardManager = FindObjectOfType<BoardManager>();
        //}
        if(_scoreManager == null)
        {
            _scoreManager = FindObjectOfType<ScoreManager>();
        }
        _customerOrder.OnCustomerSuccess += OnCustomerSuccess;
        _customerOrder.OnCustomerFail += OnCustomerFail;
        _customerOrder.OnSpecialCustomerSuccess += OnSpecialCustomerSuccess;
        //_customerOrder.OnSpecialCustomerFail += OnSpecialCustomerFail;
        _customerOrder.OnSpecialCustomerRewardGiven += OnSpecialCustomerRewardGiven;

        spawnButton.onClick.AddListener(SpawnCustomer); //테스트용 버튼
    }

    private void OnDestroy()
    {
        _customerOrder.OnCustomerSuccess -= OnCustomerSuccess;
        _customerOrder.OnCustomerFail -= OnCustomerFail;
        _customerOrder.OnSpecialCustomerSuccess -= OnSpecialCustomerSuccess;
        //_customerOrder.OnSpecialCustomerFail -= OnSpecialCustomerFail;
        _customerOrder.OnSpecialCustomerRewardGiven -= OnSpecialCustomerRewardGiven;
    }

    public void SpawnCustomer()
    {
        StageSO curStage = StageManager.Instance.CurrentStage;

        _curCustomer = curStage.Customer;

        _customerImage.sprite = _curCustomer.CustomerPic;

        OnCustomerSpawn?.Invoke(_curCustomer);

        _customerOrder.StartCustomerOrder(_curCustomer, curStage); //손님 주문

        Debug.Log($"현재 스테이지 {curStage.StageID}");
    }

    private void OnCustomerSuccess(CustomerSO customer, float percentage)
    {
        StageClear(customer, percentage);
    }

    private void OnSpecialCustomerRewardGiven(float percentage)
    {
        int percent = Mathf.FloorToInt(percentage);
        Debug.Log($"스페셜 중간 성공 인내심 {percent}%. 보상제공");

        for (int i = 0; i < 2; i++) //보상 개수 두개
        {
            int rand = UnityEngine.Random.Range(0, 100);
            Debug.Log($"랜덤 숫자 {rand}");
            if (rand >= 90)
            {
                Debug.Log("도넛상자 제공");
            }
            else if (rand >= 68)
            {
                Debug.Log("팝콘 제공");
            }
            else if (rand >= 46)
            {
                Debug.Log("우유 제공");
            }
            else if (rand >= 23)
            {
                Debug.Log("밀대 세로 제공");
            }
            else if (rand >= 0)
            {
                Debug.Log("밀대 가로 제공");
            }
        }
    }

    private void OnSpecialCustomerSuccess(CustomerSO customer, float averagePercent)
    {
        StageClear(customer, averagePercent);
    }

    private void OnCustomerFail()
    {
        StageFail();
    }

    //private void OnSpecialCustomerFail(CustomerSO customer)
    //{
    //    StageFail();
    //}

    private void StageClear(CustomerSO customer, float percentage)
    {
        int percent = Mathf.FloorToInt(percentage); //int 로 변경

        Debug.Log($"스테이지 클리어. 보상 기준 인내심{percent}");

        StageManager.Instance.AdvanceStage();

        //특수블록 보상 도넛상자 제외
        if (percent >= 70)
        {
            //_boardManager.Spawner.SpawnRandomBlock
            Debug.Log("블록 4개 제공");
        }
        else if (percent >= 40)
        {
            Debug.Log("블록 3개 제공");
        }
        else if(percent >= 17)
        {
            Debug.Log("블록 2개 제공");
        }    
        else if(percent >= 0)
        {
            Debug.Log("블록 1개 제공");
        }

        _scoreManager.AddScore(percent * 10); //점수 전송

        OnStageCleared?.Invoke(); //스테이지 클리어 이벤트
        StartCoroutine(TmpChangeSceneRoutine()); //임시 씬 넘기기 코루틴
    }
    
    private void StageFail()
    {
        Debug.Log("스테이지 실패");
        OnStageFailed?.Invoke();

        StartCoroutine(TmpChangeSceneRoutine()); //임시 씬 넘기기 코루틴
    }

    private IEnumerator TmpChangeSceneRoutine() //임시 씬 넘기기 코루틴
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("StageSelectScene");
    }
}