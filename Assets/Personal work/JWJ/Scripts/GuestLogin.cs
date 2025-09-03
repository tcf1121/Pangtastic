using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuestLogin : MonoBehaviour
{
    [SerializeField] private Button _guestLoginButton;

    private void Awake()
    {
        _guestLoginButton.onClick.AddListener(OnGuestLoginButtonClicked);
    }

    private void OnDestroy()
    {
        _guestLoginButton.onClick.RemoveListener(OnGuestLoginButtonClicked);
    }

    public void OnGuestLoginButtonClicked()
    {
        LoginAsGuest();
    }

    private void LoginAsGuest()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            if (auth.CurrentUser.IsAnonymous)
            {
                Debug.Log($"이미 게스트 로그인 상태: {auth.CurrentUser.UserId}");
            }
            else
            {
                Debug.Log($"이미 일반 로그인 상태: {auth.CurrentUser.UserId}");
            }
            return;
        }


        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("익명 로그인 취소");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"익명 로그인 실패 : {task.Exception}");
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result.User;
            string uid = newUser.UserId;

            Debug.LogFormat($"게스트 로그인 성공 : {newUser.UserId}");

            DatabaseSystem.Instance.GetUserPath(uid)
            .Child("heart")
            .Child("currentHeart")
            .SetValueAsync(5);

            DatabaseSystem.Instance.GetUserPath(uid)
            .Child("heart")
            .Child("lastSaveTime")
            .SetValueAsync(DateTime.Now.ToString("O"));

            DatabaseSystem.Instance.GetUserPath(uid)
            .Child("heart")
            .Child("remainingSeconds")
            .SetValueAsync(0);

            DatabaseSystem.Instance.GetUserPath(uid)
            .Child("playerName")
            .SetValueAsync("Guest");

        });
    }
}
