using LHJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusGemList : MonoBehaviour
{
    [SerializeField] private BonusGem[] _gems;

    public GameObject Spawn(GemType type, Vector3 worldPos, Transform parent)
    {
        GameObject prefab = FindPrefab(type);
        if (prefab == null) return null;
        return Instantiate(prefab, worldPos, Quaternion.identity, parent);
    }

    private GameObject FindPrefab(GemType type)
    {
        for (int i = 0; i < _gems.Length; i++)
        {
            BonusGem item = _gems[i];
            if (item == null) continue;
            if (item.bonusGemType == type) return item.gemPrefabs;
        }
        return null;
    }
}
