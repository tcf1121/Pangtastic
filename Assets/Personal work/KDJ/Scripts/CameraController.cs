using KDJ;
using UnityEngine;

public class CameraController : MonoBehaviour
{
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

        float newOrthographicSize = BoardManager.Instance.Spawner.GameBoardData.Width / screenAspect / 2f;

        mainCamera.orthographicSize = newOrthographicSize + 1.5f;
    }
    
    public void SetStarted(bool isStarted)
    {
        _isStarted = isStarted;
    }
}
