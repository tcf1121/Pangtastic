using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCustomizing : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] ResidentController _controller;
    [SerializeField] Light _light;

    private float _prevLightIntensity;

    private Vector3 _prevPos;
    private Vector3 _customizePos = new Vector3(50f, 0.315f, -1.31f);

    private Quaternion _prevRotation;
    private Quaternion _customizeRotation = Quaternion.Euler(0f, 180f, 0f);

    private void Awake()
    {
        _prevLightIntensity = _light.intensity;
    }

    private void OnEnable()
    {
        _light.intensity = 0.7f;
        _prevPos = _controller.transform.position;
        _prevRotation = _controller.transform.rotation;

        animator.SetTrigger("IsCustomizing");
        _controller.StopMoving(_customizePos, _customizeRotation);
        
    }

    private void OnDisable()
    {
        _light.intensity = _prevLightIntensity;
        animator.ResetTrigger("IsCustomizing");
        _controller.ResumeMoving(_prevPos, _prevRotation);
    }
}
