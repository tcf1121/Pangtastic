using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemPrefab : MonoBehaviour
{
    SpriteRenderer _sprite;

    void Awake()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite)
    {
        _sprite.sprite = sprite;
    }

    public void HighLight(bool isSelect)
    {
        if (isSelect) _sprite.material.SetFloat("_OnOutline", 1);
        else _sprite.material.SetFloat("_OnOutline", 0);
    }
}
