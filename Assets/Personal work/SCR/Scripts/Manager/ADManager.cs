using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections;
using System.Linq.Expressions;
using UnityEngine;
//using AdSize = GoogleMobileAds.Api.AdSize;


public class ADManager : Singleton<ADManager>
{
    //public static AdSystem Instance { get; private set; }
    private const string RemoveAD = "RemoveAD";
    public bool RemovedAD { get { return _removedAD; } }
    private bool _removedAD;

    // ===================== Rewarded Interstitial (보상형) =====================
    private RewardedInterstitialAd _rewardedInterstitialAd;
    public Action OnRewardAdClosed;
    private bool _getReward = false;

    // ===================== App Open Event Ad (오프닝) =====================
    private AppOpenAd _appOpenAd;
    private DateTime _appOpenAdLoadTime;
    private bool _hasShownAtStartup = false; // 앱을 처음 시작했는지 체크

    // ===================== Interstitial Ad (전면) =====================
    private InterstitialAd _interstitialAd;
    public Action OnInterstitialAdClosed;
    private float prevTimeScale;

    // ===================== Banner Ad (배너) =====================
    private BannerView _bannerView;
    public Action BannerAdLoad;
    public float bannerHeight;

    private int adDay = 2;

    // 광고 ID (테스트 용)
    // private string rewardedInterstitialAdID = "ca-app-pub-3940256099942544/5354046379";
    // private string appOpenAdID = "ca-app-pub-3940256099942544/9257395921";
    // private string interstitialAdID = "ca-app-pub-3940256099942544/1033173712";
    // private string bannerAdID = "ca-app-pub-3940256099942544/6300978111";
    // 광고 ID (실제 사용)
    private string rewardedInterstitialAdID = "ca-app-pub-6717704131793477/7329232893";
    private string appOpenAdID = "ca-app-pub-6717704131793477/6016151222";
    private string interstitialAdID = "ca-app-pub-6717704131793477/7421874537";
    private string bannerAdID = "ca-app-pub-6717704131793477/2547679507";

    protected override void Awake()
    {
        base.Awake();
        CheckRemoveAD();

        PlayerPrefs.SetInt(RemoveAD, 0); //테스트용 지울예정
    }

