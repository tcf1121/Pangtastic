using UnityEngine;
using UnityEngine.UI;

public class ADPanel : MonoBehaviour
{
    [SerializeField] RectTransform thisPanel;
    [SerializeField] RectTransform parentCanvasRect;
    private CanvasScaler bannerCanvas;

    void Awake()
    {
        if (!Manager.Ad.RemovedAD)
            Manager.Ad.BannerAdLoad += CheckPanel;
        else
            gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (!Manager.Ad.RemovedAD)
            Manager.Ad.BannerAdLoad -= CheckPanel;
    }

    private void CheckPanel()
    {
        Debug.Log("인게임 배너 광고 생성 후 사이즈 측정");
        ChangerCanvasF();
        //ChangerCanvasS();
    }

    private void ChangerCanvasF()
    {
        GameObject go = GameObject.Find("ADAPTIVE(Clone)");
        if (go == null) return;

        Debug.Log($"인게임 배너 광고창 찾음{go}");
        bannerCanvas = go.GetComponent<CanvasScaler>();
        bannerCanvas.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        bannerCanvas.referenceResolution = new Vector2(1080, 1920);
        bannerCanvas.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        bannerCanvas.referencePixelsPerUnit = 100f;
        float bannerHeightInPixels = Manager.Ad.bannerHeight;
        Vector2 size = thisPanel.sizeDelta;
        size.y = bannerHeightInPixels;
        thisPanel.sizeDelta = size;
    }

    private void ChangerCanvasS()
    {
        float bannerHeightInPixels = Manager.Ad.bannerHeight;
        float canvasHeight = parentCanvasRect.rect.height;
        float bannerRatio = bannerHeightInPixels / Screen.height;
        float finalHeightForUI = bannerRatio * canvasHeight;
        Debug.Log($"광고 배너{bannerHeightInPixels}/{canvasHeight}/{bannerRatio}/{finalHeightForUI}");
        Vector2 size = thisPanel.sizeDelta;
        size.y = finalHeightForUI;
        thisPanel.sizeDelta = size;
    }


}
