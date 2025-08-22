using UnityEngine;
using UnityEngine.SceneManagement;

public class OutGameUiController : MonoBehaviour
{
    private void Start()
    {
        AudioController.Instance.StopBGM();
        if (InGameUiController.needStartSetting)
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                if (root.name == "StartSetting")
                {
                    root.SetActive(true);
                }
            }
            InGameUiController.needStartSetting = false;
        }
    }

    public void ChangeInGameScene()
    {
        SceneManager.LoadScene("JGH_InGameUI");
    }

    public void SfxPlayStartButton()
    {
        AudioController.Instance.PlaySFXByName("GameInSfx");
    }
}
