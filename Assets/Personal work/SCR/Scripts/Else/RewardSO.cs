using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RewardSO", menuName = "PangTasticSO/RewardSO")]
public class RewardSO : ScriptableObject
{
    public int RewardID;
    public string RewardExplane;
    public List<Rewards> Rewards;

    public void AddReward()
    {
        foreach (var s in Rewards)
        {
            if (s.GoodsType == Goods.InfinityHeart)
            {
                OutGameManager.AddIHReward((float)s.Count / 60);
            }
            else
                OutGameManager.AddReward(s.GoodsType, s.Count);
        }
    }
}

[Serializable]
public class Rewards
{
    public Goods GoodsType;
    public int Count;
}

