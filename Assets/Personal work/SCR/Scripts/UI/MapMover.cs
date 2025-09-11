using UnityEngine;

public class MapMover : MonoBehaviour
{
    private Camera _cam;

    [Header("드래그 설정")]
    [SerializeField] private float _dragSpeed = 1f;
    [SerializeField] private float threshold = 0.1f;
    [SerializeField] private float _deadZone = 1f; // 작은 움직임 무시
    [SerializeField] private float _smoothTime = 0.1f; // 부드러운 이동 시간

    [Header("줌 설정")]
    [SerializeField] private float _zoomSpeed = 0.1f;
    [SerializeField] private float _minZoom = 3f;
    [SerializeField] private float _maxZoom = 10f;

    [Header("맵 범위 제한")]
    private Vector2 _minBounds = new(-10.9f, 0);
    private Vector2 _maxBounds = new(10.9f, 10);

    private Vector2 _lastWorldPos;
    private Vector3 _targetPos;
    private Vector3 _velocity = Vector3.zero;

    void Start()
    {
        if (_cam == null) _cam = Camera.main;
        _targetPos = _cam.transform.position;
    }

    void Update()
    {
        HandleDrag();
        HandleZoom();

        // 부드럽게 카메라 이동
        _cam.transform.position = Vector3.SmoothDamp(
            _cam.transform.position,
            _targetPos,
            ref _velocity,
            _smoothTime
        );


    }

    void FixedUpdate()
    {
        ClampCamera();
    }

    void HandleDrag()
    {
        // 모바일
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchWorldPos = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                _lastWorldPos = touchWorldPos;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = _lastWorldPos - touchWorldPos;
                if (Mathf.Abs(delta.x) < threshold) delta.x = 0;
                if (Mathf.Abs(delta.y) < threshold) delta.y = 0;

                if (delta.magnitude > _deadZone)
                {
                    delta = delta.normalized;
                    _targetPos += new Vector3(delta.x, 0, delta.y) * _dragSpeed;
                }

                _lastWorldPos = touchWorldPos;
            }
        }

        // PC
        if (Input.GetMouseButtonDown(0))
        {
            _lastWorldPos = Input.mousePosition;

        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 currentPos = Input.mousePosition;

            Vector2 delta = _lastWorldPos - currentPos;
            if (Mathf.Abs(delta.x) < threshold) delta.x = 0;
            if (Mathf.Abs(delta.y) < threshold) delta.y = 0;

            if (delta.magnitude > _deadZone)
            {
                delta = delta.normalized;
                _targetPos += new Vector3(delta.x, 0, delta.y) * _dragSpeed;
            }

            _lastWorldPos = currentPos;
        }
    }

    void HandleZoom()
    {
        // 모바일
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prevPos0 = t0.position - t0.deltaPosition;
            Vector2 prevPos1 = t1.position - t1.deltaPosition;

            float prevMag = (prevPos0 - prevPos1).magnitude;
            float currentMag = (t0.position - t1.position).magnitude;

            float diff = currentMag - prevMag;

            _cam.orthographicSize -= diff * _zoomSpeed * Time.deltaTime;
            _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, _minZoom, _maxZoom);
        }

        // PC
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            _cam.orthographicSize -= scroll * (_zoomSpeed * 100f) * Time.deltaTime;
            _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, _minZoom, _maxZoom);
        }
    }

    void ClampCamera()
    {
        Vector3 pos = _targetPos;
        pos.x = Mathf.Clamp(pos.x, _minBounds.x, _maxBounds.x);
        pos.z = Mathf.Clamp(pos.z, _minBounds.y, _maxBounds.y);
        _targetPos = pos;
    }
}
