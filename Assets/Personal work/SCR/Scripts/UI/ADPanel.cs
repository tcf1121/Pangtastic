using UnityEngine;

public class ADPanel : MonoBehaviour
{
    [SerializeField] RectTransform thisPanel;
    [SerializeField] RectTransform parentCanvasRect;



    void Start()
    {
        if (Manager.Ad.RemovedAD)
            Manager.Ad.BannerAdLoad += CheckPanel;
        else
            gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (Manager.Ad.RemovedAD)
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
