using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderUIPresenter : MonoBehaviour
{
    [Header("스크립트")]
    [SerializeField] private OrderStateController _orderState; // 주문 상태 컨트롤러
    [SerializeField] private CustomerOrderController _customerOrder;

    [Header("대사창")]
    [SerializeField] private GameObject _chatBox;
    [SerializeField] private TMP_Text _leftDialogue;
    [SerializeField] private float _dialogueDuration;

    [Header("이모지")]
    [SerializeField] private GameObject _emojiBox;
    [SerializeField] private Image _emojiImage;
    [SerializeField] private Sprite _emojiHigh;
    [SerializeField] private Sprite _emojiMid;
    [SerializeField] private Sprite _emojiLow;

    [Header("레시피 아이템 리스트")]
    [SerializeField] private List<OrderItem> _recipeSlotList = new List<OrderItem>(); 

    private List<OrderItem> _activeRecipeSlots = new List<OrderItem>();

    private Coroutine emojiCo;
    private Coroutine pairCo;

    private void Awake()
    {
        if (_orderState == null)
        {
            _orderState = FindObjectOfType<OrderStateController>();
        }

        if (_customerOrder == null)
        {
            _customerOrder = FindObjectOfType<CustomerOrderController>();
        }

        // 이벤트 구독
        _orderState.OnOrderStarted += BuildOrderUI;
        _orderState.OnIngredientProgress += OnIngredientProgress;
        _orderState.OnRecipeCompleted += OnRecipeCompleted;
        _orderState.OnOrderCompleted += OnOrderCompleted;
        _orderState.OnOrderTimeout += OnOrderTimeout;
        _customerOrder.OnSpecialCustomerSuccess += OnSpecialOrderCompleted;
        _customerOrder.OnSpecialCustomerFail += OnSpecialCustomerFail;
    }

    private void OnDestroy()
    {
        _orderState.OnOrderStarted -= BuildOrderUI;
        _orderState.OnIngredientProgress -= OnIngredientProgress;
        _orderState.OnRecipeCompleted -= OnRecipeCompleted;
        _orderState.OnOrderCompleted -= OnOrderCompleted;
        _orderState.OnOrderTimeout -= OnOrderTimeout;
        _customerOrder.OnSpecialCustomerSuccess -= OnSpecialOrderCompleted;
        _customerOrder.OnSpecialCustomerFail -= OnSpecialCustomerFail;

        StopRunningCoroutines();
    }

    // 주문이 새로 시작될 때 UI 생성
    private void BuildOrderUI(List<Dictionary<IngredientSO, int>> requiredList, List<RecipeSO> recipes)
    {
        ResetAllOrderSlots();
        _activeRecipeSlots.Clear();

        for (int i = 0; i < recipes.Count; i++) // 레시피 수만큼 반복
        {
            //Debug.Log($"슬롯{i}개");
            OrderItem slot = _recipeSlotList[i]; // i번째

            slot.gameObject.SetActive(true); // 슬롯 활성화
            slot.SetMenu(recipes[i].FoodPic); // 메뉴 이미지 세팅
            slot.BuildRows(requiredList[i]); //행 생성 
            slot.RecipeComplete(false); // 클리어 표시 리셋

            _activeRecipeSlots.Add(slot); // 활성 목록에 추가
        }
    }

    private void OnIngredientProgress(int recipeIndex, IngredientSO ing, int have, int need) // 진행도 업데이트
    {
        if (recipeIndex >= 0 && recipeIndex < _activeRecipeSlots.Count) //방어코드
        {
            _activeRecipeSlots[recipeIndex].UpdateRow(ing, have, need);
        }
    }

    private void OnRecipeCompleted(int recipeIndex) //레시피 성공
    {
        if (recipeIndex >= 0 && recipeIndex < _activeRecipeSlots.Count)
        {
            _activeRecipeSlots[recipeIndex].RecipeComplete(true); //클리어 표시
        }
    }

    private void OnOrderCompleted(CustomerSO curCustomer, float percent) //주문 성공
    {
        if (curCustomer.Type == CustomerType.Special) //스페셜 손님이면 중간주문은 이모지만
        {
            StartEmojiOnly(percent);
        }
        else // 노멀,유니크
        {
            StartEmojiAndDialogue(percent, curCustomer); // 이모지,대사 동시 표시
        }
    }

    private void OnSpecialOrderCompleted(CustomerSO curCustomer, float averagePercent) // 스페셜 최종 성공
    {
        StartEmojiAndDialogue(averagePercent, curCustomer);
    }

    private void OnOrderTimeout(CustomerSO curCustomer) //주문 실패
    {
        if (curCustomer.Type == CustomerType.Special)
        {
            StartEmojiOnly(0f);
        }
        else 
        {
            StartEmojiAndDialogue(0f, curCustomer);
        }
    }

    private void OnSpecialCustomerFail(CustomerSO curCustomer) // 스페셜 최종 실패
    {
        StartEmojiAndDialogue(0f, curCustomer);
    }

    private void StartEmojiOnly(float percent) //이모지 코루틴 시작
    {
        if (emojiCo != null) // 혹시 코루틴 도는동안 다음 재료가 들어올 수도 있으니 코루틴 중복 호출 방지
        {
            StopCoroutine(emojiCo);
            emojiCo = null;
        }
        emojiCo = StartCoroutine(EmojiRoutine(percent));
    }

    private void StartEmojiAndDialogue(float percent, CustomerSO curCustomer) //이모지 대사 코루틴 시작
    {
        StopRunningCoroutines(); //돌고있는 코루틴있으면 정리
        pairCo = StartCoroutine(PairRoutine(curCustomer, percent));
    }


    private void StopRunningCoroutines() //코루틴 정지
    {
        if (emojiCo != null)
        {
            StopCoroutine(emojiCo);
            emojiCo = null;
        }
        if (pairCo != null)
        {
            StopCoroutine(pairCo);
            pairCo = null;
        }
    }

    private IEnumerator PairRoutine(CustomerSO curCustomer, float percent) //이모지 + 대사 코루틴
    {
        if (percent <= 0f)
        {
            _leftDialogue.text = curCustomer.LeftDialogue;
        }
        else if (percent <= 50f)
        {
            _leftDialogue.text = curCustomer.MidDialogue;
        }
        else
        {
            _leftDialogue.text = curCustomer.HighDialogue;
        }

        _emojiImage.sprite = GetEmojiSprite(percent);
        _emojiBox.gameObject.SetActive(true);
        _chatBox.gameObject.SetActive(true);

        yield return new WaitForSeconds(_dialogueDuration);

        _emojiBox.gameObject.SetActive(false);
        _chatBox.gameObject.SetActive(false);
    }

    private IEnumerator EmojiRoutine(float percent) //이모지 코루틴
    {
        _emojiImage.sprite = GetEmojiSprite(percent);
        _emojiBox.gameObject.SetActive(true);

        yield return new WaitForSeconds(_dialogueDuration);
        _emojiBox.gameObject.SetActive(false);
    }


    private Sprite GetEmojiSprite(float percent) //필요한 이모지 이미지 넣기
    {
        if (percent <= 0f)
        {
            return _emojiLow;
        }
        else if (percent <= 50f)
        {
            return _emojiMid;
        }
        else
        {
            return _emojiHigh;
        }
    }

    private void ResetAllOrderSlots() // 슬롯 초기화
    {
        for (int i = 0; i < _recipeSlotList.Count; i++)
        {
            OrderItem slot = _recipeSlotList[i];
            slot.ResetUI();

            slot.gameObject.SetActive(false);
        }
    }
}
