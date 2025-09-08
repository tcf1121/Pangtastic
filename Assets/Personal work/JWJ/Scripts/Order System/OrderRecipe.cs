using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderRecipe
{
    private List<IngredientSO> ingredients = new List<IngredientSO>(); // 재료 목록
    private List<int> required = new List<int>(); // 필요 수량
    private List<int> collected = new List<int>(); // 수집 수량

    public bool IsCompleted { get; private set; }

    //리스트 값들 읽기전용
    public int IngredientCount => ingredients.Count; 
    public IngredientSO GetIngredient(int index) => ingredients[index];
    public int GetRequiredAmount(int index) => required[index];
    public int GetCollectedAmount(int index) => collected[index];

    public OrderRecipe(RecipeSO recipe, StageSO stage) //재료 초기화하는 생성자
    {
        var final = RecipeRule.ApplyMultipliers(recipe, stage); //스테이지 재료 보정 적용

        foreach (var kv in final) //스테이지 보정된 재료정보 리스트에 넣어줌
        {
            ingredients.Add(kv.Key);
            required.Add(kv.Value);
            collected.Add(0); //처음에는 모인재료 없으니 0
        }
        IsCompleted = false;
    }

    public bool CollectIngredient(IngredientSO ingredient, out int have, out int need)
    {
        have = 0;
        need = 0;

        if (IsCompleted)
        {
            return false;
        }

        for (int i = 0; i < ingredients.Count; i++)
        {
            if (ingredients[i] == ingredient && collected[i] < required[i]) //레시피에 들어온 재료가 필요하고 더 필요할때
            {
                collected[i]++;
                have = collected[i];
                need = required[i];

                if (CheckCompleted()) //레시피가 완료되면
                {
                    IsCompleted = true;
                }
                return true;
            }
        }
        return false;
    }

    private bool CheckCompleted() //레시피 완료 체크
    {
        for (int i = 0; i < ingredients.Count; i++)
        {
            if (collected[i] < required[i]) //부족한 재료가 있으면
            {
                return false;
            }
        }
        return true;
    }


}
