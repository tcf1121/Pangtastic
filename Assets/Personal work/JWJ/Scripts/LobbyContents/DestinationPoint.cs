using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationPoint : MonoBehaviour
{
    [SerializeField] private DestinationType type;
    [SerializeField] private bool _showGizmos;
    [SerializeField] private float _radius = 0.6f;

    public DestinationType Type { get { return type; } }

    private void OnDrawGizmos()
    {
        if (_showGizmos)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
