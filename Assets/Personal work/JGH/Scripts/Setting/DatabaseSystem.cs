using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using GooglePlayGames;
using UnityEngine;
using UnityEngine.AI;

public class DatabaseSystem : MonoBehaviour
{
    //[HideInInspector] public static DatabaseSystem Instance { get; private set; }
    [HideInInspector] public FirebaseUser user;
    [HideInInspector] public DatabaseReference dbRef;
    [HideInInspector] public FirebaseAuth auth;
    public static DatabaseSystem Instance;

    [System.Serializable]
    public class AuthInfo
    {
        public string uid;
        public string type;
        public string nickname;
    }

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = FirebaseAuth.DefaultInstance.CurrentUser;
    }

    void Start()
    {
        FirebaseDatabase.DefaultInstance.GoOnline();
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        // firebase 초기화
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public string GetUserRoot()
    {
        if (auth.CurrentUser != null && auth.CurrentUser.IsAnonymous) // 익명 여부 확인
        {
            return "guests"; // 익명
        }
        return "users"; // 일반 로그인
    }

    public DatabaseReference GetUserPath(string uid)
    {
        return dbRef.Child(GetUserRoot()).Child(uid);
    }

    public string GetAuthInfo()
    {
        FirebaseUser curUser = auth.CurrentUser;

        if (curUser == null || string.IsNullOrEmpty(curUser.UserId))
        {
            return "{}"; // 로그인 안 된 경우 빈 JSON
        }

        string uid = curUser.UserId;
        string type;
        string nickname;

        if (curUser.IsAnonymous) // 게스트
        {
            type = "guests";
            nickname = "Guest";
        }
        else
        {
            type = "users";
            Debug.Log($"유저 네임: {curUser.DisplayName}");
            nickname = string.IsNullOrEmpty(curUser.DisplayName) ? "Unknown" : curUser.DisplayName;
            Debug.Log($"최종 유저 네임: {nickname}");
        }

        AuthInfo info = new AuthInfo
        {
            uid = uid,
            type = type,
            nickname = nickname
        };


        return JsonUtility.ToJson(info, true);

    }

    public void UserIntoSave()
    {
        string authJson = GetAuthInfo();
        AuthInfo info = JsonUtility.FromJson<AuthInfo>(authJson);

        Debug.Log($"info.type : {info.type}, info.nickname : {info.nickname}, info.uid : {info.uid}");

        dbRef.Child(info.type)
            .Child(info.uid)
            .Child("PlayerName")
            .SetValueAsync(info.nickname);
    }

    public void MigrateGuestDataToUser(string uid)
    {
        Debug.Log("데이터 마이그레이션 시작");

        var guestRef = dbRef.Child("guests").Child(uid);
        var userRef = dbRef.Child("users").Child(uid);

        guestRef.GetValueAsync().ContinueWithOnMainThread(readTask =>
        {
            if (readTask.IsFaulted || readTask.IsCanceled)
            {
                Debug.LogError($"게스트 데이터 읽기 실패:{readTask.Exception}");
                return;
            }

            DataSnapshot snapshot = readTask.Result;
            if (!snapshot.Exists)
            {
                Debug.Log("옮길 게스트 데이터 없음");
                return;
            }

            userRef.SetValueAsync(snapshot.Value).ContinueWithOnMainThread(writeTask =>
            {
                if (writeTask.IsFaulted || writeTask.IsCanceled)
                {
                    Debug.LogError($"유저 데이터 저장 실패: {writeTask.Exception}");
                    return;
                }

                Debug.Log("데이터 users 경로로 복사 완료");

                guestRef.RemoveValueAsync().ContinueWithOnMainThread(removeTask =>
                {
                    if (removeTask.IsFaulted || removeTask.IsCanceled)
                    {
                        Debug.LogError($"게스트 데이터 삭제 실패:{removeTask.Exception}");
                        return;
                    }

                    Debug.Log("게스트 데이터 삭제 완료, 마이그레이션 성공");
                    ChangeNickname(PlayGamesPlatform.Instance.localUser.userName);
                    Manager.IAP.RefreshOwnership(); // 구매목록 새로고침
                });
            });
        });
    }

    public void ChangeNickname(string newName)
    {
        string authJson = GetAuthInfo();
        AuthInfo info = JsonUtility.FromJson<AuthInfo>(authJson);

        dbRef.Child(info.type)
            .Child(info.uid)
            .Child("PlayerName")
            .SetValueAsync(newName);

        Debug.Log($"바뀐 닉네임: {newName}");
    }
}
