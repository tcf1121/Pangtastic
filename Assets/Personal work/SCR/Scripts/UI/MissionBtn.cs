using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MissIonBtn : MonoBehaviour
{
    Button _missionBtn;
    [SerializeField] private RoomManager _roomManager;
    [SerializeField] TMP_Text _currentRoom;
    [SerializeField] TMP_Text _progressText;
    [SerializeField] Slider _progressSlider;

    void Awake()
    {
        _missionBtn = GetComponent<Button>();
        Refresh();
    }

    public void Refresh()
    {
        _currentRoom.text = $"{Enum.GetName(typeof(MissonPlace), Manager.User.GetCurPlace())}";
        _progressSlider.minValue = 0;
        _progressSlider.maxValue = Manager.User.GetCurMisson().Count;
        _progressSlider.value = Manager.User.GetCurMisson().FindAll(n => n == true).ToList().Count;
        _progressText.text = $"{Manager.User.GetCurMisson()}/{_progressSlider.maxValue}";
    }

}
