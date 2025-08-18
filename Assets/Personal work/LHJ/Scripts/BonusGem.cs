using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    [CreateAssetMenu(fileName = "New Bonus Gem", menuName = "Gem/Bonus Gem")]
    public class BonusGem : ScriptableObject
    {
        public BonusGemType bonusGemType;

        [Header("Prefab")]
        public GameObject gemPrefabs;
    }
}
