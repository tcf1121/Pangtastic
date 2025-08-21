using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StageSetting : MonoBehaviour
{
    public RecipeSO CurRecipe;
    public StageSO CurStage;
    public int TotalCustomerCount { get; private set; }
    public int StageClearCustomerCount { get; private set; }

    private Dictionary<IngredientSO, int> _finalRecipe;

    public void SetStage(StageSO stage) //스테이지 세팅 변경
    {
        CurStage = stage;
        TotalCustomerCount = CurStage.CustomerList.Length;
        StageClearCustomerCount = CurStage.StageClearCustomerCount;
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
                //_finalRecipe[_adj.Ingredient] += _adj.ExtraAmount;//새로운 스크립트에 더하기에서 곱하기로 바꿔서 주석처리. 더이상 안씀
            }
        } 
    }

    public Dictionary<IngredientSO, int> GetRequiredIngs() //재료 증감치 적용된 레시피 딕셔너리 반환
    {
        return _finalRecipe;
    }
}
