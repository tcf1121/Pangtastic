using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

public class MaskController : MonoBehaviour // 클래스 이름은 실제 사용하시는 이름으로 변경하세요
{
    // --- 인스펙터에서 아래 변수들을 할당해주세요 ---
    public UnityEngine.UI.Image _maskImage; // 마스크 UI 이미지
    public GameObject _boardObject; // BlockPlate 게임오브젝트
    public Canvas _canvas; // 마스크가 속한 캔버스
    public Camera _mainCamera; // 메인 카메라
    // -----------------------------------------

    void Start()
    {
        // Start에서 바로 실행하지 않고, 한 프레임 렌더링이 끝난 후에 실행하여 타이밍 문제를 방지합니다.
        StartCoroutine(SetMaskImageAfterFrame());
    }

    private IEnumerator SetMaskImageAfterFrame()
    {
        // 렌더링 파이프라인이 모두 완료될 때까지 기다립니다.
        yield return new WaitForEndOfFrame();

        SetMaskImage();
    }

    public void SetMaskImage()
    {
        // 1. 자식 렌더러들의 Bounds를 모두 합칩니다.
        Renderer[] childRenderers = _boardObject.GetComponentsInChildren<Renderer>();
        if (childRenderers.Length == 0)
        {
            Debug.LogError("보드 오브젝트의 자식 중에 Renderer 컴포넌트가 없습니다!", _boardObject);
            return;
        }

        Bounds totalBounds = childRenderers[0].bounds;
        for (int i = 1; i < childRenderers.Length; i++)
        {
            totalBounds.Encapsulate(childRenderers[i].bounds);
        }

        // --- 디버깅 로그 ---
        Debug.Log($"[Mask Debug] 계산된 전체 Bounds Center: {totalBounds.center}, Size: {totalBounds.size}");

        // 2. Bounds의 8개 꼭짓점을 모두 스크린 좌표로 변환합니다.
        // (단순히 min, max만 사용하는 것보다 회전이나 원근이 적용된 카메라에서 더 정확합니다)
        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(totalBounds.min.x, totalBounds.min.y, totalBounds.min.z);
        corners[1] = new Vector3(totalBounds.max.x, totalBounds.min.y, totalBounds.min.z);
        corners[2] = new Vector3(totalBounds.min.x, totalBounds.max.y, totalBounds.min.z);
        corners[3] = new Vector3(totalBounds.min.x, totalBounds.min.y, totalBounds.max.z);
        corners[4] = new Vector3(totalBounds.max.x, totalBounds.max.y, totalBounds.max.z);
        corners[5] = new Vector3(totalBounds.min.x, totalBounds.max.y, totalBounds.max.z);
        corners[6] = new Vector3(totalBounds.max.x, totalBounds.min.y, totalBounds.max.z);
        corners[7] = new Vector3(totalBounds.max.x, totalBounds.max.y, totalBounds.min.z);

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (Vector3 corner in corners)
        {
            Vector2 screenPoint = _mainCamera.WorldToScreenPoint(corner);
            if (screenPoint.x < minX) minX = screenPoint.x;
            if (screenPoint.y < minY) minY = screenPoint.y;
            if (screenPoint.x > maxX) maxX = screenPoint.x;
            if (screenPoint.y > maxY) maxY = screenPoint.y;
        }
        
        Vector2 screenBottomLeft = new Vector2(minX, minY);
        Vector2 screenTopRight = new Vector2(maxX, maxY);

        // 3. 마스크 RectTransform의 피벗과 앵커를 강제로 중심으로 설정 (매우 중요!)
        RectTransform maskRectTransform = _maskImage.rectTransform;
        maskRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        maskRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        maskRectTransform.pivot = new Vector2(0.5f, 0.5f);

        // 4. 스크린 좌표 기준으로 크기와 위치를 계산하고 적용합니다.
        float screenWidth = screenTopRight.x - screenBottomLeft.x;
        float screenHeight = screenTopRight.y - screenBottomLeft.y;

        // 캔버스 모드에 따라 위치 설정
        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // ScreenSpaceOverlay 에서는 position이 바로 스크린 좌표가 됩니다.
            maskRectTransform.position = screenBottomLeft + new Vector2(screenWidth / 2, screenHeight / 2);
            // 크기는 scaleFactor의 영향을 받지 않습니다.
            maskRectTransform.sizeDelta = new Vector2(screenWidth, screenHeight);
        }
        else // ScreenSpaceCamera 또는 WorldSpace
        {
            // Camera 모드에서는 캔버스 스케일링을 보정해주어야 합니다.
            maskRectTransform.position = screenBottomLeft + new Vector2(screenWidth / 2, screenHeight / 2);
            maskRectTransform.sizeDelta = new Vector2(screenWidth / _canvas.scaleFactor, screenHeight / _canvas.scaleFactor);
        }

        // --- 디버깅 로그 ---
        Debug.Log($"[Mask Debug] 최종 마스크 Position: {maskRectTransform.position}, SizeDelta: {maskRectTransform.sizeDelta}");
    }
}