using Firebase.Auth;
using Firebase.Database;
using GooglePlayGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempToPerm : MonoBehaviour
{
    [SerializeField] Button _tempToPermButton;
    private FirebaseUser _user;

    private void Awake()
    {
        _user = Manager.DB.auth.CurrentUser;

        _tempToPermButton.onClick.AddListener(LinkToGPGS);

        if(_user != null && _user.IsAnonymous)
        {
            _tempToPermButton.gameObject.SetActive(true);
            _tempToPermButton.interactable = true;
        }
        else
        {
            _tempToPermButton.gameObject.SetActive(false);
        }
    }

    private void LinkToGPGS()
    {
        Debug.Log("마이그래이션 버튼눌림");

        _user = Manager.DB.auth.CurrentUser;

        if (_user == null)
        {
            Debug.LogError("유저가 없습니다.");
            return;
        }

        if (!_user.IsAnonymous)
        {
            Debug.LogError("이미 영구계정 입니다.");
            return;
        }

        _tempToPermButton.interactable = false;

        Manager.GPGS.LinkGuestToGoogle(success =>
        {
            if (success) //성공하면
            {
                _tempToPermButton.interactable = false; //버튼 숨김

            }
            else // 실패하면
            {
                _tempToPermButton.interactable = true; // 버튼 다시 활성화
            }
        });
    }
}
