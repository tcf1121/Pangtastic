using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterRotationUI : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject _character;
    [SerializeField] private float _rotationSpeed = 0.8f;

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
}
