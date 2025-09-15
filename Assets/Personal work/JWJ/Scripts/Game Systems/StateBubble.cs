using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateBubble : MonoBehaviour
{
    [SerializeField] private Image image;

    [SerializeField] private Sprite _greet;
    [SerializeField] private Sprite _workout;
    [SerializeField] private Sprite _lookAround;
    [SerializeField] private Sprite _talking;

    public void SetSprite(ResidentState state)
    {
        switch (state)
        {
            case ResidentState.Idle:
                image.sprite = _greet;
                break;

            case ResidentState.Move:
                image.sprite = _greet;
                break;

            case ResidentState.Interact:
                image.sprite = _lookAround;
                break;

            case ResidentState.Greet:
                image.sprite = _greet;
                break;

            case ResidentState.Talk:
                image.sprite = _talking;
                break;

            case ResidentState.Workout:
                image.sprite = _workout;
                break;

            case ResidentState.Touched:
                image.sprite = _greet;
                break;
        }
    }
}

