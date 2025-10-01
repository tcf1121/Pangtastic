using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyQuestPanel : MonoBehaviour
{
    [SerializeField] List<RewardSO> rewards;
    [SerializeField] List<GameObject> levelCheckObj;
    [SerializeField] List<Button> rewardButtons;
    [SerializeField] List<GameObject> receivedObj;

    void Awake()
    {
        for (int i = 0; i < rewardButtons.Count; i++)
        {
            int index = i;
            rewardButtons[i].onClick.AddListener(() => GetReward(index));
        }
        SetWeeklyQuest();
    }

    private void SetWeeklyQuest()
    {
        for (int i = 0; i < levelCheckObj.Count; i++)
        {
            if (i < Manager.Date.WeekLevel) levelCheckObj[i].SetActive(true);
            else levelCheckObj[i].SetActive(false);
        }

        for (int i = 0; i < rewardButtons.Count; i++)
        {
            if (Manager.Date.Received[i])
            {
                receivedObj[i].SetActive(true);
                rewardButtons[i].interactable = false;
            }
            else
            {
                receivedObj[i].SetActive(false);
                rewardButtons[i].interactable = true;
            }
        }
    }

    private void GetReward(int index)
    {

        if (levelCheckObj[index].activeSelf && !Manager.Date.Received[index])
        {
            rewardButtons[index].interactable = false;
            rewards[index].AddReward();
            OutGameManager.ShowRewardPopup();
            receivedObj[index].SetActive(true);
            Manager.Date.SetReceived(index);
        }
    }
}
