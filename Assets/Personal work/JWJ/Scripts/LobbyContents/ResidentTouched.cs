using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResidentTouched : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private float _maxDistance = 80f;
    [SerializeField] private LayerMask _mask = ~0;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Vector3 screenPos = Input.mousePosition;
            Ray ray = _cam.ScreenPointToRay(screenPos);
            RaycastHit hit;

            bool hitSomething = Physics.Raycast(ray, out hit, _maxDistance, _mask, QueryTriggerInteraction.Collide);

            if (hitSomething == false)
            {
                return;
            }

            GameObject target = hit.collider.gameObject;
            ResidentController resident = target.GetComponent<ResidentController>();

            if (resident == null)
            {
                return;
            }
            resident.OnTouched();
        }
    }
}