using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    [CreateAssetMenu(fileName = "NewNomalGem", menuName = "Gem/Nomal Gem")]
    public class NomalGem : ScriptableObject
    {
        public BlockNum nomalGemType;

        [Header("Prefab")]
        public GameObject gemPrefabs;
    }
}
