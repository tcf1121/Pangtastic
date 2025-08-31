using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class DatabaseSystem : MonoBehaviour
{
    public static DatabaseSystem Instance { get; private set; }
    
    [HideInInspector] public FirebaseAuth auth;
    [HideInInspector] public DatabaseReference dbRef;
   
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
       FirebaseDatabase.DefaultInstance.GoOnline();
       
        // firebase 초기화
        // auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        
       // WriteUserData();
       // ReadUserData();
    }
   
   // public void GetResultData()
   // {
   //     FirebaseDatabase.DefaultInstance.GetReference($"GetPlayerId :::  {GPGSManager.Instance.GetPlayerId()}")
   //         .GetValueAsync().ContinueWithOnMainThread(task =>
   //         {
   //             if (task.IsFaulted)
   //             {
   //                 // 실패
   //             }
   //             else if (task.IsCompleted)
   //             {
   //                 // 성공
   //                 DataSnapshot snapshot = task.Result;
   //                 for ( int i = 0; i < snapshot.ChildrenCount; i++)
   //                     Debug.Log(snapshot.Child(i.ToString()).Child("username").Value);
   //            
   //             }
   //         });
   // }
   
   public void UserIntoSave()
   {
       // dbRef.Child($"{GPGSManager.Instance.GetPlayerId()}").Child("users").Child("playerName").SetValueAsync($"{GPGSManager.Instance.GetPlayerName()}");
       dbRef.Child("users").Child($"{GPGSManager.Instance.GetPlayerId()}").Child("playerName").SetValueAsync($"{GPGSManager.Instance.GetPlayerName()}");
   }
}
