using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    [SerializeField] TMP_Text _userID;

    private void Awake()
    {
        _userID.text = $"{GPGSManager.Instance.GetPlayerId()}";
    }
}
