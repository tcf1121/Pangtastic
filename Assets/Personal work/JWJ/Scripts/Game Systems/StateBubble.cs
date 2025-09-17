using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateBubble : MonoBehaviour
{
    [SerializeField] private Image image;
    private ResidentManager _manager;

    private ResidentState _curState;

    private void Awake()
    {
        _manager = FindObjectOfType<ResidentManager>();
        if (image == null)
        {
            image = GetComponent<Image>();
        }
    }

    public void SetSprite(ResidentState state)
    {
        image.sprite = _manager.GetSpriteByState(state);
        _curState = state;
    }
}

