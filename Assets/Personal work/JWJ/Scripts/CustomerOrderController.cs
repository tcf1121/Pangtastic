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

    public event Action<float> OnCustomerSuccess;
    public event Action<float> OnSpecialCustomerRewardGiven;
    public event Action<CustomerSO ,float> OnSpecialCustomerSuccess;
    //public event Action<CustomerSO> OnSpecialCustomerFail; //혹시라도 스페셜손님 실패 이벤트 추가될까봐 넣음 OnCustomerFail로 바꿔도 작동함
    public event Action OnCustomerFail;

    private void Awake()
    {
        if (_orderState == null)
        {
            _orderState = FindObjectOfType<OrderStateController>();
        }

        _orderState.OnOrderCompleted += OnOrderClear;
        _orderState.OnOrderTimeout += OnOrderFail;
    }

    private void OnDestroy()
    {
        _orderState.OnOrderCompleted -= OnOrderClear;
        _orderState.OnOrderTimeout -= OnOrderFail;
    }

    public void StartCustomerOrder(CustomerSO customer, StageSO stage) //손님 주문 시작
    {
        _curCustomer = customer;
        _curStage = stage;

        _orderRecipes = RecipeRule.BuildOrder(customer, stage); //주문가능 메뉴 리스트 만들기

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
            _specialPatientSum = 0;
            _specialSuccess = 0;
            StartSpecialOrder(0); //스페셜 전용 첫주문
        }
        else
        {
            _orderState.StartOrder(_orderRecipes, _curStage, _curCustomer); //일반, 유니크는 바로 주문
        }
    }

    private void StartSpecialOrder(int index)
    {
        //Debug.Log($"인덱스: {index}, 주문 레시피 수: {_orderRecipes.Count}");
        if (index >= _orderRecipes.Count) //주문 끝나면
        {
            EndSpecialCustomer();
            return;
        }

        _orderState.StartOrder(new List<RecipeSO> { _orderRecipes[index] }, _curStage, _curCustomer);
    }

    private void OnOrderClear(CustomerSO curCustomer, float remainPercent)
    {
        if (_curCustomer.Type == CustomerType.Special)
        {
            _specialSuccess++;
            //Debug.Log($"스페셜 주문 성공 수 : {_specialSuccess}");

            _specialPatientSum += remainPercent; //스페셜 손님 총 인내심 퍼센트 합

            int done = _specialSuccess; //여태 주문 받은 횟수. 다음 주문 인덱스

            if (done < _orderRecipes.Count) //아직 주문할게 남았으면
            {
                OnSpecialCustomerRewardGiven?.Invoke(remainPercent); //보상제공 이벤트
                StartSpecialOrder(done);
                return;
            }

            EndSpecialCustomer();
        }
        else //노멀, 유니크 손님 성공
        {
            OnCustomerSuccess?.Invoke(remainPercent);
            Debug.Log($"주문 클리어");
        }
    }

    private void OnOrderFail(CustomerSO customer)
    {
        OnCustomerFail?.Invoke();
    }

    private void EndSpecialCustomer()
    {
        float averagePercent = _specialPatientSum / _specialSuccess;
        OnSpecialCustomerSuccess?.Invoke(_curCustomer, averagePercent);
    }
}
