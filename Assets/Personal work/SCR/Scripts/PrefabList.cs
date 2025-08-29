using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    [CreateAssetMenu(fileName = "PrefabList", menuName = "Match/Block/PrefabList")]
    public class PrefabList : ScriptableObject
    {
        public List<GemData> GemDatas;

        public GameObject GetPrefab(GemType type)
        {
            return GemDatas[(int)type].GemPrefab;
        }

        public IngredientSO GetSO(GemType type)
        {
            return GemDatas[(int)type].ingredientSO;
        }
    }

    [Serializable]
    public class GemData
    {
        public GameObject GemPrefab;
        public IngredientSO ingredientSO;
    }
}

