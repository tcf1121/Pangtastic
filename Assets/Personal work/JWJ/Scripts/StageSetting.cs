using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StageSetting : MonoBehaviour
{//스테이지에 따라 재료량 변경을 체크하는 스크립트
    public RecipeSO CurRecipe;
    public StageSO CurStage;

    private Dictionary<IngredientSO, int> _finalRecipe;

    public void SetStage(StageSO stage) //스테이지 세팅 변경
    {
        CurStage = stage;
    }

    public void InitStageRecipe() //레시피에 스테이지 재료 증감치 적용
    {
        _finalRecipe = new Dictionary<IngredientSO, int>();

        foreach (var _req in CurRecipe.Ingredients) //딕셔너리에 원본 레시피 복사
        {
            _finalRecipe[_req.Ingredient] = _req.Amount;
        }
            
        foreach (var _adj in CurStage.IngredientAdjustments) //추가된 재료를 더해서 딕셔너리 업데이트
        {
            if (_finalRecipe.ContainsKey(_adj.Ingredient))
            {
                _finalRecipe[_adj.Ingredient] += _adj.ExtraAmount;
            }
        } 
    }

    public Dictionary<IngredientSO, int> GetRequiredIngs() //재료 증감치 적용된 레시피 딕셔너리 반환
    {
        return _finalRecipe;
    }
}
