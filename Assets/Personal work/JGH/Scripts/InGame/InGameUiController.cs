using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameUiController : MonoBehaviour
{
    public static bool needStartSetting = false;
    
    private void Start()
    {
        AudioSystem.Instance.PlayBGMByName("InGameMusic");
    }
    
    public void ChangeOutGameScene(int addCoin)
    {
        CoinRewardEffectSystem.Instance.pendingSpawnType = CoinRewardEffectSystem.SpawnType.Exit;
        
        CoinSystem.Instance.AddCoin(addCoin);
        
        SceneManager.LoadScene("JGH_OutGameUI");
        
        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }
    
    public void ChangeContinueOutGameScene(int addCoin)
    {
        CoinRewardEffectSystem.Instance.pendingSpawnType = CoinRewardEffectSystem.SpawnType.Continue;
        
        needStartSetting = true;
        
        CoinSystem.Instance.AddCoin(addCoin);

        SceneManager.LoadScene("JGH_OutGameUI");
        
        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }
    
    public void ChangeQuitGameScene()
    {
        SceneManager.LoadScene("JGH_OutGameUI");
        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }

    public void SfxGameExit()
    {
        AudioSystem.Instance.PlaySFXByName("GameExitSfx");
    }
    
}
