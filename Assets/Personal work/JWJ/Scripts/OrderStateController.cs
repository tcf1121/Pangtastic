using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderStateController : MonoBehaviour
{
    public event Action<List<OrderRecipe>, List<RecipeSO>> OnOrderStarted; //주문 시작 시 요구목록/레시피 전달
    public event Action<int, IngredientSO, int, int> OnIngredientProgress; //레시피 인덱스, 재료, 현재수, 필요수
    public event Action<int> OnRecipeCompleted;

    //노멀, 유니크 손님 성공/실패 이벤트
    public event Action<CustomerSO, float> OnOrderCompleted;
    public event Action<CustomerSO> OnOrderTimeout; //레시피 인덱스

    [SerializeField] private Slider patienceSlider;

    private CustomerSO _curCustomer;

    private List<OrderRecipe> _orderRecipes = new List<OrderRecipe>();

    private float _maxPatience; //인내심 최대값
    private float _curPatience; //현재 인내심
    private float _elapsed; //경과 시간
    private bool _isRunning; //타이머 동작중 여부
    private bool _hasEnded; //이미 성공/실패로 종료됐는지

    public void StartOrder(List<RecipeSO> recipes, StageSO stage, CustomerSO customer)
    {
        _curCustomer = customer;

        _orderRecipes.Clear();

        for (int i = 0; i < recipes.Count; i++) // 주문 레시피 순회
        {
            OrderRecipe orderRecipe = new OrderRecipe(recipes[i], stage); //레시피별로 필요재료, 배수 적용
            _orderRecipes.Add(orderRecipe);
        }

        OnOrderStarted?.Invoke(_orderRecipes, recipes); //UI로 정보 전달

        StartPatience(); //인내심 감소 시작
    }

    public void AddIngredient(IngredientSO ingredient) //블록 터지면 호출되는 함수
    {
        if (_orderRecipes.Count == 0)
        {
            Debug.Log("주문 없음");
            return;
        }
        if (_hasEnded)
        {
            Debug.Log("스테이지 종료됨 재료수집 불가능");
            return;
        }

        for (int i = 0; i < _orderRecipes.Count; i++) //레시피 순회
        {
            OrderRecipe recipe = _orderRecipes[i];

            if (recipe.IsCompleted) //이미 완료된 레시피면 건너뜀
            {
                continue;
            }

            bool collected = recipe.CollectIngredient(ingredient, out int have, out int need); //재료 수집 시도

            if (collected) // 재료가 반영되면
            {
                OnIngredientProgress?.Invoke(i, ingredient, have, need); // UI 갱신 이벤트

                if (recipe.IsCompleted) // 이 레시피가 완료됐다면
                {
                    OnRecipeCompleted?.Invoke(i); // 레시피 완료 이벤트
                }

                if (IsAllComplete()) //주문 전체 완료되면
                {
                    _hasEnded = true; //재료수집 막음
                    StopPatience();

                    float remainPercent = (_curPatience / _maxPatience) * 100f;
                    OnOrderCompleted?.Invoke(_curCustomer, remainPercent); //주문 완료 이벤트 (남은 인내심 포함)
                }
                break; //재료 반영되면 반복문 종료
            }
        }
    }


    private bool IsAllComplete()
    {
        foreach (var recipe in _orderRecipes) //주문 레시피 목록 순회
        {
            if (!recipe.IsCompleted) //완료 안된 레시피가 있으면
            {
                return false; //false 반환
            }
        }
        return true;
    }

    private void StartPatience() //인내심 시작
    {
        _maxPatience = _curCustomer.BASE_PATIENCE;
        _curPatience = _maxPatience;

        //슬라이더 설정
        patienceSlider.minValue = 0f;
        patienceSlider.maxValue = _maxPatience;
        patienceSlider.value = _curPatience;

        _elapsed = 0f; //경과시간
        _hasEnded = false; //주문종료?
        _isRunning = true; //인내심 작동중?
    }

    private void StopPatience() //인내심 정지
    {
        _isRunning = false;
    }

    private void Update()
    {
        if (_isRunning) //인내심 돌아가는 중 아닐때
        {
            PatienceGaugeDown();
        }
    }

    private void PatienceGaugeDown()
    {
        if (_hasEnded) //주문 종료됐으면
        {
            return;
        }

        float total = _curCustomer.TimeToReachZero; //0까지 도달하는 총 시간초
        _elapsed += Time.deltaTime; //경과시간 누적

        float progress = _elapsed / total; //진행 비율 계산
        if (progress > 1) //혹시 음수로 내려가면 
        {
            progress = 1; //0으로 고정
        }

        _curPatience = _maxPatience * (1 - progress); //현재 인내심은 최대에서 진행 비율만큼 감소하는 값

        patienceSlider.value = _curPatience; //슬라이더에 벨류 반영

        if (progress >= 1) //타임 오버
        {
            _isRunning = false;
            _hasEnded = true;
            OnOrderTimeout?.Invoke(_curCustomer);
        }
    }
}
