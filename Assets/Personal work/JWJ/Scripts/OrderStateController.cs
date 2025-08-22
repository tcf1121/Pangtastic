using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderStateController : MonoBehaviour
{
    public event Action<List<Dictionary<IngredientSO, int>>, List<RecipeSO>> OnOrderStarted; //주문 시작 시 요구목록/레시피 전달
    public event Action<int, IngredientSO, int, int> OnIngredientProgress; //레시피 인덱스, 재료, 현재수, 필요수
    public event Action<int> OnRecipeCompleted;

    //노멀, 유니크 손님 성공/실패 이벤트
    public event Action<CustomerSO, float> OnOrderCompleted;
    public event Action<CustomerSO> OnOrderTimeout; //레시피 인덱스 //유지

    [SerializeField] private Slider patienceSlider;

    private List<RecipeSO> _curRecipes = new List<RecipeSO>(); //이번 주문의 레시피 목록
    private StageSO _curStage;
    private CustomerSO _curCustomer;

    private List<Dictionary<IngredientSO, int>> _requiredPerRecipe = new List<Dictionary<IngredientSO, int>>(); //레시피별 요구 재료
    private List<Dictionary<IngredientSO, int>> _collectedPerRecipe = new List<Dictionary<IngredientSO, int>>(); //레시피별 수집 재료
    private List<bool> _isRecipeCompleted = new List<bool>(); //레시피별 완료 여부

    private float _maxPatience; //인내심 최대값
    private float _curPatience; //현재 인내심
    private float _elapsed; //경과 시간
    private bool _isRunning; //타이머 동작중 여부
    private bool _hasEnded; //이미 성공/실패로 종료됐는지

    public void StartOrder(List<RecipeSO> recipes, StageSO stage, CustomerSO customer)
    {
        _curRecipes = recipes; //현재 레시피 목록 저장
        _curStage = stage; //현재 스테이지 저장
        _curCustomer = customer; //현재 손님 저장

        _requiredPerRecipe.Clear(); //이전 요구 목록 초기화
        _collectedPerRecipe.Clear(); //이전 수집 목록 초기화
        _isRecipeCompleted.Clear(); //이전 완료 목록 초기화

        foreach (RecipeSO recipe in _curRecipes) //레시피들을 순회
        {
            var required = RecipeRule.ApplyMultipliers(recipe, _curStage); //스테이지 보정 적용된 최종값
            var reqCopy = new Dictionary<IngredientSO, int>(); //내부 보관용 복사 딕셔너리 생성

            foreach (var kv in required) //요구 목록을 순회
            {
                reqCopy[kv.Key] = kv.Value; //키/값 복사
            }
            _requiredPerRecipe.Add(reqCopy); //요구 목록 리스트에 추가

            var colInit = new Dictionary<IngredientSO, int>(); //수집 초기값 딕셔너리
            foreach (var kv in reqCopy) //요구 재료 키 기반으로
            {
                colInit[kv.Key] = 0; //초기 수집량 0으로 설정
            }
            _collectedPerRecipe.Add(colInit); //수집 목록 리스트에 추가

            _isRecipeCompleted.Add(false); //해당 레시피 완료 플래그 false로 초기화
        }

        OnOrderStarted?.Invoke(_requiredPerRecipe, _curRecipes); //UI로 정보 전달

        StartPatience(); //인내심 감소 시작
    }

    public void AddIngredient(IngredientSO ingredient) //블록 터지면 호출되는 함수
    {
        if (_requiredPerRecipe == null || _requiredPerRecipe.Count == 0) //주문 들어오기 전 재료 안받음
        {
            Debug.Log("주문 없음");
            return;
        }

        if (_hasEnded)
        {
            Debug.Log("아직 재료를 수집할 수 없습니다");
            return;
        }

        for (int i = 0; i < _requiredPerRecipe.Count; i++) //각 레시피를 순회
        {
            if (_isRecipeCompleted[i]) //이미 완료된 레시피면
            {
                continue; //다음 레시피 검사
            }

            var reqIng = _requiredPerRecipe[i]; //필요 재료 딕셔너리
            var colIng = _collectedPerRecipe[i]; //수집 재료 딕셔너리

            if (!reqIng.ContainsKey(ingredient)) //이 레시피에 해당 제료가 필요 없으면
            {
                continue; //다음 레시피 검사
            }

            int need = reqIng[ingredient]; //필요 수량
            int cur = colIng.ContainsKey(ingredient) ? colIng[ingredient] : 0; //현재 수집 수량

            if (cur >= need) //이 재료가 더이상 필요 없을경우
            {
                continue; //다음 레시피 검사
            }

            colIng[ingredient] = cur + 1; //수집 개수 1 증가

            OnIngredientProgress?.Invoke(i, ingredient, colIng[ingredient], need); //UI에게 정보 전달

            if (IsRecipeComplete(i)) //이 레시피가 완료됐다면
            {
                _isRecipeCompleted[i] = true;

                OnRecipeCompleted?.Invoke(i);

                if (IsAllComplete()) //모든 주문이 완료되었다면
                {
                    _hasEnded = true; //다음 손님 주문 들어오기전까진 재료수집 막음

                    StopPatience(); //인내심 정지

                    float remainPercent = (_curPatience / _maxPatience) * 100; //성공한 인내심

                    OnOrderCompleted?.Invoke(_curCustomer ,remainPercent);
                }
            }
            break; //재료 반영 끝
        }
    }


    private bool IsRecipeComplete(int index) //레시피 완료 검사
    {
        var req = _requiredPerRecipe[index]; //요구량
        var col = _collectedPerRecipe[index]; //수집량

        foreach (var kv in req) //요구 항목 순회
        {
            int have = col[kv.Key]; //수집된 수량 조회
            if (have < kv.Value) //필요치 미달이면
            {
                return false; //미완료
            }
        }
        return true; //완료
    }

    private bool IsAllComplete() //전체 완료 검사
    {
        foreach (bool done in _isRecipeCompleted) //완료 레시피 순회
        {
            if (!done) //하나라도 미완료면
            {
                return false;
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
