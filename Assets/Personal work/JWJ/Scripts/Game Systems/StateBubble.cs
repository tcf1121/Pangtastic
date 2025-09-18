using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateBubble : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private ResidentManager _manager;

    private ResidentState _curState;

    private void Awake()
    {
        if(_manager == null)
        {
            _manager = FindObjectOfType<ResidentManager>();
        }
    }

    public void SetSprite(ResidentState state)
    {
        if (image == null)
        {
            Debug.LogError("이미지없음");
        }
        if (_manager == null)
        {
            Debug.LogError("매니져없음");
        }
        image.sprite = _manager.GetSpriteByState(state);
        _curState = state;
    }
}

