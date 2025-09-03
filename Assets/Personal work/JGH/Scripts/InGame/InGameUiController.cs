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
        HeartSystem.Instance.isPlaying = true;
    }

    public void ChangeOutGameScene()
    {
        Manager.Currency.pendingSpawnType = CurrencySystem.SpawnType.Exit;

        Manager.Currency.AddCoin(InGameManager.GetCoin());
        Manager.Currency.AddStar(1);

        SceneManager.LoadScene(2/*로비씬*/);

        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeContinueOutGameScene()
    {
        Manager.Currency.pendingSpawnType = CurrencySystem.SpawnType.Continue;

        needStartSetting = true;

        Manager.Currency.AddCoin(InGameManager.GetCoin());
        Manager.Currency.AddStar(1);

        SceneManager.LoadScene(3/*게임씬*/);

        AudioSystem.Instance.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeQuitGameScene()
    {
        SceneManager.LoadScene(2/*로비씬*/);
        Manager.Heart.UseHearts();
        Manager.Audio.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeRetryGameScene()
    {
        needStartSetting = true;
        Manager.Heart.UseHearts();
        SceneManager.LoadScene(3/*게임씬*/);
        Manager.Audio.PlaySFXByName("GameToHomeSfx");
    }

    public void SfxGameExit()
    {
        Manager.Audio.PlaySFXByName("GameExitSfx");
    }

}
