using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterImageMake : MonoBehaviour
{
    [SerializeField] SpeakerUI _leftSpeaker;
    [SerializeField] SpeakerUI _centerSpeaker;
    [SerializeField] SpeakerUI _rightSpeaker;
    [SerializeField] List<RawImage> _rawImages;
    [SerializeField] Color _speakerColor;
    [SerializeField] Color _elseColor;
    [SerializeField] List<GameObject> _nameLabel;
    [SerializeField] TMP_Text _dialog;

    public void SetCharacter(Speaker? left, Speaker? center, Speaker? right, int speakerIndex, StringSO dialog)
    {

        _leftSpeaker.SetSpeaker(left);
        _centerSpeaker.SetSpeaker(center);
        _rightSpeaker.SetSpeaker(right);


        for (int i = 0; i < _rawImages.Count; i++)
        {
            if (i == speakerIndex)
            {
                _rawImages[i].color = _speakerColor;
                _nameLabel[i].transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                _rawImages[i].color = _elseColor;
                _nameLabel[i].transform.GetChild(2).gameObject.SetActive(true);
            }
        }
        SetDialog(dialog.GetText(Manager.Language.GetLanguage()));
    }

    public void SetDialog(string dialog)
    {
        _dialog.text = dialog;
    }
}
