using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateBubble : MonoBehaviour
{
    [SerializeField] private GameObject _iconBox;
    [SerializeField] private Image _icon;

    [SerializeField] private GameObject _chatBox;
    [SerializeField] private TMP_Text _text;

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
        if (_icon == null)
        {
            Debug.LogError("이미지없음");
        }
        if (_manager == null)
        {
            Debug.LogError("매니져없음");
        }
        _icon.sprite = _manager.GetSpriteByState(state);
        _curState = state;
    }

    public void IconUI(bool isOn)
    {
        _iconBox.SetActive(isOn);
    }

    public void ChatUI(ResidentSO resident, bool isOn)
    {
        if (isOn == false)
        {
            _chatBox.SetActive(false);
        }
        else
        {
            int rand = Random.Range(0, resident.DialogueTouched.Length);
            StringSO stringSO = resident.DialogueTouched[rand];

            if (stringSO == null)
            {
                _text.text = "Hello!!";
            }
            else
            {
                _text.text = stringSO.GetText(Manager.Language.GetLanguage());
            }
            _chatBox.SetActive(true);
        }
    }
}

