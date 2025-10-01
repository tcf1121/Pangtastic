using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchivementObject : MonoBehaviour
{
    [SerializeField] TMP_Text _title;
    [SerializeField] TMP_Text _explane;
    [SerializeField] Slider _progressBar;
    [SerializeField] TMP_Text _progressValue;
    [SerializeField] Button _rewardButton;
    [SerializeField] GameObject _buttonText;
    [SerializeField] TMP_Text _coinText;
    [SerializeField] List<TMP_Text> _itemText;
    [SerializeField] AchivementSO rewards;
    [SerializeField] GameObject _completeText;
    [SerializeField] int _achivementIndex;
    private int _achvLevel;

    void Awake()
    {
        Manager.Language.ChangedLanguage += SetTitle;
        _rewardButton.onClick.AddListener(GetReward);
        CheckAllComplete();
    }

    void OnDestroy()
    {
        Manager.Language.ChangedLanguage -= SetTitle;
    }

    void OnEnable()
    {
        SetAchivement(Manager.User.GetAchievementLevel(_achivementIndex));
    }

    public void SetAchivement(int index)
    {
        _achvLevel = index;
        SetTitle();
        SetProgressBar(_achvLevel);
        SetReward(_achvLevel);
    }

    private void SetTitle()
    {
        _title.text = rewards.achivements[_achvLevel].Title.GetText(Manager.Language.GetLanguage());
        _explane.text = rewards.achivements[_achvLevel].Explane.GetText(Manager.Language.GetLanguage());
    }

    private void SetProgressBar(int index)
    {
        if (index == 0)
            _progressBar.minValue = 0;
        else
            _progressBar.minValue = rewards.achivements[index - 1].Target;

        _progressBar.maxValue = rewards.achivements[index].Target;
        int currentValue = Manager.User.GetAchievementProgress(_achivementIndex);
        _progressBar.value = Mathf.Clamp(currentValue, currentValue, rewards.achivements[index].Target);
        _progressValue.text = $"{_progressBar.value}/{_progressBar.maxValue}";
    }

    private void SetReward(int index)
    {
        _coinText.text = $"{rewards.achivements[index].reward.Rewards[0].Count}";
        foreach (var T in _itemText)
        {
            T.text = $"{rewards.achivements[index].reward.Rewards[1].Count}";
        }
    }

    private void GetReward()
    {
        if (_progressBar.value == _progressBar.maxValue)
        {
            rewards.achivements[_achivementIndex].reward.AddReward();
            Manager.User.CompleteAchievement(_achivementIndex);
            CheckAllComplete();
            OutGameManager.ShowRewardPopup();

        }
    }

    private void CheckAllComplete()
    {
        if (Manager.User.GetAchievementLevel(_achivementIndex) > rewards.achivements.Count)
        {
            _achvLevel = Manager.User.GetAchievementLevel(_achivementIndex);
            _title.text = rewards.achivements[_achvLevel - 1].Title.GetText(Manager.Language.GetLanguage());
            _explane.text = rewards.achivements[_achvLevel - 1].Explane.GetText(Manager.Language.GetLanguage());
            _progressBar.minValue = 0;
            _progressBar.maxValue = 1;
            _progressBar.value = 1;
            _progressValue.text = $"{Manager.User.GetAchievementProgress(_achivementIndex)}";
            SetReward(_achvLevel - 1);
            _buttonText.SetActive(false);
            _completeText.SetActive(true);
            _rewardButton.interactable = false;
        }
        else
        {
            SetAchivement(Manager.User.GetAchievementLevel(_achivementIndex));
            _buttonText.SetActive(true);
            _completeText.SetActive(false);
            _rewardButton.interactable = true;
        }
    }


}
