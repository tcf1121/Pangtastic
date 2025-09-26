using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlowButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _shopButton;
    [SerializeField] private Toggle _typeToggle;

    private void Awake()
    {
        _button.onClick.AddListener(Flow);
    }

    private void Flow()
    {
        if (_closeButton != null) _closeButton.onClick?.Invoke();
        _shopButton.onClick?.Invoke();
        _typeToggle.onValueChanged?.Invoke(true);
    }
}
