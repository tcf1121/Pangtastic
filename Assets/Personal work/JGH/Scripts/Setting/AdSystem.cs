using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine;


public class AdSystem : Singleton<AdSystem>
{
    //public static AdSystem Instance { get; private set; }

    private RewardedInterstitialAd _rewardedInterstitialAd;
    
    private AppOpenAd appOpenAd;
    private DateTime appOpenAdLoadTime;
    private bool hasShownAtStartup = false; // 앱을 처음 시작했는지 체크

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // ===================== Rewarded Interstitial =====================
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            if (initstatus == null)
            {
                Debug.LogError("Google Mobile Ads initialization failed.");
                return;
            }

            LoadAD();
            Debug.Log("Google Mobile Ads initialization complete.");
        });
        
        // ===================== App Open Event Ad =====================
        LoadAppOpenAd();
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }
    

    // ===================== Rewarded Interstitial =====================
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
    public void ShowAD(Action action = null)
    {
        if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
        {
            _rewardedInterstitialAd.Show((Reward reward) =>
            {
                action?.Invoke();
                Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");
            });

            _rewardedInterstitialAd = null;
        }
        else
        {
            Debug.Log("광고 준비 안 됨!");
        }
    }
    
    // ===================== App Open Ad =====================
    public void LoadAppOpenAd()
    {
        // Clean up the old ad before loading a new one.
        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }

        Debug.Log("Loading the app open ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        AppOpenAd.Load("ca-app-pub-3940256099942544/9257395921", adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("app open ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("App open ad loaded with response : "
                          + ad.GetResponseInfo());

                appOpenAd = ad;
                appOpenAdLoadTime = DateTime.UtcNow;
                RegisterEventHandlers(ad);
                
                // 앱 처음 시작에서만 적용
                if (!hasShownAtStartup)
                {
                    hasShownAtStartup = true;
                    ShowAppOpenAd();
                }
            });
    }
    
    private void RegisterEventHandlers(AppOpenAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        // 수익 발생시
        // 단가, 통화정보 등 통계시 사용
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("App open ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        // 광고 노출 기록
        // 통계시 사용
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        // 클릭 했을때
        // 클릭률 통계시 사용
        ad.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        // 풀스크린 열렸을때
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App open ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        // 광고가 닫혔을때
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App open ad full screen content closed.");
            appOpenAd.Destroy();
            appOpenAd = null;
            LoadAppOpenAd();
        };
        // Raised when the ad failed to open full screen content.
        // 열리지 못하고 실패했을때
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("App open ad failed to open full screen content " +
                           "with error : " + error);
            appOpenAd.Destroy();
            appOpenAd = null;
            LoadAppOpenAd();
        };
    }
    private void OnDestroy()
    {
        // Always unlisten to events when complete.
        AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
    } 
    
    private void OnAppStateChanged(AppState state)
    {
        Debug.Log("App State changed to : "+ state);

        // if the app is Foregrounded and the ad is available, show it.
        if (state == AppState.Foreground)
        {
            if (IsAppOpenAdAvailable())
            {
                ShowAppOpenAd();
            }
            else
            {
                LoadAppOpenAd();
            }
        }
    }
    private bool IsAppOpenAdAvailable()
    {
        return appOpenAd != null
               && appOpenAd.CanShowAd()
               && (DateTime.UtcNow - appOpenAdLoadTime).TotalHours < 4; // 만료 방지
    }
    
    public void ShowAppOpenAd()
    {
        if (appOpenAd != null && appOpenAd.CanShowAd())
        {
            Debug.Log("Showing app open ad.");
            appOpenAd.Show();
            
            appOpenAd = null;
            
            LoadAppOpenAd();
        }
        else
        {
            Debug.LogError("App open ad is not ready yet.");
            LoadAppOpenAd();
        }
    }

}