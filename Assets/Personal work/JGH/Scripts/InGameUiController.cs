using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUiController : MonoBehaviour
{
    public static bool needStartSetting = false;
    
    private void Start()
    {
        AudioController.Instance.PlayBGMByName("InGameMusic");
    }

    public void ChangeOutGameScene()
    {
        SceneManager.LoadScene("JGH_OutGameUI");
    }
    
    public void ChangeContinueOutGameScene()
    {
        needStartSetting = true;
        SceneManager.LoadScene("JGH_OutGameUI");
    }

    public void SfxGameExit()
    {
        AudioController.Instance.PlaySFXByName("GameExitSfx");
    }
    
    public void SfxGameToHome()
    {
        AudioController.Instance.PlaySFXByName("GameToHomeSfx");
    }
}
