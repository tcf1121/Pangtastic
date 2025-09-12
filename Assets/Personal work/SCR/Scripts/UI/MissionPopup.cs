using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionPopup : MonoBehaviour
{
    [SerializeField] RoomManager _roomManager;
    [SerializeField] List<MissionList> _missionLists;
    [SerializeField] TMP_Text _progressText;
    [SerializeField] Slider _progressSlider;

    void Awake()
    {
        OpenTwoMission();
        Refresh();
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

    private void Refresh()
    {
        _progressSlider.minValue = 0;
        _progressSlider.maxValue = Manager.User.GetCurMisson().Length;
        _progressSlider.value = Array.FindAll(Manager.User.GetCurMisson(), n => n == true).ToList().Count;
        _progressText.text = $"{Array.FindAll(Manager.User.GetCurMisson(), n => n == true).ToList().Count}/{_progressSlider.maxValue}";
        if (GetCurMissionList() == null)
            SetMission();
    }

    private MissionList GetMissionList()
    {
        foreach (var mission in _missionLists)
            if (!mission.gameObject.activeSelf)
                return mission;
        return null;
    }

    private MissionList GetCurMissionList()
    {
        foreach (var mission in _missionLists)
            if (mission.gameObject.activeSelf)
                return mission;
        return null;
    }

    private void SetMission()
    {
        if (GetMissionList() != null)
        {
            if (_roomManager.CreateMission()[0].MissionID == GetCurMissionList().GetIndex()) return;
            else GetMissionList().SetMission(_roomManager.CreateMission()[0]);
        }
    }

    public void ClearMission(int index)
    {
        _roomManager.ClearMission(index);
        SetMission();
        Refresh();
    }
}
