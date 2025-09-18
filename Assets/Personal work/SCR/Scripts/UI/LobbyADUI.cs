using UnityEngine;
using UnityEngine.UI;

public class LobbyADUI : MonoBehaviour
{
    [SerializeField] RectTransform parentPanel;
    [SerializeField] RectTransform mainPanel;
    [SerializeField] RectTransform thisPanel;
    [SerializeField] RectTransform parentCanvasRect;

    void OnEnable()
    {
        RefreshLayout();
    }

    void OnDisable()
    {
        RefreshLayout();
    }

    private void RefreshLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentPanel);
        float height = parentPanel.rect.height;
        var offsetMin = mainPanel.offsetMin;
        offsetMin.y = height;
        mainPanel.offsetMin = offsetMin;
    }

    void Start()
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
