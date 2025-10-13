using UnityEngine;
using UnityEngine.UI;

public class LobbyADUI : MonoBehaviour
{
    [SerializeField] RectTransform parentPanel;
    [SerializeField] RectTransform mainPanel;
    [SerializeField] RectTransform thisPanel;
    [SerializeField] RectTransform parentCanvasRect;
    [SerializeField] RectTransform _customizePanel;

    void OnEnable()
    {
        if (Manager.Ad.RemovedAD)
            RefreshLayout();
    }

    void OnDisable()
    {
        if (Manager.Ad.RemovedAD)
            RefreshLayout();
    }

    void Start()
    {
        if (!Manager.Ad.RemovedAD)
            Manager.Ad.BannerAdLoad += CheckPanel;
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

    private void CheckPanel()
    {
        float bannerHeightInPixels = Manager.Ad.bannerHeight;
        float canvasHeight = parentCanvasRect.rect.height;
        float bannerRatio = bannerHeightInPixels / Screen.height;
        float finalHeightForUI = bannerRatio * canvasHeight;
        Debug.Log($"광고 배너{bannerHeightInPixels}/{canvasHeight}/{bannerRatio}/{finalHeightForUI}");
        Vector2 size = thisPanel.sizeDelta;
        size.y = finalHeightForUI;
        thisPanel.sizeDelta = size;
        RefreshLayout();
    }
}
