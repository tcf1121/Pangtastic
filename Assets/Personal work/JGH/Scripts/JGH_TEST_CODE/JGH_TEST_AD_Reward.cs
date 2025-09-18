using UnityEngine;
using UnityEngine.UI;

public class JGH_TEST_AD_Reward : MonoBehaviour
{
    void Start()
    {
        // 하위 버튼 전부 찾기
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            switch (btn.name)
            {
                case "ShowAd":
                    btn.onClick.AddListener(() => Manager.Ad.ShowAD());
                    break;

                case "LoadAd":
                    btn.onClick.AddListener(() => Manager.Ad.LoadAD());
                    break;
            }
        }
    }
}
