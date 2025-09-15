using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeakerUI : MonoBehaviour
{
    [SerializeField] List<GameObject> Speakers;
    [SerializeField] GameObject _nameLabel;
    [SerializeField] TMP_Text _charactersName;
    [SerializeField] SpeakerSO _speakerSO;


    public void SetSpeaker(Speaker? speaker)
    {
        SetNameLabel(speaker);
        if (speaker != null)
            for (int i = 0; i < Speakers.Count; i++)
            {
                if (i == (int)speaker) Speakers[i].SetActive(true);
                else Speakers[i].SetActive(false);
            }
        else
        {
            for (int i = 0; i < Speakers.Count; i++)
            {
                Speakers[i].SetActive(false);
            }
        }
    }

    private void SetNameLabel(Speaker? speaker)
    {

        if (speaker == null) _nameLabel.SetActive(false);
        else
        {
            _charactersName.text = _speakerSO.GetSpeaker((Speaker)speaker).
            GetText(Manager.Language.GetLanguage());
            _nameLabel.SetActive(true);
        }
    }


}

public enum Speaker
{
    Lulu,
    Toto,
    Bambi,
    Leo,
    Momo,
    Kiki,
    Uni,
    Milo,
    Ollie
}
