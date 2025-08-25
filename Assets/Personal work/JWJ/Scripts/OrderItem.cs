using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderItem : MonoBehaviour
{
    [SerializeField] private Image menuImage;
    [SerializeField] private GameObject clearImage;
    [SerializeField] private List<IngredientItem> ingredientSlots = new List<IngredientItem>();

    private Dictionary<IngredientSO, IngredientItem> _rows = new Dictionary<IngredientSO, IngredientItem>();

    public void SetMenu(Sprite icon)
    {
        menuImage.sprite = icon;
        //메뉴 이름 추가할거면 여기에 추가
    }

    public void BuildRows(OrderRecipe orderRecipe) //필요 재료 리스트 만들기
    {
        _rows.Clear();

        for (int i = 0; i < ingredientSlots.Count; i++) //다 꺼줌
        {
            ingredientSlots[i].gameObject.SetActive(false);
        }

        int slotIndex = 0;
        int ingredientCount = orderRecipe.IngredientCount;

        for (int j = 0; j < ingredientCount; j++) // 레시피의 재료 개수만큼 반복
        {
            if (slotIndex >= ingredientSlots.Count) // 재료 수가 슬롯 수를 초과하면
            {
                Debug.LogError("재료 수가 슬롯 수를 초과함");
                break;
            }

            IngredientSO ing = orderRecipe.GetIngredient(j); //재료
            int need = orderRecipe.GetRequiredAmount(j); // 필요수량
            int have = orderRecipe.GetCollectedAmount(j); //모은 수량

            IngredientItem slot = ingredientSlots[slotIndex];
            slot.gameObject.SetActive(true);
            slot.Init(ing.IngredientPic, have, need);
            _rows[ing] = slot;
            slotIndex++;
        }

        RecipeComplete(false); // 레시피 완료 마크를 초기화(처음에는 미완료)
    }

    public void UpdateRow(IngredientSO ing, int have, int need) //필요 개수 업데이트
    {
        IngredientItem row;
        if(_rows.TryGetValue(ing, out row) == true)
        {
            row.SetAmount(have, need);
        }
    }

    public void RecipeComplete(bool active) //레시피 클리어 이미지
    {
        clearImage.SetActive(active);
    }

    public void ResetUI() // 재료 목록 초기화
    {
        _rows.Clear();
        for (int i = 0; i < ingredientSlots.Count; i++)
        {
            ingredientSlots[i].gameObject.SetActive(false);
        }
        RecipeComplete(false);
    }
}
