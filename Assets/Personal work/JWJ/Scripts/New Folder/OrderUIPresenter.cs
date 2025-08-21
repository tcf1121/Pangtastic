using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderUIPresenter : MonoBehaviour
{
    [SerializeField] private OrderStateController _orderState; // 주문 상태 컨트롤러
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

    private void Awake()
    {
        if (_orderState == null)
        {
            _orderState = FindObjectOfType<OrderStateController>();
        }

        // 이벤트 구독
        _orderState.OnOrderStarted += BuildOrderUI;
        _orderState.OnIngredientProgress += OnIngredientProgress;
        _orderState.OnRecipeCompleted += OnRecipeCompleted;
        _orderState.OnOrderCompleted += OnOrderCompleted;
        _orderState.OnOrderTimeout += OnOrderTimeout;   
    }

    private void OnDestroy()
    {
        _orderState.OnOrderStarted -= BuildOrderUI;
        _orderState.OnIngredientProgress -= OnIngredientProgress;
        _orderState.OnRecipeCompleted -= OnRecipeCompleted;
        _orderState.OnOrderCompleted -= OnOrderCompleted;
        _orderState.OnOrderTimeout -= OnOrderTimeout;
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

    private void OnOrderCompleted(CustomerSO curCustomer, float Percent)
    {
        if(Percent <= 50f)
        {
            _leftDialogue.text = curCustomer.HighDialogue; //나중에 번역 추가되면 수정해야할듯
            _emojiImage.sprite = _emojiHigh;
        }
        else
        {
            _leftDialogue.text = curCustomer.MidDialogue;
            _emojiImage.sprite = _emojiMid;
        }

        StartCoroutine(LeftDialogueRoutine());
    }

    private IEnumerator LeftDialogueRoutine()
    {
        _chatBox.gameObject.SetActive(true);
        _emojiBox.gameObject.SetActive(true);
        yield return new WaitForSeconds(_dialogueDuration);
        _chatBox.gameObject.SetActive(false);
        _emojiBox.gameObject.SetActive(false);
    }

    private void OnOrderTimeout(CustomerSO curCustomer)
    {
        _leftDialogue.text = curCustomer.LeftDialogue;
        _emojiImage.sprite = _emojiLow;
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
