using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StageSetting : MonoBehaviour
{//���������� ���� ��ᷮ ������ üũ�ϴ� ��ũ��Ʈ
    public RecipeSO CurRecipe;
    public StageSO CurStage;

    private Dictionary<IngredientSO, int> _finalRecipe;

    public void SetStage(StageSO stage) //�������� ���� ����
    {
        CurStage = stage;
    }

    public void InitStageRecipe() //�����ǿ� �������� ��� ����ġ ����
    {
        _finalRecipe = new Dictionary<IngredientSO, int>();

        foreach (var _req in CurRecipe.Ingredients) //��ųʸ��� ���� ������ ����
        {
            _finalRecipe[_req.Ingredient] = _req.Amount;
        }
            
        foreach (var _adj in CurStage.IngredientAdjustments) //�߰��� ��Ḧ ���ؼ� ��ųʸ� ������Ʈ
        {
            if (_finalRecipe.ContainsKey(_adj.Ingredient))
            {
                _finalRecipe[_adj.Ingredient] += _adj.ExtraAmount;
            }
        } 
    }

    public Dictionary<IngredientSO, int> GetRequiredIngs() //��� ����ġ ����� ������ ��ųʸ� ��ȯ
    {
        return _finalRecipe;
    }
}
