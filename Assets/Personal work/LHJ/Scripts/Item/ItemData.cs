using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    public enum ItemType
    {
        Whisk,    // 3x3
        Scissor  // 가로+세로
    }

    [CreateAssetMenu(fileName = "ItemData", menuName = "Puzzle/ItemData")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public Sprite icon;
        public ItemType itemType;

        [Header("개수 관리")]
        public int Count = 3; // 개수 
        [HideInInspector] public int currentCount; // 런타임에 사용

        public void ResetCount()
        {
            currentCount = Count;
        }

        public bool UseOne()
        {
            if (currentCount <= 0) return false;
            currentCount--;
            return true;
        }
    }
}
