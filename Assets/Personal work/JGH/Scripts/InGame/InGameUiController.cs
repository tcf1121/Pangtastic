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
        CurrencySystem.Instance.pendingSpawnType = CurrencySystem.SpawnType.Exit;
        
        CurrencySystem.Instance.AddCoin(addCoin);
        CurrencySystem.Instance.AddStar(addCoin);
        
        SceneManager.LoadScene("JGH_OutGameUI");
        
        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }
    
    public void ChangeContinueOutGameScene(int addCoin)
    {
        CurrencySystem.Instance.pendingSpawnType = CurrencySystem.SpawnType.Continue;
        
        needStartSetting = true;
        
        CurrencySystem.Instance.AddCoin(addCoin);
        CurrencySystem.Instance.AddStar(addCoin);

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
