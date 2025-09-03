using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class DatabaseSystem : Singleton<DatabaseSystem>
{
    //public static DatabaseSystem Instance { get; private set; }

    [HideInInspector] public DatabaseReference dbRef;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log($"파ㅣ배 생성");
    }

    void Start()
    {
        FirebaseDatabase.DefaultInstance.GoOnline();

        // firebase 초기화
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public string GetUserRoot()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

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

    public void UserIntoSave()
    {
        string uid;
        string type;
        string nickname;

        if (string.IsNullOrEmpty(Manager.GPGS.GetPlayerName())) //닉네임이 비었으면
        {
            uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            if (string.IsNullOrEmpty(uid))
            {
                return;
            }

            type = "guests";
            nickname = "Guest"; // 게스트로 고정
        }
        else
        {
            uid = Manager.GPGS.GetPlayerId();

            if (string.IsNullOrEmpty(uid))
            {
                return;
            }
            type = "users";
            nickname = Manager.GPGS.GetPlayerName(); // 일반 로그인
        }

        // dbRef.Child($"{GPGSManager.Instance.GetPlayerId()}").Child("users").Child("playerName").SetValueAsync($"{GPGSManager.Instance.GetPlayerName()}");
        dbRef.Child($"{name}")
            .Child($"{type}")
            .Child("playerName")
            .SetValueAsync($"{nickname}");
        // GetUserPath(uid)
        //     .Child("playerName")
        //     .SetValueAsync(name);
        
    }
    
}
