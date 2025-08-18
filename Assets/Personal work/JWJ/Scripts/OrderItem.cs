using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderItem : MonoBehaviour
{
    [SerializeField] private Sprite menuImage;
    [SerializeField] private Sprite ingredient1Image;
    [SerializeField] private Sprite ingredient2Image;

    [SerializeField] private TMP_Text ingredient1Amount;
    [SerializeField] private TMP_Text ingredient2Amount;

    public void SetOrderUI(Sprite foodImage, Sprite ing1Image, Sprite ing2Image, TMP_Text ing1Amount, TMP_Text ing2Amount)
    {
        menuImage = foodImage;
        ingredient1Image = ing1Image;
        ingredient2Image = ing2Image;

        ingredient1Amount = ing1Amount;
        ingredient2Amount = ing2Amount;
    }
}
