using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class DatabaseSystem : MonoBehaviour
{
    public static DatabaseSystem Instance { get; private set; }
    
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
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }
   
   public void UserIntoSave()
   {
       // dbRef.Child($"{GPGSManager.Instance.GetPlayerId()}").Child("users").Child("playerName").SetValueAsync($"{GPGSManager.Instance.GetPlayerName()}");
       dbRef.Child("users")
           .Child($"{GPGSManager.Instance.GetPlayerId()}")
           .Child("playerName")
           .SetValueAsync($"{GPGSManager.Instance.GetPlayerName()}");
   }
}
