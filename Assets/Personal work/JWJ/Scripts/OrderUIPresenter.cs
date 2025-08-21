using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderUIPresenter : MonoBehaviour
{
    [SerializeField] private OrderStateController _orderState; // 주문 상태 컨트롤러
    [SerializeField] private CustomerOrderController _customerOrder;

    [SerializeField] private GameObject orderItemPrefab;       // 주문 아이템 프리팹
    [SerializeField] private Transform orderItemParent;        // UI 부모 

    [SerializeField] private GameObject _chatBox;
    [SerializeField] private TMP_Text _leftDialogue;
    [SerializeField] private float _dialogueDuration;

    [SerializeField] private GameObject _emojiBox;
    [SerializeField] private Image _emojiImage;
    [SerializeField] private Sprite _emojiHigh;
    [SerializeField] private Sprite _emojiMid;
    [SerializeField] private Sprite _emojiLow;

    private List<OrderItem> _orderItems = new List<OrderItem>();

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

        StopAllCoroutines(); //이거 필요함?
    }

    // 주문이 새로 시작될 때 UI 생성
    private void BuildOrderUI(List<Dictionary<IngredientSO, int>> requiredList, List<RecipeSO> recipes)
    {
        ClearOrderUI();

        for (int i = 0; i < recipes.Count; i++)
        {
            GameObject obj = Instantiate(orderItemPrefab, orderItemParent);
            OrderItem orderItem = obj.GetComponent<OrderItem>();

            // 메뉴 이미지 세팅
            orderItem.SetMenu(recipes[i].FoodPic);

            // 요구 재료 행들 생성
            orderItem.BuildRows(requiredList[i]);

            _orderItems.Add(orderItem);
        }
    }

    private void OnIngredientProgress(int recipeIndex, IngredientSO ing, int have, int need)
    {
        if (recipeIndex >= 0 && recipeIndex < _orderItems.Count)
        {
            _orderItems[recipeIndex].UpdateRow(ing, have, need);
        }
    }
    private void OnRecipeCompleted(int recipeIndex) 
    {
        if (recipeIndex >= 0 && recipeIndex < _orderItems.Count)
        {
            _orderItems[recipeIndex].RecipeComplete(true);
        }
    }

    private void OnOrderCompleted(CustomerSO curCustomer, float percent)
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

    private void OnOrderTimeout(CustomerSO curCustomer)
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

    private void StartEmojiOnly(float percent)
    {
        if (emojiCo != null) // 혹시 코루틴 도는동안 다음 재료가 들어올 수도 있으니 코루틴 중복 호출 방지
        {
            StopCoroutine(emojiCo);
            emojiCo = null;
        }
        emojiCo = StartCoroutine(EmojiRoutine(percent));
    }

    private void StartEmojiAndDialogue(float percent, CustomerSO curCustomer)
    {
        StopRunningCoroutines(); //돌고있는 코루틴있으면 정리
        pairCo = StartCoroutine(PairRoutine(curCustomer, percent));
    }


    private void StopRunningCoroutines()
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

    private IEnumerator PairRoutine(CustomerSO curCustomer, float percent)
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

    private IEnumerator EmojiRoutine(float percent)
    {
        _emojiImage.sprite = GetEmojiSprite(percent);
        _emojiBox.gameObject.SetActive(true);

        yield return new WaitForSeconds(_dialogueDuration);
        _emojiBox.gameObject.SetActive(false);
    }


    private Sprite GetEmojiSprite(float percent)
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

    private void ClearOrderUI() //ui프리팹 파괴. 자식부터 지움
    {
        for (int i = orderItemParent.childCount - 1; i >= 0; i--)
        {
            Destroy(orderItemParent.GetChild(i).gameObject);
        }
        _orderItems.Clear();
    }
}
