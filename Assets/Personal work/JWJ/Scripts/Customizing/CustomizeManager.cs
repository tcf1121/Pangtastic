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
    [SerializeField] private GameObject[] _accessories;

    [SerializeField] private string _testSaveData;
    [SerializeField] private GameObject _uis;

    private int _curSkinIndex;
    private int _curFaceIndex;
    private int _curHeadwearIndex;
    private int _curFacewearIndex;
    private int _curBagIndex;
    private int _curAccessoriesIndex;

    private void Awake()
    {
        LoadCustomize();
    }
    private void ChangeSkin(int index)
    {
        if(index < 0 || index >= _skins.Length)
        {
            index = 0;
        }
        Material[] materials = _skinRenderer.materials;
        materials[0] = _skins[index];
        _skinRenderer.materials = materials;
        Debug.Log($"스킨 변경 : {_skins[index].name}");
        _curSkinIndex = index;
    }

    private void ChangeFace(int index)
    {
        if (index < 0 || index >= _faces.Length)
        {
            index = 0;
        }

        Material[] materials = _skinRenderer.materials;
        materials[1] = _faces[index];
        _skinRenderer.materials = materials;
        Debug.Log($"얼굴 변경 : {_faces[index].name}");
        _curFaceIndex = index;
    }

    private void ChangeHeadwear(int index)
    {
        if (index < 0 || index >= _headwears.Length)
        {
            for (int i = 0; i < _headwears.Length; i++)
            {
                GameObject go = _headwears[i];
                go.SetActive(false);
            }
            _curHeadwearIndex = -1;
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
            _curHeadwearIndex = index;
        }
    }

    private void ChangeFacewear(int index)
    {
        if (index < 0 || index >= _facewears.Length)
        {
            for (int i = 0; i < _facewears.Length; i++)
            {
                GameObject go = _facewears[i];
                go.SetActive(false);
            }
            _curFacewearIndex = -1;
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
            _curFacewearIndex = index;
        }


    }

    private void ChangeBag(int index)
    {
        if (index < 0 || index >= _bags.Length)
        {
            for (int i = 0; i < _bags.Length; i++)
            {
                GameObject go = _bags[i];
                go.SetActive(false);
            }
            _curBagIndex = -1;
        }
        else
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
            _curBagIndex = index;
        }


    }

    private void ChangeAccessory(int index)
    {
        if (index < 0 || index >= _accessories.Length)
        {
            for (int i = 0; i < _accessories.Length; i++)
            {
                GameObject go = _accessories[i];
                go.SetActive(false);
            }
            _curAccessoriesIndex = -1;
        }
        else
        {
            for (int i = 0; i < _accessories.Length; i++)
            {
                GameObject go = _accessories[i];
                if (i == index)
                {
                    go.SetActive(true);
                }
                else
                {
                    go.SetActive(false);
                }
            }
            _curAccessoriesIndex = index;
        }
    }
    private void LoadCustomize()
    {
        //string[] customizeData = Manager.User.GetCustomizeData();
        string[] splitData = _testSaveData.Split(',');
        if (splitData.Length < 6)
        {
            Debug.LogWarning("커스터마이즈 저장 데이터 이상함. 기본 세팅으로 불러옴");
            ChangeSkin(0);
            ChangeFace(0);
            ChangeHeadwear(-1);
            ChangeFacewear(-1);
            ChangeBag(-1);
            ChangeAccessory(-1);

            return;
        }
        else
        {
            int[] saveData = new int[6];

            for (int i = 0; i < 6; i++)
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
            ChangeSkin(saveData[0]);
            ChangeFace(saveData[1]);
            ChangeHeadwear(saveData[2]);
            ChangeFacewear(saveData[3]);
            ChangeBag(saveData[4]);
            ChangeAccessory(saveData[5]);
        }
    }

    public void SaveCustomize()
    {
        string saveSkin = _curSkinIndex.ToString();
        string saveFace = _curFaceIndex.ToString();
        string saveHeadwear = _curHeadwearIndex.ToString();
        string saveFacewear = _curFacewearIndex.ToString();
        string saveBag = _curBagIndex.ToString();
        string saveAcc = _curAccessoriesIndex.ToString();

        _testSaveData = $"{saveSkin}, {saveFace}, {saveHeadwear}, {saveFacewear}, {saveBag}, {saveAcc}";
        Debug.Log($"세이브 : {saveSkin}, {saveFace}, {saveHeadwear}, {saveFacewear}, {saveBag}, {saveAcc}");

        Manager.Audio.PlaySFX("Touch");
    }

    public void CancelCustomize()
    {
        LoadCustomize();
        _uis.SetActive(false);
    }

    public void RandomCustomize()
    {
        int randSkin = Random.Range(0, _skins.Length);
        int randFace = Random.Range(0, _faces.Length);
        int randHW = Random.Range(-1, _headwears.Length);
        int randFW = Random.Range(-1, _facewears.Length);
        int randBag = Random.Range(-1, _bags.Length);
        int randAcc = Random.Range(-1, _accessories.Length);

        ChangeSkin(randSkin);
        ChangeFace(randFace);
        ChangeHeadwear(randHW);
        ChangeFacewear(randFW);
        ChangeBag(randBag);
        ChangeAccessory(randAcc);

        ResetButtons();
        Manager.Audio.PlaySFX("Touch");
    }

    public void ResetCustomize()
    {
        LoadCustomize();
        ResetButtons();
    }
    private void ResetButtons()
    {
        CustomizingButton[] allButtons = FindObjectsOfType<CustomizingButton>();
        foreach (CustomizingButton btn in allButtons)
        {
            btn.Refresh();
        }
    }

    public int GetArrayLength(CosmeticCategory category)
    {
        switch (category)
        {
            case CosmeticCategory.None:
                return 0;

            case CosmeticCategory.Skin:
                return _skins.Length;

            case CosmeticCategory.Face:
                return _faces.Length;

            case CosmeticCategory.Headwear:
                return _headwears.Length;

            case CosmeticCategory.Facewear:
                return _facewears.Length;

            case CosmeticCategory.Bag:
                return _bags.Length;

            case CosmeticCategory.Accessory:
                return _accessories.Length;
        }
        return 0;
    }

    public int CurIndex(CosmeticCategory category)
    {
        switch (category)
        {
            case CosmeticCategory.None:
                return 0;

            case CosmeticCategory.Skin:
                return _curSkinIndex;

            case CosmeticCategory.Face:
                return _curFaceIndex;

            case CosmeticCategory.Headwear:
                return _curHeadwearIndex;

            case CosmeticCategory.Facewear:
                return _curFacewearIndex;

            case CosmeticCategory.Bag:
                return _curBagIndex;

            case CosmeticCategory.Accessory:
                return _curAccessoriesIndex;
        }
        return 0;
    }

    public void ChangeFeature(CosmeticCategory category, int index)
    {
        switch (category)
        {
            case CosmeticCategory.None:
                return;

            case CosmeticCategory.Skin:
                ChangeSkin(index);
                return;

            case CosmeticCategory.Face:
                ChangeFace(index);
                return;

            case CosmeticCategory.Headwear:
                ChangeHeadwear(index);
                return;

            case CosmeticCategory.Facewear:
                ChangeFacewear(index);
                return;

            case CosmeticCategory.Bag:
                ChangeBag(index);
                return;

            case CosmeticCategory.Accessory:
                ChangeAccessory(index);
                return;
        }
        return;
    }
}
