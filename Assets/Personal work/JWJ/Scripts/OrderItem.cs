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

    public void BuildRows(Dictionary<IngredientSO, int> required) //필요 재료 리스트 만들기
    {
        _rows.Clear();

        for (int i = 0; i < ingredientSlots.Count; i++) //다 꺼줌
        {
            ingredientSlots[i].gameObject.SetActive(false);
        }

        int slotIndex = 0;

        foreach (KeyValuePair<IngredientSO, int> kv in required) // 필요재료 수량 순회
        {
            IngredientItem slot = ingredientSlots[slotIndex]; //할당
            slot.gameObject.SetActive(true); // 활성화
            slot.Init(kv.Key.IngredientPic, 0, kv.Value); // 이미지, 수량 세팅
            _rows[kv.Key] = slot; //딕셔너리에 저장
            slotIndex++;
        }

        RecipeComplete(false); // 클리어 마크 리셋
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
