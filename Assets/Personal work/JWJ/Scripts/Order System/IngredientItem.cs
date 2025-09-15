using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientItem : MonoBehaviour
{
    [SerializeField] private Image ingImage;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private GameObject amountObject;
    [SerializeField] private GameObject clearObject;

    public void Init(Sprite ingSprite, int have, int need)
    {
        ingImage.sprite = ingSprite;
        amountText.text = $"{need.ToString()}";
        amountObject.SetActive(true);
        clearObject.SetActive(false);
    }

    public void SetAmount(int have, int need)
    {
        amountText.text = $"{(need - have).ToString()}";
        //Debug.Log($"총: {need}, 현재: {have}");

        if (need - have <= 0)
        {
            IngredientComplete();
        }
    }

    private void IngredientComplete()
    {
        amountObject.SetActive(false);
        clearObject.SetActive(true);
    }

    public void RecipeComplete()
    {

    }
}
