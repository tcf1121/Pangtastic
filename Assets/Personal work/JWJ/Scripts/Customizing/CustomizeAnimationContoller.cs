using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizeAnimationContoller : MonoBehaviour
{
    [SerializeField] Animator animator;
    private void OnEnable()
    {
        animator.SetTrigger("IsCustomizing");
    }

    private void OnDisable()
    {
        animator.ResetTrigger("IsCustomizing");
    }
}
