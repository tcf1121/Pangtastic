using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Database : MonoBehaviour
{
    DatabaseReference reference;
    
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        
        // CreateData();
        // ReadData();
        // UpdateData();
        // DeleteData();
    }

    // void CreateData()
    // {
    //     reference.Child("users").Child("user01").SetRawJsonValueAsync("{\"name\":\"테스트\"}");
    // }
    //
    // void ReadData()
    // {
    //     reference.Child("users").Child("user01").GetValueAsync().ContinueWith(task =>
    //     {
    //         if (task.IsCompleted)
    //         {
    //             DataSnapshot snapshot = task.Result;
    //             Debug.Log("읽은 데이터: " + snapshot.GetRawJsonValue());
    //         }
    //     });
    // }
    //
    // void UpdateData()
    // {
    //     reference.Child("users").Child("user01").Child("age").SetValueAsync(30);
    // }
    //
    // void DeleteData()
    // {
    //     reference.Child("users").Child("user01").RemoveValueAsync();
    // }
}
