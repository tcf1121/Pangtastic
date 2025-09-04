using KDJ;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        if (BoardManager.Instance.Spawner.GameBoardData != null)
        SetCameraSize();
    }

    public void SetCameraSize()
    {
        Camera mainCamera = Camera.main;
        float screenAspect = (float)Screen.width / Screen.height;

        float newOrthographicSize = BoardManager.Instance.Spawner.GameBoardData.Width / screenAspect / 2f;

        mainCamera.orthographicSize = newOrthographicSize + 1f;
    }
}
