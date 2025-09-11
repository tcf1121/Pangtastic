using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterImageMake : MonoBehaviour
{
    [SerializeField] List<GameObject> _leftCharacters;
    [SerializeField] List<GameObject> _centerCharacters;
    [SerializeField] List<GameObject> _rightCharacters;
    [SerializeField] List<RawImage> _rawImages;
    [SerializeField] Color _speakerColor;
    [SerializeField] Color _elseColor;
    [SerializeField] List<GameObject> _nameLabel;
    [SerializeField] List<TMP_Text> _charactersName;
    [SerializeField] TMP_Text _dialog;

    void Awake()
    {
        SetCharacter("Coco", null, "Fifi", 0);
        SetDialog("안녕안녕");
    }


    public void SetCharacter(string leftChar, string centerChar, string rightchar, int speakerIndex)
    {
        SetNameLabel(0, leftChar);
        SetNameLabel(1, centerChar);
        SetNameLabel(2, rightchar);
        SetCharacter(0, StringToCharint(leftChar));
        SetCharacter(1, StringToCharint(centerChar));
        SetCharacter(2, StringToCharint(rightchar));
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
    }

    public void SetDialog(string dialog)
    {
        _dialog.text = dialog;
    }

    private void SetNameLabel(int index, string name)
    {
        _charactersName[index].text = name;
        if (name == null) _nameLabel[index].SetActive(false);
        else _nameLabel[index].SetActive(true);
    }


    private void SetCharacter(int index, int? charindex)
    {
        if (charindex == null)
        {
            charindex = _centerCharacters.Count;
        }
        if (index == 0)
        {
            for (int i = 0; i < _leftCharacters.Count; i++)
            {
                if (i == charindex) _leftCharacters[i].SetActive(true);
                else _leftCharacters[i].SetActive(false);
            }
        }
        else if (index == 1)
        {
            for (int i = 0; i < _centerCharacters.Count; i++)
            {
                if (i == charindex) _centerCharacters[i].SetActive(true);
                else _centerCharacters[i].SetActive(false);
            }
        }
        else if (index == 2)
        {
            for (int i = 0; i < _rightCharacters.Count; i++)
            {
                if (i == charindex) _rightCharacters[i].SetActive(true);
                else _rightCharacters[i].SetActive(false);
            }
        }
    }

    private int? StringToCharint(string Charname)
    {
        if (Charname == "Coco")
        {
            return 0;
        }
        else if (Charname == "Kuku")
        {
            return 1;
        }
        else if (Charname == "Fifi")
        {
            return 2;
        }
        else if (Charname == "Meow")
        {
            return 3;
        }
        else if (Charname == "Lulu")
        {
            return 4;
        }
        else return null;
    }
}
