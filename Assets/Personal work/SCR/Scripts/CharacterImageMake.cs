using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterImageMake : MonoBehaviour
{
    [SerializeField] List<SpeakerUI> _speakers;
    [SerializeField] Color _speakerColor;
    [SerializeField] Color _elseColor;
    [SerializeField] List<GameObject> _nameLabel;
    [SerializeField] TMP_Text _dialog;

    public void SetCharacter(Speaker? left, Speaker? center, Speaker? right, int speakerIndex, StringSO dialog)
    {

        _speakers[0].SetCharacter(left);
        _speakers[1].SetCharacter(center);
        _speakers[2].SetCharacter(right);


        for (int i = 0; i < 3; i++)
        {
            if (i == speakerIndex)
            {
                _speakers[i].SetSpeaker(_speakerColor);
            }
            else
            {
                _speakers[i].SetSpeaker(_elseColor);
            }
        }
        SetDialog(dialog.GetText(Manager.Language.GetLanguage()));
    }

    public void SetDialog(string dialog)
    {
        _dialog.text = dialog;
    }
}
