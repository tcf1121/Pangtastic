using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LHJ
{
    public enum BlockNum : int
    {
        // 재료 (매치 가능한 기본 블럭)
        Carrot,
        Lemon,
        Grape,
        Strawberry,
        Apple,
        Cabbage,

        // 특수 블럭 (매치 결과 생성)
        Horizontal,
        Vertical,
        Bomb,
        Honey,
        Milk,
        Coffee,

        // 장애물 (매치 불가 또는 조건부 제거)
        Ice,
        Egg,
        GiftBox,
        Syrup,
        Box,

        None
    }
}
