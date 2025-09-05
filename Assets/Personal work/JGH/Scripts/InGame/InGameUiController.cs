using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameUiController : MonoBehaviour
{
    public static bool needStartSetting = false;

    private void Start()
    {
        Manager.Audio.PlayBGMByName("InGameMusic");
        //Manager.Heart.isPlaying = true;
    }

    public void ChangeOutGameScene()
    {
        //Manager.Currency.pendingSpawnType = CurrencySystem.SpawnType.Exit;

        Manager.User.AddCoin(InGameManager.GetCoin());
        Manager.User.AddStar(1);
        Manager.User.AddHeart();

        SceneManager.LoadScene(2/*로비씬*/);
        Manager.Audio.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeContinueOutGameScene()
    {
        //Manager.Currency.pendingSpawnType = CurrencySystem.SpawnType.Continue;

        needStartSetting = true;

        Manager.User.AddCoin(InGameManager.GetCoin());
        Manager.User.AddStar(1);

        SceneManager.LoadScene(3/*게임씬*/);

        Manager.Audio.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeQuitGameScene()
    {
        SceneManager.LoadScene(2/*로비씬*/);
        Manager.Audio.PlaySFXByName("GameToHomeSfx");
    }

    public void ChangeRetryGameScene()
    {
        needStartSetting = true;
        Manager.User.UseHeart();
        SceneManager.LoadScene(3/*게임씬*/);
        Manager.Audio.PlaySFXByName("GameToHomeSfx");
    }

    public void SfxGameExit()
    {
        Manager.Audio.PlaySFXByName("GameExitSfx");
    }

}
