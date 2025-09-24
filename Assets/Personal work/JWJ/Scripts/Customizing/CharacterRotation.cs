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
        _isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging)
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
    }
}
