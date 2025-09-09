using System.Collections;
using System.Collections.Generic;
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

    void Awake()
    {
        SetDialog("Coco", null, "Fifi", 0);
    }


    public void SetDialog(string leftChar, string centerChar, string rightchar, int speakerIndex)
    {
        SetCharacter(0, StringToCharint(leftChar));
        SetCharacter(1, StringToCharint(centerChar));
        SetCharacter(2, StringToCharint(rightchar));
        for (int i = 0; i < _rawImages.Count; i++)
        {
            if (i == speakerIndex) _rawImages[i].color = _speakerColor;
            else _rawImages[i].color = _elseColor;
        }
    }




    private void SetCharacter(int index, int? charindex)
    {
        if (charindex == null) charindex = _centerCharacters.Count;
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
