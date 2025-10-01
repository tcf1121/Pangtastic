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

    [SerializeField] private GameObject _available;
    [SerializeField] private GameObject _notAvailable;

    [SerializeField] private ButtonController _buttonController;
    private bool _isSelected = false;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);

        if(_customizeManager == null)
        {
            _customizeManager = FindObjectOfType<CustomizeManager>();
        }

        if (_buttonController == null)
        {
            _buttonController = GetComponentInParent<ButtonController>();
        }
    }

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnButtonClicked()
    {
        Manager.Audio.PlaySFX("Touch");
        _buttonController.Select(_category, this);
        _customizeManager.ChangeFeature(_category, _index);
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        _button.interactable = !_isSelected;
    }

    public void Refresh()
    {
        if (_index >= _customizeManager.GetArrayLength(_category))
        {
            _available.SetActive(false);
            _notAvailable.SetActive(true);
            _button.interactable = false;
        }
        else
        {
            _available.SetActive(true);
            _notAvailable.SetActive(false);

            if (_index == _customizeManager.CurIndex(_category))
            {
                SetSelected(true);
                _buttonController.Select(_category, this);
            }
            else
            {
                SetSelected(false);
            }
        }
    }

}
