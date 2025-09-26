using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    [SerializeField] List<SpeakerUI> _speakers;
    [SerializeField] Color _speakerColor;
    [SerializeField] Color _elseColor;
    [SerializeField] List<GameObject> _nonNameLabel;
    [SerializeField] TMP_Text _dialog;
    //[SerializeField] StringSO dialog;

    void Start()
    {
        //SetDialog(Speaker.meow, null, Speaker.Fifi, 2, dialog);
    }

    public void SetDialog(Speaker? left, Speaker? center, Speaker? right, int speakerIndex, StringSO dialog)
    {

        _speakers[0].SetCharacter(left);
        _speakers[1].SetCharacter(center);
        _speakers[2].SetCharacter(right);


        for (int i = 0; i < 3; i++)
        {
            if (i == speakerIndex)
            {
                _speakers[i].SetSpeaker(_speakerColor);
                _nonNameLabel[i].SetActive(false);
            }
            else
            {
                _speakers[i].SetSpeaker(_elseColor);
                _nonNameLabel[i].SetActive(true);
            }
        }
        _dialog.text = dialog.GetText(Manager.Language.GetLanguage());
    }
}
