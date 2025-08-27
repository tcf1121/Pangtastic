using GoogleMobileAds.Api;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OutGameUiController : MonoBehaviour
{
    // 광고 :: S
    private RewardedInterstitialAd _rewardedInterstitialAd;

    /// <summary>
    /// Loads the rewarded interstitial ad.
    /// </summary>
    public void LoadAD()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedInterstitialAd != null)
        {
            _rewardedInterstitialAd.Destroy();
            _rewardedInterstitialAd = null;
        }

        Debug.Log("Loading the rewarded interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        // send the request to load the ad.
        RewardedInterstitialAd.Load("ca-app-pub-3940256099942544/5354046379", adRequest,
        (RewardedInterstitialAd ad, LoadAdError error) =>
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                Debug.LogError("rewarded interstitial ad failed to load an ad " +
                               "with error : " + error);
                return;
            }

            Debug.Log("Rewarded interstitial ad loaded with response : "
                      + ad.GetResponseInfo());

            _rewardedInterstitialAd = ad;
            
            
            // 이벤트 등록
            _rewardedInterstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Ad closed → reload next ad");
                LoadAD();
            };

            _rewardedInterstitialAd.OnAdFullScreenContentFailed += (AdError adError) =>
            {
                Debug.LogError("Ad failed to show: " + adError);
            };
        });
        
    }
    public void ShowAD()
    {
        if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
        {
            _rewardedInterstitialAd.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");
            });
        }
    }
    // 광고 :: E
    
    private void Start()
    {
        // 배경음, 효과음 설정에 따라 초기 볼륨 조정 :: S
        if (PlayerPrefs.GetInt("BackgroundMusicOnOff_Setting") == 0)
        {
            AudioSystem.Instance.BgmAudioSource.volume = 0f;
        }
        
        if (PlayerPrefs.GetInt("SfxMusicOnOff_Setting") == 0)
        {
            AudioSystem.Instance.SfxAudioSource.volume = 0f;   
        }
        // 배경음, 효과음 설정에 따라 초기 볼륨 조정 :: E
        
        // 배경음 정지
        // AudioController.Instance.StopBGM();
        
        AudioSystem.Instance.PlayBGMByName("OutGameMusic");
        
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
        
        // 광고 :: S
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            if (initstatus == null)
            {
                Debug.LogError("Google Mobile Ads initialization failed.");
                return;
            }

            // 10초 뒤 자동으로 광고 시도
            LoadAD();
            Debug.Log("Google Mobile Ads initialization complete.");
            
        }); 
        // 광고 :: E
    }
    

  

    public void ChangeInGameScene(int num)
    {
        AudioSystem.Instance.PlaySFXByName("GameInSfx");
        if (HeartSystem.Instance.TryUseHearts(num))
        {
            SceneManager.LoadScene("JGH_InGameUI");
        }
        else
        {
            // TODO: 하트 부족 시 UI 띄우기
        }
    }
    
   
}
