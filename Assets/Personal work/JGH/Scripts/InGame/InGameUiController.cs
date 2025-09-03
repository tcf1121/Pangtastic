using System.Collections;
using TMPro;
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
        Manager.Currency.pendingSpawnType = CurrencySystem.SpawnType.Exit;

        Manager.Currency.AddCoin(InGameManager.GetCoin());
        Manager.Currency.AddStar(1);

        SceneManager.LoadScene("OutGame Test Scene");

        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeContinueOutGameScene()
    {
        Manager.Currency.pendingSpawnType = CurrencySystem.SpawnType.Continue;

        needStartSetting = true;

        Manager.Currency.AddCoin(InGameManager.GetCoin());
        Manager.Currency.AddStar(1);

        SceneManager.LoadScene("InGameTest Scene");

        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeQuitGameScene()
    {
        SceneManager.LoadScene("OutGame Test Scene");
        Manager.Heart.UseHearts();
        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeRetryGameScene()
    {
        needStartSetting = true;
        Manager.Heart.UseHearts();
        SceneManager.LoadScene("InGameTest Scene");
        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }

    public void SfxGameExit()
    {
        AudioSystem.Instance.PlaySFXByName("GameExitSfx");
    }

}
