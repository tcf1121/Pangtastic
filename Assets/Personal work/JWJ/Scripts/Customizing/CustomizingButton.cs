using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizingButton : MonoBehaviour
{
    [SerializeField] private CosmeticCategory _category;
    [SerializeField] private Button _button;
    [SerializeField] private int _index;
    [SerializeField] private CustomizeManager _customizeManager;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
        _customizeManager = FindObjectOfType<CustomizeManager>();
    }

    private void OnButtonClicked()
    {
        if (_category == CosmeticCategory.Skin)
        {
            _customizeManager.ChangeSkin(_index);
        }
        else if (_category == CosmeticCategory.Face)
        {
            _customizeManager.ChangeFace(_index);
        }
        else if (_category == CosmeticCategory.Headwear)
        {
            _customizeManager.ChangeHeadwear(_index);
        }
        else if (_category == CosmeticCategory.Facewear)
        {
            _customizeManager.ChangeFacewear(_index);
        }
        else if (_category == CosmeticCategory.Bag)
        {
            _customizeManager.ChangeBag(_index);
        }
        else if (_category == CosmeticCategory.Accessory)
        {
            _customizeManager.ChangeAccessory(_index);
        }
        else
        {
            Debug.LogError($"커스텀 아이템 타입이 이상함: {gameObject.name}");
        }
        
    }
}
