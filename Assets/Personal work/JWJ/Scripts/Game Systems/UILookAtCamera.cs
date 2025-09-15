using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILookAtCamera : MonoBehaviour
{
    private Transform _cam;

    private void Start()
    {
        _cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + _cam.rotation * Vector3.forward, _cam.rotation * Vector3.up);
    }
}