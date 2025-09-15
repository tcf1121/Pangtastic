using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseManager : Singleton<FirebaseManager>
{
    public FirebaseAuth auth;
    public FirebaseUser user;
    public DatabaseReference dbRef;

    protected override void Awake()
    {
        base.Awake();
        auth = FirebaseAuth.DefaultInstance;
        user = FirebaseAuth.DefaultInstance.CurrentUser;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private async void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            bool isGuest = user.IsAnonymous;
            if (!isGuest)
                await UploadUserData();
        }
    }
#if UNITY_EDITOR
    private async void OnApplicationQuit()
    {
        bool isGuest = user.IsAnonymous;
        if (!isGuest)
            await UploadUserData();
    }

#endif

    public string GetUserRoot()
    {
        if (auth.CurrentUser != null && auth.CurrentUser.IsAnonymous) // 익명 여부 확인
        {
            return "guests"; // 익명
        }
        return "users"; // 일반 로그인
    }

    public string GetUid()
    {
        return user != null ? user.UserId : null;
    }

    public DatabaseReference GetUserPath(string uid)
    {
        return dbRef.Child("Users").Child(uid);
    }

    private string GetProviderIdentifier()
    {
        if (user != null)
        {
            return user.DisplayName;
        }
        return null;
    }

    public async Task InitFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            Debug.Log("Firebase Auth 초기화 완료");
            if (IsOnline())
            {
                if (auth.CurrentUser != null)
                {
                    user = auth.CurrentUser;
                    Debug.Log("기존 유저 로그인 유지, UID: " + user.UserId);
                }
                else
                {
                    await SignInAnonymously();
                }
            }
            else
            {
                Debug.Log("인터넷 연결 없음 → 로컬 데이터만 사용");
            }
        }
        else
        {
            Debug.LogError($"Firebase 초기화 실패: {dependencyStatus}");
        }
    }

    private async Task SignInAnonymously()
    {
        var task = auth.SignInAnonymouslyAsync();
        await task;

        if (task.Exception != null)
        {
            Debug.LogError("익명 로그인 실패: " + task.Exception);
            return;
        }

        user = task.Result.User;
        Debug.Log("Firebase 로그인 성공, UID: " + user.UserId);
    }

    public bool IsOnline()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public async Task DownloadUserData()
    {

        var task = Manager.DB.GetUserPath(GetProviderIdentifier()).GetValueAsync();

        await task;

        if (task.IsFaulted)
        {
            Debug.LogError("데이터 로드 실패: " + task.Exception);
            return;
        }
        if (task.IsCanceled)
        {
            Debug.LogError("데이터 로드 취소: " + task.Exception);
            return;
        }

        DataSnapshot snapshot = task.Result;

        if (!snapshot.Exists)
        {
            Debug.LogWarning("해당 uid에 대한 사용자 데이터가 없습니다.");
            return;
        }
        else if (snapshot.Exists)
        {
            string json = snapshot.GetRawJsonValue();
            UserData userData = JsonConvert.DeserializeObject<UserData>(json);
            Debug.Log(userData.Stage);
            Manager.User.SetUser(userData);
        }
    }

    // 파이어베이스에 업로드
    public async Task UploadUserData()
    {
        Manager.User.SetLeaveTime(DateTime.Now.ToString("O"));
        string json = JsonConvert.SerializeObject(Manager.User.GetCurrentUserData());

        var task = dbRef.Child("users").Child(GetProviderIdentifier()).SetRawJsonValueAsync(json);
        await task;

        if (task.Exception != null)
        {
            Debug.LogError("Realtime Database 업로드 실패: " + task.Exception);
        }
        else
        {
            Debug.Log("Realtime Database 업로드 성공");
        }
    }
}
