using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderUIPresenter : MonoBehaviour
{
    [Header("스크립트")]
    [SerializeField] private OrderStateController _orderState; // 주문 상태 컨트롤러
    [SerializeField] private CustomerOrderController _customerOrder;
    [SerializeField] private CustomerFlowController _customerFlow;

    [Header("대사창")]
    [SerializeField] private GameObject _chatBox;
    [SerializeField] private TMP_Text _dialogue;
    [SerializeField] private float _dialogueDuration;

    [Header("이모지")]
    [SerializeField] private GameObject _emojiBox;
    [SerializeField] private Image _emojiImage;
    [SerializeField] private Sprite _emojiHigh;
    [SerializeField] private Sprite _emojiMid;
    [SerializeField] private Sprite _emojiLow;
    [SerializeField] private RectTransform _start;
    [SerializeField] private RectTransform _end;
    [SerializeField] private Color _startColor;
    [SerializeField] private Color _endColor;

    [Header("레시피 아이템 리스트")]
    [SerializeField] private List<OrderItem> _recipeSlotList = new List<OrderItem>();

    private List<OrderItem> _activeRecipeSlots = new List<OrderItem>();

    private Coroutine emojiCo;
    private Coroutine pairCo;
    private Coroutine firstDialogueCo;

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

        if (_customerFlow == null)
        {
            _customerFlow = FindObjectOfType<CustomerFlowController>();
        }

        // 이벤트 구독
        _orderState.OnOrderStarted += BuildOrderUI;
        _orderState.OnIngredientProgress += OnIngredientProgress;
        _orderState.OnRecipeCompleted += OnRecipeCompleted;
        _orderState.OnOrderCompleted += OnOrderCompleted;
        _orderState.OnOrderTimeout += OnOrderTimeout;
        _customerOrder.OnStageClear += OnStageClear;
        _customerFlow.OnCustomerSpawn += OnCustomerSpawn;
    }

    private void OnDestroy()
    {
        _orderState.OnOrderStarted -= BuildOrderUI;
        _orderState.OnIngredientProgress -= OnIngredientProgress;
        _orderState.OnRecipeCompleted -= OnRecipeCompleted;
        _orderState.OnOrderCompleted -= OnOrderCompleted;
        _orderState.OnOrderTimeout -= OnOrderTimeout;
        _customerOrder.OnStageClear -= OnStageClear;
        _customerFlow.OnCustomerSpawn -= OnCustomerSpawn;

        StopRunningCoroutines();
    }

    private void OnCustomerSpawn(CustomerSO customer)
    {
        StopRunningCoroutines();
        firstDialogueCo = StartCoroutine(FirstDialogueRoutine(customer));
    }

    // 주문이 새로 시작될 때 UI 생성
    private void BuildOrderUI(List<OrderRecipe> orderRecipes, List<RecipeSO> recipes)
    {
        ResetAllOrderSlots();
        _activeRecipeSlots.Clear();

        for (int i = 0; i < recipes.Count; i++)
        {
            OrderItem slot = _recipeSlotList[i]; //주문 들어온 레시피 수만큼
            slot.gameObject.SetActive(true); //켜줌
            slot.SetMenu(recipes[i].FoodPic);

            slot.BuildRows(orderRecipes[i]);
            slot.RecipeComplete(false);

            _activeRecipeSlots.Add(slot);
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
        StartEmojiOnly(percent);
    }

    private void OnStageClear(CustomerSO curCustomer, float averagePercent) // 스테이지 클리어
    {
        StartEmojiAndDialogue(averagePercent, curCustomer);
    }

    private void OnOrderTimeout(CustomerSO curCustomer) //주문 실패
    {
        StartEmojiAndDialogue(0f, curCustomer);
    }

    private void StartEmojiOnly(float percent) //이모지 코루틴 시작
    {
        StopRunningCoroutines();// 혹시 코루틴 도는동안 다음 재료가 들어올 수도 있으니 코루틴 중복 호출 방지
        emojiCo = StartCoroutine(EmojiRoutine(percent));
    }

    private void StartEmojiAndDialogue(float percent, CustomerSO curCustomer) //이모지 대사 코루틴 시작
    {
        StopRunningCoroutines(); //돌고있는 코루틴있으면 정리
        pairCo = StartCoroutine(PairRoutine(curCustomer, percent));
    }

    private IEnumerator FirstDialogueRoutine(CustomerSO customer)
    {
        StringSO stringSO = customer.DialogueEnter;

        if (stringSO == null)
        {
            _dialogue.text = "Hi! How are you?";
        }
        else
        {
            _dialogue.text = stringSO.GetText(Manager.Language.GetLanguage());
        }
        _chatBox.SetActive(true);
        yield return new WaitForSeconds(_dialogueDuration);
        _chatBox.SetActive(false);
    }

    private IEnumerator EmojiRoutine(float percent) //이모지 코루틴
    {
        _emojiImage.sprite = GetEmojiSprite(percent);
        _emojiBox.gameObject.SetActive(true);

        _emojiImage.rectTransform.position = _start.position;
        _emojiImage.color = _startColor;

        float elapsed = 0f;

        while (elapsed < _dialogueDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / _dialogueDuration; // 0 → 1

            // 위치 보간
            _emojiImage.rectTransform.position = Vector3.Lerp(_start.position, _end.position, t);

            // 색상 보간
            _emojiImage.color = Color.Lerp(_startColor, _endColor, t);

            yield return null;
        }
        _emojiBox.gameObject.SetActive(false);
    }

    private IEnumerator PairRoutine(CustomerSO curCustomer, float percent) //이모지 + 대사 코루틴
    {
        if (percent <= 0f)
        {
            StringSO stringSO = curCustomer.DialogueLeft;

            if (stringSO == null)
            {
                _dialogue.text = "This is not good";
            }
            else
            {
                _dialogue.text = stringSO.GetText(Manager.Language.GetLanguage());
            }
        }
        else if (percent <= 50f)
        {
            StringSO stringSO = curCustomer.DialogueMid;

            if (stringSO == null)
            {
                _dialogue.text = "Thanks";
            }
            else
            {
                _dialogue.text = stringSO.GetText(Manager.Language.GetLanguage());
            }
        }
        else
        {
            StringSO stringSO = curCustomer.DialogueHigh;

            if (stringSO == null)
            {
                _dialogue.text = "You are the Best";
            }
            else
            {
                _dialogue.text = stringSO.GetText(Manager.Language.GetLanguage());
            }
        }

        _emojiImage.sprite = GetEmojiSprite(percent);
        _emojiBox.gameObject.SetActive(true);
        _chatBox.gameObject.SetActive(true);

        yield return new WaitForSeconds(_dialogueDuration);

        _emojiBox.gameObject.SetActive(false);
        _chatBox.gameObject.SetActive(false);
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
        if (firstDialogueCo != null)
        {
            StopCoroutine(firstDialogueCo);
            firstDialogueCo = null;
        }
    }
}
