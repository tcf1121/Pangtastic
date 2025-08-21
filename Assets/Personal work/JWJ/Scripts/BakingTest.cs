using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BakingTest : MonoBehaviour
{
    [SerializeField] private Button strawberry;
    [SerializeField] private Button kiwi;
    [SerializeField] private Button blueberry;
    [SerializeField] private Button coffeeBean;
    [SerializeField] private Button milk;
    [SerializeField] private Button grape;
    [SerializeField] private Button lemon;

    [SerializeField] private CustomerOrder order;
    [SerializeField] private OrderStateController orderNew;

    private IngredientSO ingredient;

    [Header("각 버튼에 연결될 재료 SO")]
    [SerializeField] private IngredientSO strawberrySO;
    [SerializeField] private IngredientSO kiwiSO;
    [SerializeField] private IngredientSO blueberrySO;
    [SerializeField] private IngredientSO coffeeBeanSO; 
    [SerializeField] private IngredientSO milkSO;
    [SerializeField] private IngredientSO grapeSO;
    [SerializeField] private IngredientSO lemonSO;


    private void Awake()
    {
        strawberry.onClick.AddListener(AddStrawberry);
        kiwi.onClick.AddListener(AddKiwi);
        blueberry.onClick.AddListener(AddBlueberry);
        coffeeBean.onClick.AddListener(AddCoffeeBean);
        milk.onClick.AddListener(AddMilk);
        grape.onClick.AddListener(AddGrape);
        lemon.onClick.AddListener(AddLemon);
    }

    private void AddStrawberry()
    {
        //order.AddIngredient(strawberrySO);
        orderNew.AddIngredient(strawberrySO);
    }

    private void AddKiwi()
    {
        //order.AddIngredient(kiwiSO);
        orderNew.AddIngredient(kiwiSO);
    }

    private void AddBlueberry()
    {
        //order.AddIngredient(blueberrySO);
   
        orderNew.AddIngredient(blueberrySO);
    }

    private void AddCoffeeBean()
    {
        //order.AddIngredient(coffeeBeanSO);
        orderNew.AddIngredient(coffeeBeanSO);
    }

    private void AddMilk()
    {
        //order.AddIngredient(milkSO);
        orderNew.AddIngredient(milkSO);
    }

    private void AddGrape()
    {
        //order.AddIngredient(grapeSO);
        orderNew.AddIngredient(grapeSO);
    }

    private void AddLemon()
    {
        //order.AddIngredient(lemonSO);
        orderNew.AddIngredient(lemonSO);
    }
}
