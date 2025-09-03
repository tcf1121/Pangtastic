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
        CurrencySystem.Instance.pendingSpawnType = CurrencySystem.SpawnType.Exit;

        CurrencySystem.Instance.AddCoin(InGameManager.GetCoin());
        CurrencySystem.Instance.AddStar(1);

        SceneManager.LoadScene(2/*로비씬*/);

        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeContinueOutGameScene()
    {
        CurrencySystem.Instance.pendingSpawnType = CurrencySystem.SpawnType.Continue;

        needStartSetting = true;

        CurrencySystem.Instance.AddCoin(InGameManager.GetCoin());
        CurrencySystem.Instance.AddStar(1);

        SceneManager.LoadScene(3/*게임씬*/);

        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeQuitGameScene()
    {
        SceneManager.LoadScene(2/*로비씬*/);
        HeartSystem.Instance.UseHearts();
        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeRetryGameScene()
    {
        needStartSetting = true;
        HeartSystem.Instance.UseHearts();
        SceneManager.LoadScene(3/*게임씬*/);
        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }

    public void SfxGameExit()
    {
        AudioSystem.Instance.PlaySFXByName("GameExitSfx");
    }

}
