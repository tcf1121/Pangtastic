using SCR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    //public enum BonusGemType
    //{
    //    Horizontal,
    //    Vertical,
    //    Bomb,
    //    Honey,
    //    Milk,
    //    Coffee
    //}
    [CreateAssetMenu(fileName = "New Bonus Gem", menuName = "Gem/Bonus Gem")]
    public class BonusGem : ScriptableObject
    {
        public GemType bonusGemType;

        [Header("Prefab")]
        public GameObject gemPrefabs;
    }
}
