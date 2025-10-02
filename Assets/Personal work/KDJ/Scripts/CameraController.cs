using KDJ;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _extraZoomOut = 3f;
    private bool _isStarted = false;

    void Update()
    {
        if (_isStarted)
            SetCameraSize();
    }

    public void SetCameraSize()
    {
        Camera mainCamera = Camera.main;
        float screenAspect = (float)Screen.width / Screen.height;
        float boardWidth = BoardManager.Instance.Spawner.GameBoardData.Width;
        float thresholdAspect = 2f / 3f;
        float newOrthographicSize;

        newOrthographicSize = boardWidth / screenAspect / 2f;

        if (screenAspect > thresholdAspect)
        {
            float aspectDifference = screenAspect - thresholdAspect;

            // 차이가 클수록 더 많이 줌아웃되도록 추가 값을 더해줌
            newOrthographicSize += aspectDifference * _extraZoomOut;
        }

        mainCamera.orthographicSize = newOrthographicSize + 1.5f;
    }

    public void SetStarted(bool isStarted)
    {
        _isStarted = isStarted;
    }
}
