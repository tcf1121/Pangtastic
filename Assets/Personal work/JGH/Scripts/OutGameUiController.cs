using UnityEngine;
using UnityEngine.SceneManagement;

public class OutGameUiController : MonoBehaviour
{
    private void Start()
    {
        // 배경음, 효과음 설정에 따라 초기 볼륨 조정 :: S
        if (PlayerPrefs.GetInt("BackgroundMusicOnOff_Setting") == 0)
        {
            AudioController.Instance.BgmAudioSource.volume = 0f;
        }
        
        if (PlayerPrefs.GetInt("SfxMusicOnOff_Setting") == 0)
        {
            AudioController.Instance.SfxAudioSource.volume = 0f;   
        }
        // 배경음, 효과음 설정에 따라 초기 볼륨 조정 :: E
        
        // 배경음 정지
        AudioController.Instance.StopBGM();
        
        
        // 게임 클리어 후 계속하기 누르면 로비 화면으로 넘어오고 게임 시작 화면 활성화 :: S
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
        // 게임 클리어 후 계속하기 누르면 로비 화면으로 넘어오고 게임 시작 화면 활성화 :: E
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
