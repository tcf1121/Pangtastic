using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchivementObject : MonoBehaviour
{
    [SerializeField] TMP_Text _title;
    [SerializeField] Slider _progressBar;
    [SerializeField] TMP_Text _progressValue;
    [SerializeField] Button _rewardButton;
    [SerializeField] TMP_Text _buttonText;
    [SerializeField] TMP_Text _coinText;
    [SerializeField] List<TMP_Text> _itemText;
    [SerializeField] List<RewardSO> rewards;

    public void SetAchivement(int achv_level)
    {

    }
}
