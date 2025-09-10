using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionList : MonoBehaviour
{
    [SerializeField] TMP_Text _titleText;
    [SerializeField] TMP_Text _needStarText;
    [SerializeField] Button _useStarBtn;
    private RoomManager _roomManager;
    private int _index;
    private int _needStar;

    public void SetMission(int index, string title, int star, RoomManager roomManager)
    {
        _roomManager = roomManager;
        _titleText.text = title;
        _index = index;
        _needStar = star;
        _needStarText.text = $"{star}";
        _useStarBtn.onClick.AddListener(UseStar);
        transform.SetSiblingIndex(2);
        gameObject.SetActive(true);
    }

    private void UseStar()
    {
        if (Manager.User.CanUseStar(_needStar))
        {
            Manager.User.UseStar(_needStar);
            _roomManager.ClearMission(_index);
            transform.SetSiblingIndex(0);
            gameObject.SetActive(false);
        }
    }
}
