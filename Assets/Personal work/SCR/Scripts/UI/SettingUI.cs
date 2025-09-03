using TMPro;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    [SerializeField] TMP_Text _userID;

    private void Awake()
    {
        string authJson = DatabaseSystem.Instance.GetAuthInfo(); 
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson); 
        _userID.text = $"{info.uid}";
        // _userID.text = $"{GPGSManager.Instance.GetPlayerId()}";
    }
}
