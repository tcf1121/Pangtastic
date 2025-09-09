using UnityEngine;

public class MapMover : MonoBehaviour
{
    public Camera cam;
    public float dragSpeed = 0.01f; // 속도 조절

    private Vector2 lastPos;
    private Vector2 minBounds = new(-10.9f, 0);
    private Vector2 maxBounds = new(10.9f, 10);


    void Update()
    {
        // 터치
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchWorldPos = cam.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, cam.transform.position.y));
            if (touch.phase == TouchPhase.Began)
            {
                lastPos = touchWorldPos;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 curPos = touchWorldPos;
                Vector3 dragVector = lastPos - curPos;
                if (dragVector.magnitude > 0.1f)  //너무 작은 값은 무시
                    cam.transform.position += new Vector3(dragVector.x, 0, dragVector.y);



                lastPos = curPos;
            }
        }

        // 마우스
        if (Input.GetMouseButtonDown(0))
        {
            lastPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.transform.position.y));
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 curPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.transform.position.y));
            Vector2 dragVector = lastPos - curPos;

            if (dragVector.magnitude > 0.1f) //너무 작은 값은 무시
                cam.transform.position += new Vector3(dragVector.x, 0, dragVector.y);

            lastPos = curPos;
        }
    }

    void LateUpdate()
    {
        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.z = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);
        cam.transform.position = pos;
    }
}
