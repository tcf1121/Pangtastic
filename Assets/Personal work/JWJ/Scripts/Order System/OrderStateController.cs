using SCR;
using System;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
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

    [SerializeField] private PatienceSliderUI _patienceUI;
    private CustomerSO _curCustomer;

    private List<OrderRecipe> _orderRecipes = new List<OrderRecipe>();

    private float _maxPatience = 100f; //인내심 최대값
    private float _curPatience; //현재 인내심
    private float _elapsed; //경과 시간
    private bool _isRunning; //타이머 동작중 여부
    private bool _hasEnded; //이미 성공/실패로 종료됐는지

    private void Awake()
    {
        _patienceUI = FindObjectOfType<PatienceSliderUI>();
    }

    public void StartOrder(List<RecipeSO> recipes, StageSO stage, CustomerSO customer, bool resetPatience)
    {
        _curCustomer = customer;
        _hasEnded = false;
        _isRunning = true;
        _orderRecipes.Clear();

        for (int i = 0; i < recipes.Count; i++) // 주문 레시피 순회
        {
            OrderRecipe orderRecipe = new OrderRecipe(recipes[i], stage); //레시피별로 필요재료, 배수 적용
            _orderRecipes.Add(orderRecipe);
        }

        OnOrderStarted?.Invoke(_orderRecipes, recipes); //UI로 정보 전달

        if(resetPatience)
        {
            AddPatience(100f);
        }
    }

    public void AddIngredientSta(IngredientSO ingredient) //블록 터지면 호출되는 함수
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
                Debug.Log("재료 추가");
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

    public void AddPatience(float amount)
    {
        float timeToReachZero = GetTimeToZero();

        if (_curPatience < 0f)
        {
            _curPatience = 0f;
        }
        Debug.Log($"현재 인내심: {_curPatience}, 추가 인내심 : {amount}");
        _curPatience += amount;
        Debug.Log($"더해진 인내심: {_curPatience}");

        if (_curPatience > _maxPatience)
        {
            _curPatience = _maxPatience;
        }

        // 진행도 = 총진행 - (현재/최대)
        float progress = 1f - (_curPatience / _maxPatience);

        // 경과 시간 재계산
        _elapsed = timeToReachZero * progress;

        _hasEnded = false;
        _isRunning = true;

        _patienceUI.SetPatience(_curPatience, _maxPatience);
    }
    private float GetTimeToZero()
    {
        float timeToReachZero = 0f;

        if (_curCustomer != null && _curCustomer.Type == CustomerType.Normal)
        {
            timeToReachZero = 60f;
        }
        else if (_curCustomer != null && _curCustomer.Type == CustomerType.Unique)
        {
            timeToReachZero = 50f;
        }
        else if (_curCustomer != null && _curCustomer.Type == CustomerType.Special)
        {
            timeToReachZero = 30f;
        }
        else
        {
            Debug.LogWarning("커스터머 타입 이상함");
            timeToReachZero = 60f;
        }

        return timeToReachZero;
    }


    private void StopPatience()
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
        if (_hasEnded)
        {
            return;
        }

        float timeToReachZero = GetTimeToZero();

        _elapsed += Time.deltaTime;
        float progress = _elapsed / timeToReachZero;

        if (progress > 1f)
        {
            progress = 1f;
        }

        _curPatience = _maxPatience * (1f - progress);

        _patienceUI.SetPatience(_curPatience, _maxPatience);

        if (progress >= 1f) // 시간이 끝나면
        {
            _isRunning = false; // 타이머 정지
            _hasEnded = true; // 종료
            OnOrderTimeout?.Invoke(_curCustomer);
        }
    }

    public List<GemType> GetRequiredGem()
    {
        List<GemType> gemTypes = new();
        foreach (var s in _orderRecipes)
        {
            for (int i = 0; i < s.IngredientCount; i++)
            {
                int remainder = s.GetRequiredAmount(i) - s.GetCollectedAmount(i);
                if (remainder > 0)
                {
                    for (int j = 0; j < remainder; j++)
                        gemTypes.Add(SoToGemtype(s.GetIngredient(i)));
                }
            }
        }
        return gemTypes;
    }

    private GemType SoToGemtype(IngredientSO ingredientSO)
    {
        if (ingredientSO.ID == 2501)
            return GemType.Lavender;
        else if (ingredientSO.ID == 2502)
            return GemType.Chocolate;
        else if (ingredientSO.ID == 2503)
            return GemType.Blueberry;
        else if (ingredientSO.ID == 2504)
            return GemType.Cheese;
        else if (ingredientSO.ID == 2505)
            return GemType.Strawberry;
        else if (ingredientSO.ID == 2506)
            return GemType.Sugar;
        else if (ingredientSO.ID == 2507)
            return GemType.Syrup;
        else
            return GemType.Egg;
    }
}
