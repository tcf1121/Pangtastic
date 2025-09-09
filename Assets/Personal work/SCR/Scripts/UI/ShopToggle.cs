using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ShopToggle : MonoBehaviour
{
    [SerializeField] GameObject _isOnGO;
    private Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(IsOnToggle);
    }

    private void IsOnToggle(bool value)
    {
        _isOnGO.SetActive(!value);
    }
}
