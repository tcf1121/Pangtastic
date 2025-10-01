using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private List<Place> _places;
    [SerializeField] private MenuUI menuUI;
    private MissionPlace curPlace;
    [SerializeField] MissionListSO missionList;
    [SerializeField] private MissionBtn missonBtn;
    [SerializeField] ScenarioManager _scenario;
    Action clearPlace;

    void Awake()
    {
        curPlace = Manager.User.GetCurPlace();
        for (int i = 0; i < _places.Count; i++)
        {
            if (i < (int)curPlace) _places[i].AllClear();
            else if (i == (int)curPlace) _places[i].CurrentClear();
        }

    }

    public void ClearPlace(Action openPopup = null)
    {
        if (curPlace <= MissionPlace.Greengrocery)
        {
            if (curPlace < MissionPlace.Greengrocery)
                curPlace++;
            Manager.User.SetCurPlace(curPlace);
            Manager.User.NewMissionList(GetMaxMission());
            Manager.User.ChapterClear();
            missonBtn.Refresh();
            menuUI.GoHome(() =>
            {
                menuUI.GoRoom(() =>
                {
                    _scenario.PlayScenario(curPlace, missonBtn.Percent);
                    openPopup?.Invoke();
                });
            });


        }

    }


    public void ClearMission(int index, Action openPopup = null)
    {
        Manager.User.ClearCurMisson(index);
        if (Manager.User.MissionAllClear())
        {
            _places[(int)curPlace].MissionClear(index, () => ClearPlace(openPopup));
        }
        else
        {
            _places[(int)curPlace].MissionClear(index, openPopup);
            missonBtn.Refresh();
        }
        Manager.User.PlaceFurniture();
        _scenario.PlayScenario(curPlace, missonBtn.Percent);
    }

    public int GetMaxMission()
    {
        return missionList.Missions[(int)curPlace].Mission.Count;
    }

    public List<Mission> CreateMission(int num = 1)
    {
        if (num == 0) return null;
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

public enum MissionPlace
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
