using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientItem : MonoBehaviour
{
    [SerializeField] private Image ingImage;
    [SerializeField] private TMP_Text amountText;

    public void Init(Sprite ingSprite, int have, int need)
    {
        ingImage.sprite = ingSprite;
        amountText.text = $"x {need.ToString()}";
    }

    public void SetAmount(int have, int need)
    {
        amountText.text = $"x {(need - have).ToString()}";
        //Debug.Log($"총: {need}, 현재: {have}");

        if (need - have <= 0 )
        {
            IngredientComplete();
        }
    }

    private void IngredientComplete()
    {
        //Debug.Log("재료 완료");
        amountText.text = "CLEAR";
    }

    public void RecipeComplete()
    {

    }
}
