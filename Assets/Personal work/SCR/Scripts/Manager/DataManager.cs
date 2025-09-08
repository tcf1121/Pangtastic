using Firebase.Auth;
using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    private string path;
    private async void OnApplicationQuit()
    {
        await UploadUserDataAsync();
    }

    // 새로운 유저가 접속할 때 새로운 정보를 만듦
    public async Task NewUser(string uid, string now)
    {
        var newUserData = new UserData();
        newUserData.PlayerName = "guest";
        newUserData.UserInfo.Heart.currentHeart = 5;
        newUserData.UserInfo.Heart.lastSaveTime = now;

        string json = JsonConvert.SerializeObject(newUserData);
        try
        {
            await Manager.DB.GetUserPath(uid).SetRawJsonValueAsync(json);
            Manager.User.SetUser(newUserData);
        }
        catch (Exception ex)
        {
            Debug.LogError("유저 데이터 생성 실패: " + ex.Message);
        }
    }

    // 기존 유저가 접속할 때 기존 데이터를 가져와서 CurrentData에 넣음
    public async Task SetUser(string uid)
    {
        Manager.User.SetUser(await Manager.Data.DownloadUserData(uid));
        if (Manager.User.GetCurrentUserData() != null)
        {
            Debug.Log("사용자 데이터 로드 및 할당 성공!");
        }
        else
        {
            Debug.LogError("사용자 데이터 로드 실패. currentData가 null입니다.");
        }
    }

    public async Task<UserData> DownloadUserData(string uid)
    {
        var task = Manager.DB.GetUserPath(uid).GetValueAsync();

        await task;

        if (task.IsFaulted)
        {
            Debug.LogError("데이터 로드 실패: " + task.Exception);
            return null;
        }
        if (task.IsCanceled)
        {
            Debug.LogError("데이터 로드 취소: " + task.Exception);
            return null;
        }

        DataSnapshot snapshot = task.Result;

        if (!snapshot.Exists)
        {
            Debug.LogWarning("해당 uid에 대한 사용자 데이터가 없습니다.");
            return null;
        }
        else if (snapshot.Exists)
        {
            string json = snapshot.GetRawJsonValue();

            Debug.Log(json);
            UserData userData = JsonConvert.DeserializeObject<UserData>(json);
            Debug.Log(userData.Stage);

            return userData;
        }
        return null;
    }

    // 기기에 JSON 파일로 저장
    public void SaveUserData()
    {
        string json = JsonConvert.SerializeObject(Manager.User.GetCurrentUserData());

        File.WriteAllText(path, json);
    }

    // 파이어베이스에 업로드
    public async Task UploadUserDataAsync()
    {
        Manager.User.SetLeaveTime(DateTime.Now.ToString("O"));
        string json = JsonConvert.SerializeObject(Manager.User.GetCurrentUserData());
        try
        {
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            string uid = auth.CurrentUser.UserId;
            await Manager.DB.GetUserPath(uid).SetRawJsonValueAsync(json);
        }
        catch (Exception ex)
        {
            Debug.LogError("유저 데이터 생성 실패: " + ex.Message);
        }
    }
}
