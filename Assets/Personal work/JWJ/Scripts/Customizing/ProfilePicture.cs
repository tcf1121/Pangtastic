using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfilePicture : MonoBehaviour
{

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
    [SerializeField] private GameObject[] _accessories;


    private void Awake()
    {
        LoadCustomize();
    }
    public void ProfileSkin(int index)
    {
        if (index < 0 || index >= _skins.Length)
        {
            index = 0;
        }
        Material[] materials = _skinRenderer.materials;
        materials[0] = _skins[index];
        _skinRenderer.materials = materials;
    }

    public void ProfileFace(int index)
    {
        if (index < 0 || index >= _faces.Length)
        {
            index = 0;
        }

        Material[] materials = _skinRenderer.materials;
        materials[1] = _faces[index];
        _skinRenderer.materials = materials;
    }

    public void ProfileHeadwear(int index)
    {
        if (index < 0 || index >= _headwears.Length)
        {
            for (int i = 0; i < _headwears.Length; i++)
            {
                GameObject go = _headwears[i];
                go.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < _headwears.Length; i++)
            {
                GameObject go = _headwears[i];
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

    public void ProfileFacewear(int index)
    {
        if (index < 0 || index >= _facewears.Length)
        {
            for (int i = 0; i < _facewears.Length; i++)
            {
                GameObject go = _facewears[i];
                go.SetActive(false);
            }
        }
        else
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


    }

    public void LoadCustomize()
    {
        string customizeData = Manager.User.GetCustomize();
        string[] splitData = customizeData.Split(',');
        if (splitData.Length < 7)
        {
            ProfileSkin(0);
            ProfileFace(0);
            ProfileHeadwear(-1);

            return;
        }
        else
        {
            int[] saveData = new int[7];

            for (int i = 0; i < 7; i++)
            {
                bool ok = int.TryParse(splitData[i], out int index);

                if (ok)
                {
                    saveData[i] = index;
                }
                else
                {
                    Debug.LogError("커스터마이즈 데이터 변환 실패");
                    saveData[i] = -1;
                }
            }
            ProfileSkin(saveData[0]);
            ProfileFace(saveData[1]);
            ProfileHeadwear(saveData[2]);
        }
    }
}
