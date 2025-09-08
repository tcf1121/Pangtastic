using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

[CreateAssetMenu(fileName = "New Catalog", menuName = "PangTasticSO/Catalog")]
public class ProductCatalogSO : ScriptableObject
{
    [System.Serializable]
    public class RewardEntry
    {
        public RewardType RewardType;
        public int RewardAmount;
    }

    [Serializable]
    public class Entry
    {
        public string ProductId;
        public ProductType ProductType;
        public List<RewardEntry> Rewards;
    }

    [SerializeField] private List<Entry> _entries = new List<Entry>();

    public List<ProductDefinition> GetProductDefinitions()
    {
        List<ProductDefinition> infos = new List<ProductDefinition>();

        for (int i = 0; i < _entries.Count; i++)
        {
            Entry entry = _entries[i];
            if (entry == null)
            {
                Debug.LogWarning($"{entry.ProductId}가 null임");
                continue;
            }

            if (string.IsNullOrEmpty(entry.ProductId) == true)
            {
                Debug.LogWarning($"{entry.ProductId}가 없음");
                continue;
            }

            ProductDefinition info = new ProductDefinition(entry.ProductId, entry.ProductType);
            infos.Add(info);
        }
        return infos;
    }

    public Entry GetEntryById(string productId)
    {
        return _entries.Find(entry => entry.ProductId == productId);
    }
}
