using SCR_B;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemPrefab : MonoBehaviour
{
    SpriteRenderer _sprite;
    Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite)
    {
        _sprite.sprite = sprite;
    }

    public Sprite GetSprite()
    {
        if (_sprite == null) _sprite = GetComponentInChildren<SpriteRenderer>();
        return _sprite.sprite;
    }

    public void HighLight(bool isSelect)
    {
        if (isSelect) _sprite.material.SetFloat("_OnOutline", 1);
        else _sprite.material.SetFloat("_OnOutline", 0);
    }

    public void Broken()
    {
        _animator.SetTrigger("Broken");
    }

    public void PoolBack()
    {
        ObjectPool.ReturnPool(this);
    }
}
