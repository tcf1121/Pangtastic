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
    private FirebaseUser user = Manager.DB.auth.CurrentUser;

    private void Awake()
    {
        _tempToPermButton.onClick.AddListener(LinkToGPGS);

        if(user != null && user.IsAnonymous)
        {
            _tempToPermButton.gameObject.SetActive(true);
        }
        else
        {
            _tempToPermButton.gameObject.SetActive(false);
        }
    }

    private void LinkToGPGS()
    {
        user = Manager.DB.auth.CurrentUser;
        if (user == null)
        {
            Debug.LogError("유저가 없습니다.");
            return;
        }

        if (!user.IsAnonymous)
        {
            Debug.LogError("이미 영구계정 입니다.");
            return;
        }

        Manager.GPGS.LinkGuestToGoogle(() =>
        {
            _tempToPermButton.gameObject.SetActive(false);
        });
    }
}
