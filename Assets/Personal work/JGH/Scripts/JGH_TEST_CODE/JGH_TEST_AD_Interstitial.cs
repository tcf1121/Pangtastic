using UnityEngine;
using UnityEngine.UI;

public class JGH_TEST_AD_Interstitial : MonoBehaviour
{
    void Start()
    {
        // 하위 버튼 전부 찾기
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            switch (btn.name)
            {
                case "ShowInterstitialAd":
                    btn.onClick.AddListener(() => Manager.Ad.ShowInterstitialAd());
                    break;

                case "LoadInterstitialAd":
                    btn.onClick.AddListener(() => Manager.Ad.LoadInterstitialAd());
                    break;
            }
        }
    }
}
