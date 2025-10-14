using UnityEngine;
using UnityEngine.UI;

public class LobbyADUI : MonoBehaviour
{
    [SerializeField] RectTransform parentPanel;
    [SerializeField] RectTransform mainPanel;
    [SerializeField] RectTransform thisPanel;
    [SerializeField] RectTransform parentCanvasRect;
    [SerializeField] RectTransform _customizePanel;
    private CanvasScaler bannerCanvas;

    void OnEnable()
    {
        RefreshLayout();
    }

    void OnDisable()
    {
        RefreshLayout();
    }

    void Awake()
    {
        if (!Manager.Ad.RemovedAD)
        {
            Debug.Log("배너 광고 생성 이벤트 추가");
            Manager.Ad.BannerAdLoad += CheckPanel;
        }

        else
        {
            gameObject.SetActive(false);
            RefreshLayout();
        }

    }

    void OnDestroy()
    {
        if (!Manager.Ad.RemovedAD)
            Manager.Ad.BannerAdLoad -= CheckPanel;
    }

    private void RefreshLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentPanel);
        float height = parentPanel.rect.height;
        var offsetMin = mainPanel.offsetMin;
        offsetMin.y = height;
        mainPanel.offsetMin = offsetMin;

        if (_customizePanel != null)
        {
            float bannerHeight = thisPanel.rect.height;
            var customizeOffset = _customizePanel.offsetMin;
            customizeOffset.y = bannerHeight;
            _customizePanel.offsetMin = customizeOffset;
        }

    }

    private void ChangerCanvasF()
    {
        GameObject go = GameObject.Find("ADAPTIVE(Clone)");
        if (go == null) return;

        Debug.Log($"배너 광고창 찾음{go}");
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

    private void CheckPanel()
    {
        Debug.Log("배너 광고 생성 후 사이즈 측정");
        ChangerCanvasF();
        //ChangerCanvasS();
        RefreshLayout();
    }


}
