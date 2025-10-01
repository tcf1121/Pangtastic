using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MissionBtn : MonoBehaviour
{
    Button _missionBtn;
    [SerializeField] private RoomManager _roomManager;
    [SerializeField] TMP_Text _currentRoom;
    [SerializeField] TMP_Text _progressText;
    [SerializeField] Slider _progressSlider;
    [SerializeField] List<StringSO> placeString;
    public float Percent { get { return _percent; } }
    private float _percent;

    void Awake()
    {
        _missionBtn = GetComponent<Button>();
        Refresh();
    }

    public void Refresh()
    {
        string place = placeString[(int)Manager.User.GetCurPlace()].GetText(Manager.Language.GetLanguage());
        _currentRoom.text = $"{place}";
        _progressSlider.minValue = 0;
        _progressSlider.maxValue = Manager.User.GetCurMisson().Length;
        // _progressSlider.value = Array.FindAll(Manager.User.GetCurMisson(), n => n == true).ToList().Count;
        // _progressText.text = $"{Array.FindAll(Manager.User.GetCurMisson(), n => n == true).ToList().Count}/{_progressSlider.maxValue}";
        // _percent = (float)Array.FindAll(Manager.User.GetCurMisson(), n => n == true).ToList().Count / _progressSlider.maxValue;
        // TODO: TEST
        _progressSlider.value = Array.FindAll(Manager.UserInfoSystem.GetCurMisson(), n => n == true).ToList().Count;
        _progressText.text = $"{Array.FindAll(Manager.UserInfoSystem.GetCurMisson(), n => n == true).ToList().Count}/{_progressSlider.maxValue}";
        _percent = (float)Array.FindAll(Manager.UserInfoSystem.GetCurMisson(), n => n == true).ToList().Count / _progressSlider.maxValue;
    }

}
