using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterRotationUI : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject _character;
    [SerializeField] private float _rotationSpeed = 0.8f;

    [SerializeField] private Camera _cam;
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _minDistance = 1.5f;
    [SerializeField] private float _maxDistance = 2.3f;

    private bool _isDragging = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.touchCount >= 2) 
        { 
            _isDragging = false; 
            return; 
        }
        _isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging && (Input.touchCount == 1 || !Input.touchSupported))
        {
            float deltaX = eventData.delta.x;
            _character.transform.Rotate(Vector3.up, deltaX * -_rotationSpeed);
        }

    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 dir = (_cam.transform.position - _character.transform.position).normalized;

            float distance = Vector3.Distance(_cam.transform.position, _character.transform.position);

            distance -= scroll * _zoomSpeed;
            distance = Mathf.Clamp(distance, _minDistance, _maxDistance);

            _cam.transform.position = _character.transform.position + dir * distance;
        }

        if (Input.touchCount >= 2)
        {
            _isDragging = false;
        }

        if ( Input.touchCount == 2)
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);

            float prevDis = ((touch0.position - touch0.deltaPosition) - (touch1.position - touch1.deltaPosition)).magnitude;
            
            float curDis = (touch0.position - touch1.position).magnitude;

            float delta = (curDis - prevDis);
            ZoomByDelta(-delta * 0.002f);
        }
    }

    void ZoomByDelta(float delta)
    {
        Vector3 dir = (_cam.transform.position - _character.transform.position).normalized;
        float distance = Vector3.Distance(_cam.transform.position, _character.transform.position);
        distance += delta;
        distance = Mathf.Clamp(distance, _minDistance, _maxDistance);
        _cam.transform.position = _character.transform.position + dir * distance;
    }
}
