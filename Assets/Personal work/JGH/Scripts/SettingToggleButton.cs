using UnityEngine;
using UnityEngine.UI;

public class SettingToggleButton : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    private bool isOn = true;
    private static string prefKey;
    
    private void Awake()
    {
        prefKey = gameObject.name + "_Setting";
        
        isOn = PlayerPrefs.GetInt(prefKey, 1) == 1;
        ApplySetting();
    }


    public void Toggle()
    {
        if (targetImage == null) return;
        
        prefKey = gameObject.name + "_Setting";

        isOn = !isOn;
        
        PlayerPrefs.SetInt(prefKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
        
        ApplySetting();

    }

    private void ApplySetting()
    {
        if (isOn)
        {
            targetImage.sprite = onSprite;

            if (gameObject.name == "BackgroundMusicOnOff")
            {
                AudioController.Instance.BgmAudioSource.volume = 1f;
            }
            
            if (gameObject.name == "SfxMusicOnOff")
            {
                AudioController.Instance.SfxAudioSource.volume = 1f;
            }
        }
        else
        {
            targetImage.sprite = offSprite;

            if (gameObject.name == "BackgroundMusicOnOff")
            {
                AudioController.Instance.BgmAudioSource.volume = 0f;
            }
            
            if (gameObject.name == "SfxMusicOnOff")
            {
                AudioController.Instance.SfxAudioSource.volume = 0f;
            }
        }
    }
}