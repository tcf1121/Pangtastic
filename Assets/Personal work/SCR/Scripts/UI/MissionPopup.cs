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
    private Action OnPopup;

    void Awake()
    {
        Refresh();
        OnPopup += openPopup;
    }

    private void openPopup()
    {
        gameObject.transform.parent.gameObject.SetActive(true);
        Refresh();
    }

    private void Refresh()
    {
        _progressSlider.minValue = 0;
        _progressSlider.value = Array.FindAll(Manager.User.GetCurMisson(), n => n == true).ToList().Count;
        _progressSlider.maxValue = Manager.User.GetCurMisson().Length;
        _progressText.text = $"{Array.FindAll(Manager.User.GetCurMisson(), n => n == true).ToList().Count}/{_progressSlider.maxValue}";
        SetMission();
    }

    private MissionList GetMissionList()
    {
        foreach (var mission in _missionLists)
            if (!mission.gameObject.activeSelf)
                return mission;
        return null;
    }

    private void ClearMissionList()
    {
        foreach (var mission in _missionLists)
            if (mission.gameObject.activeSelf)
                mission.gameObject.SetActive(false);
    }

    private void SetMission()
    {
        List<Mission> missions = _roomManager.CreateMission(2);
        if (missions != null)
            for (int i = 0; i < missions.Count; i++)
            {
                if (GetMissionList() != null)
                {
                    GetMissionList().SetMission(missions[i]);
                }
            }
    }

    public void ClearMission(int index)
    {
        Manager.Audio.PlaySFX("Mission_Clear");
        gameObject.transform.parent.gameObject.SetActive(false);
        ClearMissionList();
        _roomManager.ClearMission(index, OnPopup);
    }
}
