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

    private void OnCustomerSuccess(float percentage)
    {
        StageClear(percentage);
    }

    private void OnSpecialCustomerRewardGiven(float percentage)
    {
        Debug.Log($"성공 인내심 {percentage}%. 보상제공");
        //특수블록 확률은 깃[손님 유형] 참고
    }

    private void OnSpecialCustomerSuccess(CustomerSO customer, float averagePercent)
    {
        StageClear(averagePercent);
    }

    private void OnCustomerFail()
    {
        StageFail();
    }

    //private void OnSpecialCustomerFail(CustomerSO customer)
    //{
    //    StageFail();
    //}

    private void StageClear(float percentage)
    {
        Debug.Log($"스테이지 클리어. 보상 기준 인내심{percentage}");

        StageManager.Instance.AdvanceStage();

        OnStageCleared?.Invoke();
        //특수블록 보상 도넛상자 제외

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