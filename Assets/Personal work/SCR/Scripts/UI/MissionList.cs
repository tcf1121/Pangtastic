using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionList : MonoBehaviour
{
    [SerializeField] TMP_Text _titleText;
    [SerializeField] TMP_Text _needStarText;
    [SerializeField] Button _useStarBtn;
    [SerializeField] private MissionPopup _missionPopup;
    private int _index;
    private int _needStar;

    public void SetMission(Mission mission)
    {
        _titleText.text = mission.Explane.value[(int)Manager.Language.GetLanguage()];
        _index = mission.MissionID;
        _needStar = mission.Star;
        _needStarText.text = $"{mission.Star}";
        _useStarBtn.onClick.AddListener(UseStar);
        transform.SetSiblingIndex(2);
        gameObject.SetActive(true);
    }
    public int GetIndex()
    {
        return _index;
    }

    private void UseStar()
    {
        if (Manager.User.CanUseStar(_needStar))
        {
            Manager.User.UseStar(_needStar);
            _missionPopup.ClearMission(_index);
            transform.SetSiblingIndex(0);
            gameObject.SetActive(false);
        }
    }
}
