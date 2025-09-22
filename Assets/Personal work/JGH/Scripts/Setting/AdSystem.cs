using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
//using AdSize = GoogleMobileAds.Api.AdSize;


public class AdSystem : Singleton<AdSystem>
{
    //public static AdSystem Instance { get; private set; }
    private const string RemoveAD = "RemoveAD";
    public bool RemovedAD { get { return _removedAD; } }
    private bool _removedAD;

    // ===================== Rewarded Interstitial (보상형) =====================
    private RewardedInterstitialAd _rewardedInterstitialAd;
    public Action OnRewardAdClosed;

    // ===================== App Open Event Ad (오프닝) =====================
    private AppOpenAd _appOpenAd;
    private DateTime _appOpenAdLoadTime;
    private bool _hasShownAtStartup = false; // 앱을 처음 시작했는지 체크

    // ===================== Interstitial Ad (전면) =====================
    private InterstitialAd _interstitialAd;
    public Action OnInterstitialAdClosed;

    // ===================== Banner Ad (배너) =====================
    private BannerView _bannerView;
    public float bannerHeight;

    protected override void Awake()
    {
        base.Awake();
        CheckRemoveAD();
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
        });

        // ===================== App Open Event Ad =====================
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }
    
    public bool CheckDays(int day)
    {
        // UserData dataInfo = Manager.Data.Load();
        List<String> savedData = Manager.User.GetLogAccessDates();
        
        // Json 저장 데이터 
        // DateTime startDay = DateTime.Parse(dataInfo.StartUtcDay);
        DateTime startDay = DateTime.Parse(savedData[0]);
        
        // 현재 UTC → 한국시간(+9)
        DateTime kstNow = DateTime.UtcNow.AddHours(9);

        // 기준일을 00시로 맞춤
        DateTime baseDate = new DateTime(startDay.Year, startDay.Month, startDay.Day, 0, 0, 0);

        // 2일(48시간) 경과 여부 체크
        bool isOverDays = (kstNow - baseDate).TotalDays >= day;

        Debug.Log($"dddddd 기준일: {baseDate:yyyy-MM-dd HH:mm:ss}");
        Debug.Log($"dddddd 현재 한국 시간: {kstNow:yyyy-MM-dd HH:mm:ss}");
        Debug.Log($"dddddd {day}일 지났는가? {isOverDays}");

        return isOverDays;
     }
    
    public bool HasOverTwoDaysGap(int day)
    {
        // UserData dataInfo = Manager.Data.Load();
        List<String> savedData = Manager.User.GetLogAccessDates();
        
        if (savedData == null || savedData.Count < 2)
        {
            // 로그가 없거나 1개밖에 없으면 비교할 수 없음
            return false;
        }

        for (int i = 0; i < savedData.Count - 1; i++)
        {
            if (!DateTime.TryParse(savedData[i], out DateTime currentDay) ||
                !DateTime.TryParse(savedData[i + 1], out DateTime nextDay))
            {
                Debug.LogError($"ddddddd [HasOverTwoDaysGap] 날짜 파싱 실패: {savedData[i]} or {savedData[i + 1]}");
                return false;
            }

            int diffDays = (nextDay.Date - currentDay.Date).Days;

            if (diffDays >= day)
            {
                Debug.Log($"ddddddd [HasOverTwoDaysGap] {currentDay:yyyy-MM-dd} → {nextDay:yyyy-MM-dd} ({diffDays}일 차이) {day}일 이상 발견 즉시 종료");
                return true; // 바로 빠져나옴
            }
        }
        
        return false;
    }

    void CheckRemoveAD()
    {
        int AD = PlayerPrefs.GetInt(RemoveAD);
        if (AD == 1) _removedAD = true;
        else _removedAD = false;
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

    public void ShowAD()
    {
        if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
        {
            _rewardedInterstitialAd.OnAdFullScreenContentClosed += ShowAdClosed;
            _rewardedInterstitialAd.Show((Reward reward) =>
            {
                Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");
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
        OnRewardAdClosed?.Invoke();

        // 이벤트 핸들러 해제 (중복 호출 방지)
        if (_interstitialAd != null)
        {
            _interstitialAd.OnAdFullScreenContentClosed -= ShowAdClosed;
        }

    }

    // ===================== App Open Ad =====================
    public void ShowAppOpenAd()
    {
        // 설치 후 2일 지나지 않음 -- 단순 2일 체크
        // if (!CheckDays(2))
            // return;
        
        // 설치 후 2일 지나지 않음 -- 접속일 기준으로 체크
        if (!HasOverTwoDaysGap(2))
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
    private void OnDestroy()
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
        InterstitialAd.Load("ca-app-pub-3940256099942544/1033173712", adRequest,
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
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
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
        Debug.Log("Interstitial ad closed. Notifying subscribers.");
        // 광고 닫힘 이벤트를 외부에 알림
        OnInterstitialAdClosed?.Invoke();

        // 이벤트 핸들러 해제 (중복 호출 방지)
        if (_interstitialAd != null)
        {
            _interstitialAd.OnAdFullScreenContentClosed -= InterstitialAdClosed;
        }
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
        // AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(width);
        AdSize adaptiveSize = new AdSize(320, 50);
        bannerHeight = DipsToPixels(adaptiveSize.Height);

        // 배너 생성
        // _bannerView = new BannerView("ca-app-pub-3940256099942544/6300978111", AdSize.Banner, AdPosition.Bottom);
        _bannerView = new BannerView("ca-app-pub-3940256099942544/6300978111", adaptiveSize, AdPosition.Bottom);

        BannerListenToAdEvents();

        // 보이기
        var adRequest = new AdRequest();
        _bannerView.LoadAd(adRequest);
    }

    public float DipsToPixels(float dips)
    {
        Debug.Log($"dips:{dips}");
        Debug.Log($"Pixels: {dips * Screen.dpi / 160f}");
        return dips * Screen.dpi / 160f;
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
