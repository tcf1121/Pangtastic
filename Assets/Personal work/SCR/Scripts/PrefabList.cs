using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    [CreateAssetMenu(fileName = "PrefabList", menuName = "Match/Block/PrefabList")]
    public class PrefabList : ScriptableObject
    {
        public List<GameObject> GemPrefab;
    }
}

