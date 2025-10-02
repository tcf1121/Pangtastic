using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomerOrderController : MonoBehaviour
{
    [SerializeField] private OrderStateController _orderState;

    private CustomerSO _curCustomer;
    private StageSO _curStage;

    private List<RecipeSO> _orderRecipes = new List<RecipeSO>();
    
    private int _specialSuccess;
    private float _specialPatientSum;

    public event Action OnCustomerFail; //스테이지 실패
    public event Action<CustomerSO, float> OnOrderSegmentCleared; // 주문 한 번 완료
    public event Action<CustomerSO, float> OnStageClear; // 전체 주문 최종 완료

    private int _segmentSize; // 이번 손님이 한 번에 주문할 레시피 개수
    private int _segmentStartIndex; // 현재 진행 중인 주문 시작 인덱스
    private float _normalCurPatient;

    private void Awake()
    {
        if (_orderState == null)
        {
            _orderState = FindObjectOfType<OrderStateController>();
        }

        _orderState.OnOrderCompleted += OnOrderCompleted;
        _orderState.OnOrderTimeout += OnOrderFail;
    }

    private void OnDestroy()
    {
        _orderState.OnOrderCompleted -= OnOrderCompleted;
        _orderState.OnOrderTimeout -= OnOrderFail;
    }

    public void StartCustomerOrder(CustomerSO customer, StageSO stage) //손님 주문 시작
    {
        _curCustomer = customer;
        _curStage = stage;

        _orderRecipes = Manager.Stage.GetStageRecipes();

        Debug.Log($"주문하는 손님 이름[타입]: {_curCustomer}[{_curCustomer.Type}]");

        //주문 목록 로그
        RecipeSO[] arr = _orderRecipes.ToArray();
        string log = "주문 메뉴목록: ";
        for (int i = 0; i < arr.Length; i++)
        {
            log += arr[i].Name;
            if (i < arr.Length - 1)
                log += ", ";
        }
        Debug.Log(log);
        //여기까지 로그

        if (_curCustomer.Type == CustomerType.Special)
        {
            _segmentSize = 1;
        }
        else
        {
            _segmentSize = 2;
        }

        _segmentStartIndex = 0; // 첫 주문 시작 인덱스

        _specialPatientSum = 0f; // 스페셜 누적 퍼센트
        _specialSuccess = 0; // 스페셜 완료 개수

        
        StartOrderSegment(_segmentStartIndex, _segmentSize, true); // 첫 주문 주문
    }

    private void StartOrderSegment(int startIndex, int menuCount, bool forceReset)
    {
        if (startIndex >= _orderRecipes.Count) // 시작 인덱스가 전체 메뉴개수보다 크면 주문 끝
        {
            EndAnyCustomer();
            return;
        }

        int remaining = _orderRecipes.Count - startIndex;
        if (menuCount > remaining) //주문할 메뉴 수가 남은 메뉴 수 보다 많으면 남은것만 주문
        {
            menuCount = remaining;
        }

        List<RecipeSO> segment = new List<RecipeSO>();

        for (int i = 0; i < menuCount; i++) // 필요한 개수만큼
        {
            segment.Add(_orderRecipes[startIndex + i]); // 레시피 추가
        }

        bool resetPatience; // 주문시 인내심 초기화 여부
        if (_curCustomer.Type == CustomerType.Special)
        {
            resetPatience = true; // 스페셜은 매번 초기화
        }
        else
        {
            resetPatience = forceReset; // 일반/유니크는 첫 주문에만 초기화
        }

        _orderState.StartOrder(segment, _curStage, _curCustomer, resetPatience);
   
}

    private void OnOrderCompleted(CustomerSO curCustomer, float remainPercent) // 한 주문 완료
    {
        if (curCustomer.Type == CustomerType.Special)
        {
            _specialSuccess ++;
            _specialPatientSum += remainPercent;
        }
        else
        {
            _normalCurPatient = remainPercent; 
        }
        Debug.Log($"주문 하나 완료 남은 인내심: {remainPercent}%");

        OnOrderSegmentCleared?.Invoke(curCustomer, remainPercent); // 한 주문 완료

        int nextStart = _segmentStartIndex + _segmentSize; // 다음 주문 시작인덱스 계산

        if (nextStart < _orderRecipes.Count) // 남은 주문이 있으면
        {
            _segmentStartIndex = nextStart; // 시작 인덱스 갱신

            bool nextForceReset; // 다음주문 인내심 초기화 여부
            if (_curCustomer.Type == CustomerType.Special)
            {
                nextForceReset = true;
            }
            else
            {
                nextForceReset = false; // 일반/유니크 인내심 초기화로 바뀌면 이거 true 로 변경하면 됨
            }

            StartOrderSegment(_segmentStartIndex, _segmentSize, nextForceReset); // 다음 주문
            return;
        }

        EndAnyCustomer();
    }

    private void OnOrderFail(CustomerSO customer) //손님 받을 이유가 있나???
    {
        OnCustomerFail?.Invoke();
    }


    private void EndAnyCustomer()
    {
        float finalPercent;

        if (_curCustomer.Type == CustomerType.Special)
        {
            finalPercent = _specialPatientSum / _specialSuccess;
        }
        else
        {
            finalPercent = _normalCurPatient;
        }

        Debug.Log($"스테이지 종료 최종 인내심 : {finalPercent}");
        OnStageClear?.Invoke(_curCustomer, finalPercent); // 최종 완료이벤트
    }
}
