using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private List<Place> _places;
    private MissonPlace curPlace;
    [SerializeField] MissionListSO missionList;
    [SerializeField] private MissionBtn missonBtn;

    void Awake()
    {
        curPlace = Manager.User.GetCurPlace();
        for (int i = 0; i < _places.Count; i++)
        {
            if (i < (int)curPlace) _places[i].AllClear();
            else if (i == (int)curPlace) _places[i].CurrentClear();
        }

    }

    public void ClearPlace()
    {
        if (curPlace < MissonPlace.Greengrocery)
        {
            curPlace++;
            Manager.User.SetCurPlace(curPlace);
            Manager.User.NewMissionList(GetMaxMission());
            missonBtn.Refresh();
        }
    }

    public void ClearMission(int index)
    {
        Manager.User.ClearCurMisson(index);
        _places[(int)curPlace].MissionClear(index);
        missonBtn.Refresh();
    }

    public int GetMaxMission()
    {
        return missionList.Missions[(int)curPlace].Mission.Count;
    }

    public List<Mission> CreateMission(int num = 1)
    {
        List<Mission> returnMission = new();
        List<bool> isclearMission = Manager.User.GetCurMisson().ToList();
        List<Mission> missionLists = missionList.Missions[(int)curPlace].Mission;
        for (int i = 0; i < isclearMission.Count; i++)
        {
            if (isclearMission[i] == false)
            {
                if (missionLists[i].Prerequisites == 0 ||
                isclearMission[missionLists[i].Prerequisites - 1])
                {
                    returnMission.Add(missionLists[i]);
                    if (returnMission.Count == num) break;
                }
            }
        }
        return returnMission;
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
