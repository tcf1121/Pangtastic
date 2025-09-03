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
            auth.CurrentUser.ReloadAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("유저 정보 리로드 실패 ");
                    return;
                }

                if (auth.CurrentUser != null && auth.CurrentUser.IsAnonymous)
                {
                    Debug.Log($"게스트 로그인 유지: {auth.CurrentUser.UserId}");
                }
                else
                {
                    Debug.Log("유저 없음");
                    auth.SignOut();

                    auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                        {
                            Debug.LogError("새로운 익명 계정로그인 실패");
                            return;
                        }

                        FirebaseUser newUser = task.Result.User;
                        Debug.Log($"새로운 익명 로그인 성공 : {newUser.UserId}");
                        string uid = newUser.UserId;

                        Manager.DB.GetUserPath(uid)
                        .Child("heart")
                        .Child("currentHeart")
                        .SetValueAsync(Manager.Heart.GetMaxHearts());

                        Manager.DB.GetUserPath(uid)
                        .Child("heart")
                        .Child("lastSaveTime")
                        .SetValueAsync(DateTime.Now.ToString("O"));

                        Manager.DB.GetUserPath(uid)
                        .Child("heart")
                        .Child("remainingSeconds")
                        .SetValueAsync(0);

                        Manager.DB.GetUserPath(uid)
                        .Child("playerName")
                        .SetValueAsync("Guest");
                    });
                }
            });
        }
        else
        {
            auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("익명 계정로그인 실패");
                    return;
                }

                FirebaseUser newUser = task.Result.User;
                Debug.Log($"익명 로그인 성공 : {newUser.UserId}");
                string uid = newUser.UserId;

                Manager.DB.GetUserPath(uid)
                .Child("heart")
                .Child("currentHeart")
                .SetValueAsync(Manager.Heart.GetMaxHearts());

                Manager.DB.GetUserPath(uid)
                .Child("heart")
                .Child("lastSaveTime")
                .SetValueAsync(DateTime.Now.ToString("O"));

                Manager.DB.GetUserPath(uid)
                .Child("heart")
                .Child("remainingSeconds")
                .SetValueAsync(0);

                Manager.DB.GetUserPath(uid)
                .Child("playerName")
                .SetValueAsync("Guest");
            });

        }
    }
}
