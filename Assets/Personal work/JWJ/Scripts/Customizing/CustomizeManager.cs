using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CustomizeManager : MonoBehaviour
{
    [SerializeField] private GameObject _character;
    [SerializeField] private Renderer _skinRenderer;

    [Header("스킨")]
    [SerializeField] private Material[] _skins;

    [Header("얼굴")]
    [SerializeField] private Material[] _faces;

    [Header("머리")]
    [SerializeField] private GameObject[] _headwears;

    [Header("얼굴장식")]
    [SerializeField] private GameObject[] _facewears;

    [Header("가방")]
    [SerializeField] private GameObject[] _bags;

    [Header("악세서리")]
    [SerializeField] private GameObject[] _accessorys;

    public void ChangeSkin(int index)
    {
        _skinRenderer.material = _skins[index];
    }

    public void ChangeFace(int index)
    {
        _skinRenderer.material = _faces[index];
    }

    public void ChangeHeadwear(int index)
    {
        for (int i = 0; i < _headwears.Length; i++)
        {
            GameObject go = _headwears[i];
            if(i == index)
            {
                go.SetActive(true);
            }
            else
            {
                go.SetActive(false);
            }
        }
    }

    public void ChangeFacewear(int index)
    {
        for (int i = 0; i < _facewears.Length; i++)
        {
            GameObject go = _facewears[i];
            if (i == index)
            {
                go.SetActive(true);
            }
            else
            {
                go.SetActive(false);
            }
        }
    }

    public void ChangeBag(int index)
    {
        for (int i = 0; i < _bags.Length; i++)
        {
            GameObject go = _bags[i];
            if (i == index)
            {
                go.SetActive(true);
            }
            else
            {
                go.SetActive(false);
            }
        }
    }

    public void ChangeAccessory(int index)
    {
        for (int i = 0; i < _accessorys.Length; i++)
        {
            GameObject go = _accessorys[i];
            if (i == index)
            {
                go.SetActive(true);
            }
            else
            {
                go.SetActive(false);
            }
        }
    }

}
