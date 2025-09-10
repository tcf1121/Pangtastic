using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> lockedRooms;
    private MissonPlace curPlace;
    [SerializeField] private MissIonBtn missonBtn;
    [SerializeField] private List<int> maxMissionNum;

    void Awake()
    {
        curPlace = Manager.User.GetCurPlace();
        for (int i = 0; i < lockedRooms.Count; i++)
        {
            if (i <= (int)curPlace) lockedRooms[i].SetActive(false);
        }

    }

    public void ClearPlace()
    {
        if (curPlace < MissonPlace.Greengrocery)
            curPlace++;
        Manager.User.SetCurPlace(curPlace);
        Manager.User.NewMissionList(maxMissionNum[0]);
        lockedRooms[(int)curPlace].SetActive(true);
        missonBtn.Refresh();
    }

    public void ClearMission(int index)
    {

    }

    public int GetMaxMission()
    {
        return maxMissionNum[(int)curPlace];
    }
}

public enum MissonPlace
{
    Donut,
    MiniCafe,
    Cafe,
    IceCream,
    Bakery,
    Pizzeria,
    Bar,
    Greengrocery
}
