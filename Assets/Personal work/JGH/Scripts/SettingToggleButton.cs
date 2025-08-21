using UnityEngine;
using UnityEngine.UI;

public class SettingToggleButton : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    private bool isOn = true;

    public void Toggle()
    {
        if (targetImage == null) return;

        isOn = !isOn;
        targetImage.sprite = isOn ? onSprite : offSprite;
    }
}