using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderItem : MonoBehaviour
{
    [SerializeField] private Image menuImage;
    [SerializeField] private Transform ingredientParent;
    [SerializeField] private GameObject ingredientPrefab;
    [SerializeField] private GameObject clearImage;

    private Dictionary<IngredientSO, IngredientItem> _rows = new Dictionary<IngredientSO, IngredientItem>();

    public void SetMenu(Sprite icon)
    {
        menuImage.sprite = icon;
        //메뉴 이름 추가할거면 여기에 추가
    }

    public void BuildRows(Dictionary<IngredientSO, int> required )
    {
        foreach(KeyValuePair<IngredientSO, int> kv in required)
        {
            GameObject go = GameObject.Instantiate(ingredientPrefab, ingredientParent);
            IngredientItem row = go.GetComponent<IngredientItem>();
            row.Init(kv.Key.IngredientPic, 0, kv.Value);
            _rows[kv.Key] = row;
        }
    }

    public void UpdateRow(IngredientSO ing, int have, int need)
    {
        IngredientItem row;
        if(_rows.TryGetValue(ing, out row) == true)
        {
            row.SetAmount(have, need);
        }
    }

    public void RecipeComplete(bool active)
    {
        clearImage.SetActive(active);
    }
}
