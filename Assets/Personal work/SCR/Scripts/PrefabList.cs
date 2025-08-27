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
    }

    [Serializable]
    public class GemData
    {
        public GameObject GemPrefab;
        public IngredientSO ingredientSO;
    }
}

