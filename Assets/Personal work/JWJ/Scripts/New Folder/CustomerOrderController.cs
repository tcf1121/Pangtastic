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
    private int _specialFail;
    private int _specialNeed;
    private int _specialTotal;

    public event Action<float> OnCustomerSuccess;
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

        _orderRecipes = RecipeRuleService.BuildOrder(customer, stage); //주문가능 메뉴 리스트 만들기

        //Debug.Log($"주문하는 손님 이름[타입]: {_curCustomer}[{_curCustomer.Type}]");

        //주문 목록 로그
        //RecipeSO[] arr = _orderRecipes.ToArray();
        //
        //string log = "주문 메뉴목록: ";
        //
        //for (int i = 0; i < arr.Length; i++)
        //{
        //    log += arr[i].Name;
        //    if (i < arr.Length - 1)
        //        log += ", ";
        //}
        //
        //Debug.Log(log);
        //여기까지 로그

        if (_curCustomer.Type == CustomerType.Special)
        {
            _specialSuccess = 0;
            _specialFail = 0;
            _specialTotal = _orderRecipes.Count;
            _specialNeed = Mathf.CeilToInt(_specialTotal * 0.5f); //스페셜 손님 클리어 조건. 성공률 50% 이상 int. 소수점 올림
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
        if (index >= _orderRecipes.Count) //주문 목록을 다 소비 할이 없을것같음
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
            
            //스페셜 손님 대사 조건 퍼센트 계산 여기서
            

            int done = _specialSuccess + _specialFail; //여태 주문 받은 횟수. 다음 주문 인덱스

            if (_specialSuccess >= _specialNeed) //스페셜 손님 성공조건 충족시 즉시 성공
            {
                //스페셜 손님 보상로직 여기에
                Debug.Log("스페셜 손님 클리어. 보상 제공");
                OnCustomerSuccess?.Invoke(remainPercent);
                return;
            }
            StartSpecialOrder(done);
        }
        else //노멀, 유니크 손님 성공
        {
            OnCustomerSuccess?.Invoke(remainPercent);
            Debug.Log($"주문 클리어");
        }
    }

    private void OnOrderFail(CustomerSO customer)
    {
        if (_curCustomer.Type == CustomerType.Special) //스페셜 손님이면
        {
            _specialFail++;
            Debug.Log($"스페셜 주문 실패 수 : {_specialFail}");

            int done = _specialSuccess + _specialFail; //여태 주문 받은 횟수
            int remaining = _specialTotal - done;

            if (_specialSuccess + remaining < _specialNeed) //성공 불가능하면 바로 실패처리
            {
                OnCustomerFail?.Invoke();
                Debug.Log("스페셜 성공 불가능. 즉시 실패");
                return;
            }
            StartSpecialOrder(done); //아직 안끝났으면 주문
        }
        else //스페셜손님 아니면
        {
            OnCustomerFail?.Invoke();
        }
    }

    private void EndSpecialCustomer() //성공 여부 판정에 따라 삭제될 수 있음
    {
        if (_specialSuccess >= _specialNeed)
        {
            OnCustomerSuccess?.Invoke(0);
        }
        else
        {
            OnCustomerFail?.Invoke();
        }
    }
}
