using System.Collections.Generic;
using UnityEngine;

public class MissionPopup : MonoBehaviour
{
    [SerializeField] RoomManager _roomManager;
    [SerializeField] List<MissionList> _missionLists;

    void Awake()
    {
        OpenTwoMission();
    }

    private void OpenTwoMission()
    {
        List<Mission> twomission = _roomManager.CreateMission(2);
        for (int i = 0; i < twomission.Count; i++)
        {
            if (GetMissionList() != null)
            {
                GetMissionList().SetMission(twomission[i]);
            }
        }

    }

    private MissionList GetMissionList()
    {
        foreach (var mission in _missionLists)
            if (!mission.gameObject.activeSelf)
                return mission;
        return null;
    }

    private void SetMission()
    {
        if (GetMissionList() != null)
        {
            GetMissionList().SetMission(_roomManager.CreateMission()[0]);
        }
    }

    public void ClearMission(int index)
    {
        _roomManager.ClearMission(index);
        SetMission();
    }
}
