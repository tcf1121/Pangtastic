using UnityEngine;

public class ADPanel : MonoBehaviour
{
    [SerializeField] RectTransform thisPanel;
    [SerializeField] RectTransform parentCanvasRect;



    void Start()
    {
        Manager.Ad.BannerAdLoad += CheckPanel;
    }

    void OnDestroy()
    {
        Manager.Ad.BannerAdLoad -= CheckPanel;
    }

    private void CheckPanel()
    {
        float heightPx = Manager.Ad.bannerHeight;
        float canvasScale = parentCanvasRect.GetComponentInParent<Canvas>().scaleFactor;
        float heightCanvasUnits = heightPx / canvasScale;

        thisPanel.offsetMin = new Vector2(0, heightCanvasUnits);
        Vector2 size = thisPanel.sizeDelta;
        size.y = heightCanvasUnits;
        thisPanel.sizeDelta = size;
    }
}
