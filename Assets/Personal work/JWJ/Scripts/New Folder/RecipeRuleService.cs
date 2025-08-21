using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RecipeRuleService
{
    public static List<RecipeSO> BuildOrder(CustomerSO customer, StageSO stage)
    {
        List<RecipeSO> stagePool = new List<RecipeSO>(stage.StageRecipes);
        List<RecipeSO> result = new List<RecipeSO>();

        // 주문 메뉴 수 설정
        int orderCount; 
        if(customer.Type == CustomerType.Special) //스페셜 손님일때
        {
            orderCount = Random.Range(3, 6); // 3~5개
        }
        else //노멀이나 유니크일때
        {
            orderCount= Random.Range(stage.MinOrderCount, 5); // 최대 4개
        }


        //주문 목록 설정
        if(customer.Type == CustomerType.Normal)
        {
            List<RecipeSO> copy = new List<RecipeSO>(stagePool);

            while (result.Count < orderCount && copy.Count > 0) //스테이지 메뉴에서 랜덤선택. 중복없음
            {
                int rand = Random.Range(0, copy.Count);
                result.Add(copy[rand]);
                copy.RemoveAt(rand);
            }
        }

        else if (customer.Type == CustomerType.Unique)
        {
            foreach (var must in customer.FavoriteRecipes)
            {
                if (stagePool.Contains(must) && orderCount > result.Count)
                {
                    result.Add(must); // 필수메뉴 리스트에 넣음
                }
            }

            List<RecipeSO> copy = new List<RecipeSO>(stagePool);

            foreach (var must in customer.FavoriteRecipes)
            {
                copy.Remove(must); //중복 방지를 위해 필수메뉴 스테이지 메뉴에서 지워줌
            }

            while (result.Count < orderCount && copy.Count > 0) //필수메뉴 넣고 남은 주문은 스테이지 레시피에서 랜덤으로 채움
            {
                int rand = Random.Range(0, copy.Count);
                result.Add(copy[rand]);
                copy.RemoveAt(rand);
            }
        }
        else if (customer.Type == CustomerType.Special) // 스페셜
        {
            foreach (var fav in customer.FavoriteRecipes) // 선호메뉴 순회
            {
                if (stagePool.Contains(fav)) // 스테이지에 선호메뉴 있으면
                {
                    result.Add(fav); // 주문 리스트에 추가
                }
            }

            int need = orderCount - result.Count; // 부족한 주문수 

            if (need > 0) // 주문 더 해야하면
            {
                List<RecipeSO> favPool = new List<RecipeSO>(); //선호 메뉴 리스트 생성

                foreach (var fav in customer.FavoriteRecipes)
                {
                    if (stagePool.Contains(fav))
                    {
                        favPool.Add(fav); // 선호 메뉴 리스트에 넣어줌
                    }
                }

                while (need > 0 && favPool.Count > 0) // 추가분은 중복없이 추가
                {
                    int rand = Random.Range(0, favPool.Count);
                    result.Add(favPool[rand]);
                    favPool.RemoveAt(rand);
                    need--;
                }
            }
        }
        return result;
    }

    public static Dictionary<IngredientSO, int> ApplyMultipliers(RecipeSO recipe, StageSO stage)
    {
        Dictionary<IngredientSO, int> result = new Dictionary<IngredientSO, int>();

        foreach (var ing in recipe.Ingredients) 
        {
            int baseAmount = ing.Amount; 
            float multiplier = 1f;

            foreach (var adj in stage.IngredientAdjustments) //스테이지의 재료 증감 배열 순회 
            {
                if (adj.Ingredient == ing.Ingredient) //주문 레시피의 재료가있으면
                {
                    multiplier = adj.MuliflyBy; //배수
                    break;
                }
            }

            int finalAmount = Mathf.FloorToInt(baseAmount * multiplier); //곱한 후 수량 int로 바꿔줌
            
            result[ing.Ingredient] = finalAmount; //배수 적용해서 주문에 적용
        }
        return result;
    }
}