    private void Start()
    {
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            if (initstatus == null)
            {
                Debug.LogError("Google Mobile Ads initialization failed.");
                return;
            }
            Debug.Log("Google Mobile Ads initialization complete.");
            if (!Manager.Ad.RemovedAD)
            {
                // ===================== Rewarded Interstitial =====================
                LoadAD();
                // ===================== App Open Ad =====================
                // 로그인 화면에서 불러옴

                LoadAppOpenAd();

                // ===================== Interstitial Ad =====================
                LoadInterstitialAd();
                // ===================== Banner Ad =====================
                // 로비와 게임 화면에서 불러옴
                //BannerCreateView();
            }
        });

        // ===================== App Open Event Ad =====================
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }

    void CheckRemoveAD()
    {
        int AD = PlayerPrefs.GetInt(RemoveAD);
        if (AD == 1) _removedAD = true;
        else _removedAD = false;
    }

    public void BuyRemoveAD()
    {
        _removedAD = true;
        PlayerPrefs.SetInt(RemoveAD, 1);

        OutGameManager.CloseBannerAd();
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
        RewardedInterstitialAd.Load(rewardedInterstitialAdID, adRequest,
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
            _rewardedInterstitialAd.OnAdFullScreenContentClosed += ShowAdClosed;
            _rewardedInterstitialAd.Show((Reward reward) =>
            {
                Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");
                _getReward = true;
            });

            _rewardedInterstitialAd = null;
        }
        else
        {
            Debug.Log("광고 준비 안 됨!");
        }
    }

    public void ShowAdClosed()
    {
        Debug.Log("RewardAdClosed ad closed. Notifying subscribers.");
        // 광고 닫힘 이벤트를 외부에 알림
        if (_getReward)
            StartCoroutine(WaitAndInvokeRewoard());
        else
        {
            if (OnRewardAdClosed != null)
            {
                OnRewardAdClosed = null;
            }
        }
        // 이벤트 핸들러 해제 (중복 호출 방지)
        if (_rewardedInterstitialAd != null)
        {
            _rewardedInterstitialAd.OnAdFullScreenContentClosed -= ShowAdClosed;
        }


    }

    private IEnumerator WaitAndInvokeRewoard()
    {
        yield return new WaitForSeconds(0.1f);

        _getReward = false;
        OnRewardAdClosed?.Invoke();
        OnRewardAdClosed = null;
    }

    // ===================== App Open Ad =====================
    public void LoadAppOpenAd()
    {
        // Clean up the old ad before loading a new one.
        if (_appOpenAd != null)
        {
            _appOpenAd.Destroy();
            _appOpenAd = null;
        }

        Debug.Log("Loading the app open ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        AppOpenAd.Load(appOpenAdID, adRequest,
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

                _appOpenAd = ad;
                _appOpenAdLoadTime = DateTime.UtcNow;
                OpenAdRegisterEventHandlers(ad);

                // 앱 처음 시작에서만 적용
                if (!_hasShownAtStartup)
                {
                    _hasShownAtStartup = true;
                    ShowAppOpenAd();
                }
            });
    }

    public void ShowAppOpenAd()
    {
        // 설치 후 2일 지나지 않음
        if (!Manager.Date.GetLoginStreak(adDay))
            return;

        if (_appOpenAd != null && _appOpenAd.CanShowAd())
        {
            Debug.Log("Showing app open ad.");


            _appOpenAd.Show();
        }
        else
        {
            Debug.LogError("App open ad is not ready yet.");
            LoadAppOpenAd();
        }
    }

    private void OpenAdRegisterEventHandlers(AppOpenAd ad)
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
            _appOpenAd.Destroy();
            _appOpenAd = null;
            LoadAppOpenAd();
        };
        // Raised when the ad failed to open full screen content.
        // 열리지 못하고 실패했을때
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("App open ad failed to open full screen content " +
                           "with error : " + error);
            _appOpenAd.Destroy();
            _appOpenAd = null;
            LoadAppOpenAd();
        };
    }
    protected override void OnDestroy()
    {
        // Always unlisten to events when complete.
        AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
    }

    private void OnAppStateChanged(AppState state)
    {
        Debug.Log("App State changed to : " + state);

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
        return _appOpenAd != null
               && _appOpenAd.CanShowAd()
               && (DateTime.UtcNow - _appOpenAdLoadTime).TotalHours < 4; // 만료 방지
    }


    // ===================== Interstitial Ad =====================
    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading interstitial ad...");

        var adRequest = new AdRequest();
        InterstitialAd.Load(interstitialAdID, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Interstitial failed to load: " + error);
                    return;
                }
                // The ad loaded successfully.
                Debug.Log("Interstitial loaded.");
                _interstitialAd = ad;

                // 이벤트 등록
                InterstitialListenToAdEvents(ad);
            });
    }

    public void ShowInterstitialAd()
    {
        if (!Manager.Date.GetLoginStreak(adDay))
            return;
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            prevTimeScale = Time.timeScale;
            Time.timeScale = 1;
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.OnAdFullScreenContentClosed += InterstitialAdClosed;
            _interstitialAd.Show();
            _interstitialAd = null;
        }
        else
        {
            Debug.LogWarning("Interstitial ad not ready.");
            LoadInterstitialAd();
        }
    }

    private void InterstitialAdClosed()
    {
        Time.timeScale = prevTimeScale;
        Debug.Log("Interstitial ad closed. Notifying subscribers.");
        // 광고 닫힘 이벤트를 외부에 알림
        StartCoroutine(WaitAndInvokeInterstitial());

        // 이벤트 핸들러 해제 (중복 호출 방지)
        if (_interstitialAd != null)
        {
            _interstitialAd.OnAdFullScreenContentClosed -= InterstitialAdClosed;
        }
    }

    private IEnumerator WaitAndInvokeInterstitial()
    {
        yield return new WaitForSeconds(0.1f);

        OnInterstitialAdClosed?.Invoke();
        OnInterstitialAdClosed = null;
    }

    void InterstitialListenToAdEvents(InterstitialAd interstitialAd)
    {
        // [START ad_events]
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            // Raised when the ad is estimated to have earned money.
        };
        interstitialAd.OnAdImpressionRecorded += () =>
        {
            // Raised when an impression is recorded for an ad.
        };
        interstitialAd.OnAdClicked += () =>
        {
            // Raised when a click is recorded for an ad.
        };
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            // Raised when the ad opened full screen content.
        };
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            LoadInterstitialAd();
        };
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            LoadInterstitialAd();
        };
        // [END ad_events]]
    }

    // ===================== Banner Ad (배너) =====================
    public void BannerCreateView()
    {
        Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.
        if (_bannerView != null)
        {
            BannerDestroyAd();
        }

        // 화면 폭(dp 단위) 구하기
        // int width = Screen.width / (int)(Screen.dpi / 160f);
        // 적응형 배너 크기 얻기
        AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        //AdSize adaptiveSize = AdSize.Banner;
        // 배너 생성
        // _bannerView = new BannerView("ca-app-pub-3940256099942544/6300978111", AdSize.Banner, AdPosition.Bottom);
        _bannerView = new BannerView(bannerAdID, adaptiveSize, AdPosition.Bottom);

        BannerListenToAdEvents();

        // 보이기
        _bannerView.OnBannerAdLoaded += OnBannerLoaded;
        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("배너 로드 실패: " + error);
        };
        var adRequest = new AdRequest();

        _bannerView.LoadAd(adRequest);
    }

    private void OnBannerLoaded()
    {
        Debug.Log("배너 로드 완료!");

        // 광고 로드 후 실제 높이 가져오기
        bannerHeight = _bannerView.GetHeightInPixels();
        BannerAdLoad?.Invoke();
    }

    private void BannerListenToAdEvents()
    {
        // Raised when an ad is loaded into the banner view.
        _bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                      + _bannerView.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                           + error);
        };
        // Raised when the ad is estimated to have earned money.
        _bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        _bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        _bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        _bannerView.OnAdFullScreenContentOpened += () =>
        {
            BannerCreateView();
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        _bannerView.OnAdFullScreenContentClosed += () =>
        {
            BannerCreateView();
            Debug.Log("Banner view full screen content closed.");
        };
    }

    public void BannerDestroyAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Destroying banner view.");
            _bannerView.Destroy();
            _bannerView = null;
        }
    }
}
