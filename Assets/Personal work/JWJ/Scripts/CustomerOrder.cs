using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomerOrder : MonoBehaviour
{
    [SerializeField] private StageSetting _stageSetting;

    public List<RecipeSO> CurRecipes = new List<RecipeSO>();

    [SerializeField] private UnityEvent onAllOrdersComplete;

    private List<Dictionary<IngredientSO, int>> _requiredPerRecipe = new List<Dictionary<IngredientSO, int>>(); // 레시피별 최종 요구 재료 딕셔너리
    private List<Dictionary<IngredientSO, int>> _collectedPerRecipe = new List<Dictionary<IngredientSO, int>>(); // 레시피별 누적 재료 딕셔너리
    private List<bool> isRecipeCompleted = new List<bool>(); // 레시피별 완료 여부

    private void Awake()
    {
        if (_stageSetting == null)
        {
            _stageSetting = FindObjectOfType<StageSetting>();
        }
    }
    public void AddIngredient(IngredientSO ingredient) // 블록이 터질 때 들어오는 재료
    {
        if (_requiredPerRecipe == null || _requiredPerRecipe.Count == 0) // 주문이 없으면
        {
            Debug.LogWarning("주문이 없습니다.");
            return;
        }

        for (int i = 0; i < _requiredPerRecipe.Count; i++) // 각 레시피를 순서대로 검사
        {
            if (isRecipeCompleted[i]) // 이미 완료된 레시피면
            {
                continue; // 건너뜀
            }

            Dictionary<IngredientSO, int> reqIng = _requiredPerRecipe[i]; // 필요한 재료 딕셔너리
            Dictionary<IngredientSO, int> colIng = _collectedPerRecipe[i]; // 모은 재료 딕셔너리

            if (reqIng.ContainsKey(ingredient) == false) // 이 레시피에 받은 재료가 필요없으면
            {
                continue; // 다음 레시피로 넘어감
            }

            int need = reqIng[ingredient]; // 재료 요구 수량
            int cur = 0; // 현재 누적 수량

            if (colIng.TryGetValue(ingredient, out cur) == false) // 누적 딕셔너리에 키가 없으면
            {
                cur = 0;
            }

            if (cur >= need) // 이미 요구치에 도달했다면
            {
                continue; // 다음 레시피에 필요한지 검사
            }

            colIng[ingredient] = cur + 1; // 재료 1개 누적

            Debug.Log($"{CurRecipes[i].Name} 재료 {ingredient.Name} {colIng[ingredient].ToString()} / {need.ToString()}"); 
            // 진행 상황

            if (IsRecipeComplete(i)) // 해당 레시피가 완성되었는지 검사
            {
                isRecipeCompleted[i] = true; // 완료 플래그 설정
                Debug.Log("[완료] 레시피 클리어: " + CurRecipes[i].Name); // 완료 로그

                if (IsAllComplete()) // 모든 레시피가 완료되었는지 검사
                {
                    Debug.Log("모든 주문 완료!"); 
                    if (onAllOrdersComplete != null)
                    {
                        onAllOrdersComplete.Invoke(); // 손님 클리어 이벤트 발생
                    }
                }
            }
            break; // 재료가 들어가면 반복문 종료
        }
    }

    public void OnOrderReceived() // 주문이 들어왔을 때 호출할 함수
    {
        _requiredPerRecipe.Clear(); // 이전 주문리스트 초기화
        _collectedPerRecipe.Clear(); // 이전 주문리스트 초기화
        isRecipeCompleted.Clear(); // 이전 주문리스트 초기화

        for (int i = 0; i < CurRecipes.Count; i++) // 레시피 리스트를 순회
        {
            RecipeSO recipe = CurRecipes[i]; // i번째 레시피 가져오기

            Dictionary<IngredientSO, int> required; // 필요한 재료 저장용 딕셔너리
            _stageSetting.CurRecipe = recipe; // 현재 레시피를 StageSetting에 전달
            _stageSetting.InitStageRecipe(); // 스테이지 보정 계산 수행
            required = _stageSetting.GetRequiredIngs(); // 최종 요구 재료 딕셔너리 가져오기

            Dictionary<IngredientSO, int> reqCopy = new Dictionary<IngredientSO, int>(); // StageRecipeSet 내부 딕셔너리를 복사할 새로운 딕셔너리
            foreach (KeyValuePair<IngredientSO, int> ing in required) // 복사
            {
                reqCopy[ing.Key] = ing.Value; // 키와 값 복사
            }
            _requiredPerRecipe.Add(reqCopy); // 레시피별 요구 목록에 추가

            Dictionary<IngredientSO, int> colInit = new Dictionary<IngredientSO, int>(); // 누적 딕셔너리 초기화용
            foreach (KeyValuePair<IngredientSO, int> kv2 in reqCopy) // 요구 키들을 기준으로
            {
                colInit[kv2.Key] = 0; // 시작 누적값 0으로 세팅
            }
            _collectedPerRecipe.Add(colInit); // 레시피별 누적 목록에 추가

            isRecipeCompleted.Add(false); // 아직 완료되지 않았다고 표시

            Debug.Log("주문메뉴: " + recipe.Name); // 레시피 이름

            foreach (KeyValuePair<IngredientSO, int> _ing in reqCopy) // 요구 재료 순회
            {
                IngredientSO ingredient = _ing.Key; // 재료 SO
                int amount = _ing.Value; // 필요 수량
                string ingName = (ingredient != null) ? ingredient.Name : "Unknown"; // 이름 확인
                Debug.Log(" - " + ingName + " x" + amount.ToString()); // 로그 출력
            }
        }
    }

    private bool IsRecipeComplete(int index) // 특정 레시피가 완료되었는지 검사하는 함수
    {

        Dictionary<IngredientSO, int> req = _requiredPerRecipe[index]; // 요구 딕셔너리 참조
        Dictionary<IngredientSO, int> col = _collectedPerRecipe[index]; // 누적 딕셔너리 참조

        foreach (KeyValuePair<IngredientSO, int> kv in req) // 모든 요구 항목을 순회
        {
            IngredientSO ing = kv.Key; // 현재 검사 중인 재료
            int need = kv.Value; // 요구 수량
            int have = 0; // 보유 수량 변수
            if (col.TryGetValue(ing, out have) == false) // 누적에 키가 없으면
            {
                return false; // 아직 수집되지 않은 것으로 간주
            }
            if (have < need) // 요구 수량에 미달이면
            {
                return false; // 미완료
            }
        }
        return true; // 모든 요구 항목이 충족되면 완료
    }

    private bool IsAllComplete() // 모든 레시피가 완료되었는지 검사하는 함수
    {
        for (int i = 0; i < isRecipeCompleted.Count; i++) // 완료 플래그 배열을 순회
        {
            if (isRecipeCompleted[i] == false) // 하나라도 미완료가 있으면
            {
                return false; // 전체 미완료
            }
        }
        return true; // 전부 완료면 true
    }
}
