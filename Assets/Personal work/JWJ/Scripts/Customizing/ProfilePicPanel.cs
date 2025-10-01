using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePicPanel : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _picPanel;

    private bool _isOpen = false;

    private void Awake()
    {
        if(_button == null)
        {
            _button = GetComponent<Button>();
        }

        _button.onClick.AddListener(OpenAndClosePicPanel);
    }

    private void OnEnable()
    {
        _picPanel.SetActive(false);
        _isOpen = false;
    }

    private void OpenAndClosePicPanel()
    {
        _isOpen = !_isOpen;
        _picPanel.SetActive(_isOpen);
    }
}
