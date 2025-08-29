using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using GooglePlayGames;
using UnityEngine;

public class DatabaseSystem : MonoBehaviour
{
    public static DatabaseSystem Instance { get; private set; }

    
    [System.Serializable]
    public class UserData
    {
        public string displayName;
        public string email;
        public string createdAt;
    
        public UserData(string name, string mail, string time)
        {
            displayName = name;
            email = mail;
            createdAt = time;
        }
    }
    
   FirebaseAuth auth;
   DatabaseReference dbRef;

   protected void Awake()
   {
       if (Instance != null && Instance != this)
       {
           Destroy(gameObject);
           return;
       }

       Instance = this;
       DontDestroyOnLoad(gameObject);
   }

   void Start()
   {
        // firebase 초기화
        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
       //  
       // TestWriteSampleData();
       // TestReadSampleData();
        
        CreateData();
        ReadData();
        // UpdateData();
        // DeleteData();
    }

    // /// <summary>
    // /// 데이터 베이스에 사용자가 없으면 추가
    // /// </summary>
    // public void CheckSignupUser()
    // {
    //      string serverAuthCode = ((PlayGamesPlatform)Social.Active).GetServerAuthCode();
    //     Debug.Log("ServerAuthCode: " + serverAuthCode);
    //     
    //     if (!string.IsNullOrEmpty(serverAuthCode))
    //     {
    //         // Firebase PlayGamesAuthProvider 사용
    //         Credential credential = PlayGamesAuthProvider.GetCredential(serverAuthCode);
    //     
    //         // Firebase API
    //         auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
    //         {
    //             if (task.IsCanceled)
    //             {
    //                 Debug.LogError("SignInWithCredentialAsync was canceled.");
    //                 return;
    //             }
    //             if (task.IsFaulted)
    //             {
    //                 Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
    //                 return;
    //             }
    //     
    //             FirebaseUser newUser = task.Result;
    //             Debug.LogFormat("User signed in successfully: {0} ({1})",
    //                 newUser.DisplayName, newUser.UserId);
    //             
    //             SaveUserData(newUser);
    //         });
    //     }
    //     else
    //     {
    //         Debug.LogError("ServerAuthCode가 비어 있음 → Firebase 연동 불가");
    //     }
    // }
    //
    //          /// <summary>
    //            /// 샘플 데이터 쓰기
    //            /// </summary>
    //            public void TestWriteSampleData()
    //            {
    //                string sampleKey = "sampleUser01";
    //         
    //                // 간단한 예제 JSON
    //                var sampleData = new
    //                {
    //                    name = "테스트유저",
    //                    score = 999,
    //                    lastLogin = System.DateTime.UtcNow.ToString("o")
    //                };
    //         
    //                string json = JsonUtility.ToJson(sampleData);
    //         
    //                dbRef.Child("test").Child(sampleKey).SetRawJsonValueAsync(json)
    //                    .ContinueWithOnMainThread(task =>
    //                    {
    //                        if (task.IsCompletedSuccessfully)
    //                        {
    //                            Debug.Log("샘플 데이터 저장 성공!");
    //                        }
    //                        else
    //                        {
    //                            Debug.LogError("샘플 데이터 저장 실패: " + task.Exception);
    //                        }
    //                    });
    //            }
        
    //        /// <summary>
    //        /// 샘플 데이터 읽기
    //        /// </summary>
    //        public void TestReadSampleData()
    //        {
    //            dbRef.Child("test").Child("sampleUser01").GetValueAsync()
    //                .ContinueWithOnMainThread(task =>
    //                {
    //                    if (task.IsCompletedSuccessfully)
    //                    {
    //                        DataSnapshot snapshot = task.Result;
    //                        Debug.Log("읽어온 데이터: " + snapshot.GetRawJsonValue());
    //                    }
    //                    else
    //                    {
    //                        Debug.LogError("샘플 데이터 읽기 실패: " + task.Exception);
    //                    }
    //                });
    //        }
    //
    // private void SaveUserData(FirebaseUser user)
    // {
    //     dbRef.Child("users").Child(user.UserId).GetValueAsync().ContinueWith(task =>
    //     {
    //         if (task.IsFaulted)
    //         {
    //             Debug.LogError("DB 조회 실패: " + task.Exception);
    //             return;
    //         }
    //
    //         DataSnapshot snapshot = task.Result;
    //         if (!snapshot.Exists)
    //         {
    //             // 데이터 없으면 신규 저장
    //             UserData userData = new UserData(
    //                 user.DisplayName ?? "Unknown",
    //                 user.Email ?? "",
    //                 System.DateTime.UtcNow.ToString("o")
    //             );
    //
    //             string json = JsonUtility.ToJson(userData);
    //             Debug.Log("저장할 JSON: " + json);
    //             
    //             dbRef.Child("users").Child(user.UserId).SetRawJsonValueAsync(json)
    //                 .ContinueWith(saveTask =>
    //                 {
    //                     if (saveTask.IsCompletedSuccessfully)
    //                         Debug.Log("신규 유저 데이터 저장 완료");
    //                     else
    //                         Debug.LogError("유저 데이터 저장 실패: " + saveTask.Exception);
    //                 });
    //         }
    //         else
    //         {
    //             Debug.Log("기존 유저 데이터 있음 → 저장 스킵");
    //         }
    //     });
    // }
    //
    void CreateData()
    {
        dbRef.Child("users").Child("user01").SetRawJsonValueAsync("{\"name\":\"테스트\"}");
    }
    
    void ReadData()
    {
        dbRef.Child("users").Child("user01").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("읽은 데이터: " + snapshot.GetRawJsonValue());
            }
        });
    }
    
    void UpdateData()
    {
        dbRef.Child("users").Child("user01").Child("age").SetValueAsync(30);
    }
    
    // void DeleteData()
    // {
    //     dbRef.Child("users").Child("user01").RemoveValueAsync();
    // }
}
