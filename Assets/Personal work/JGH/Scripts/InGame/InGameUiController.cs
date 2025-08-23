using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameUiController : MonoBehaviour
{
    public static bool needStartSetting = false;
    
    private void Start()
    {
        AudioSystem.Instance.PlayBGMByName("InGameMusic");
    }

    public void ChangeOutGameScene()
    {
        SceneManager.LoadScene("JGH_OutGameUI");
        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
        CoinRewardEffectSystem.Instance.CoinPlayEffect(10);
    }
    
    public void ChangeContinueOutGameScene()
    {
        needStartSetting = true;
        SceneManager.LoadScene("JGH_OutGameUI");
        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
        CoinRewardEffectSystem.Instance.CoinPlayEffect(10);
    }

    public void SfxGameExit()
    {
        AudioSystem.Instance.PlaySFXByName("GameExitSfx");
    }
}
