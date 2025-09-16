using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationPoint : MonoBehaviour
{
    [SerializeField] private DestinationType type;

    public DestinationType Type { get { return type; } }
}
