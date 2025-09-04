using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoManager : MonoBehaviour
{
    [SerializeField] Image profileImage;
    [SerializeField] TMP_Text _coinText;
    [SerializeField] TMP_Text _heartText;
    [SerializeField] TMP_Text _leftTime;
    [SerializeField] TMP_Text _sartText;

    [SerializeField] private int _startSeconds = 1800; // 시작 시간 (기본 30분, 초 단위)
    [SerializeField] private int _curStage;
    [SerializeField] private int _maxHearts = 5; // 최대 하트 개수
    [SerializeField] private float _currentHearts = 0; // 현재 하트 개수



    private IEnumerator GetUserInfo()
    {
        string authJson = Manager.DB.GetAuthInfo();
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);

        var task = Manager.DB.dbRef.Child(info.type)
            .Child(info.uid)
            .Child("UserInfo")
            .GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted);


        if (task.Exception != null)
        {
            Debug.LogError("유저 정보 불러오기 실패: " + task.Exception);
            yield break;
        }

    }
}
