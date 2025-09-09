using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> lockedRooms;
    [SerializeField] private int curRoom;

    void Awake()
    {
        for (int i = 0; i < lockedRooms.Count; i++)
        {
            if (i <= curRoom) lockedRooms[i].SetActive(false);
        }

    }
}
