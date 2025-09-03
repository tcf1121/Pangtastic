using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class DatabaseSystem : MonoBehaviour
{
    [HideInInspector] public static DatabaseSystem Instance { get; private set; }
    [HideInInspector] public FirebaseUser user;
    [HideInInspector] public DatabaseReference dbRef;
    [HideInInspector] public FirebaseAuth auth;
    
    [System.Serializable]
    public class AuthInfo
    {
        public string uid;
        public string type;
        public string nickname;
    }

    protected void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        auth = FirebaseAuth.DefaultInstance; 
        user = FirebaseAuth.DefaultInstance.CurrentUser;
        
    }

    void Start()
    {
        FirebaseDatabase.DefaultInstance.GoOnline();

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
        string uid = user.UserId;
        string type;
        string nickname;
        
        if (user == null || string.IsNullOrEmpty(user.UserId))
        {
            return "{}"; // 로그인 안 된 경우 빈 JSON
        }

        if (user.IsAnonymous) // 게스트
        {
            type = "guests";
            nickname = "Guest";
        }
        else
        {
            type = "users";
            nickname = string.IsNullOrEmpty(user.DisplayName) ? "Unknown" : user.DisplayName;
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
            .Child("playerName")
            .SetValueAsync(info.nickname);
    }
    
}
