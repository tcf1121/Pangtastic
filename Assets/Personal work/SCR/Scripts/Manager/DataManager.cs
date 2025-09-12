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

    protected override void Awake()
    {
        base.Awake();
        path = Path.Combine(Application.persistentDataPath, "userdata.json");
    }


    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Save();
        }
    }
#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        Save();
    }

#endif

    public void SetUser()
    {
        if (!IsReturningUser()) NewUser();
        else HistoryUser();
    }

    public bool IsReturningUser()
    {
        return File.Exists(path);
    }

    public void DeleteSaveData()
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("저장된 유저 데이터 삭제 완료");
        }
        else
        {
            Debug.Log("삭제할 데이터가 없습니다.");
        }
    }



    // 새로운 유저가 접속할 때 새로운 정보를 만듦
    public void NewUser()
    {
        var newUserData = new UserData();
        newUserData.PlayerName = "guest";
        newUserData.UserInfo.Heart.currentHeart = 5;
        newUserData.UserInfo.Heart.lastSaveTime = DateTime.Now.ToString("O");
        Manager.User.SetUser(newUserData);
        Manager.User.NewMissionList(16);
    }

    public void HistoryUser()
    {
        Manager.User.SetUser(Load());
    }

    public void Save()
    {
        string json = JsonConvert.SerializeObject(Manager.User.GetCurrentUserData());
        string encrypted = Crypto.Encrypt(json);
        File.WriteAllText(path, encrypted);
        Debug.Log("저장 완료: " + path);
    }

    public UserData Load()
    {
        if (File.Exists(path))
        {
            string encrypted = File.ReadAllText(path);
            string json = Crypto.Decrypt(encrypted);
            Debug.Log("저장된 데이터 확인");
            return JsonConvert.DeserializeObject<UserData>(json);
        }
        else
        {
            Debug.LogWarning("저장된 데이터 없음, 기본값 반환");
            return new UserData();
        }
    }
}
