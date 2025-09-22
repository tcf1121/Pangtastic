using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    private string path;
    private bool _isNewData;
    private bool _isTest;
    private const string NewUserKey = "NewUserData";

    protected override void Awake()
    {
        base.Awake();
        string lastDateStr = PlayerPrefs.GetString(NewUserKey, "");
        _isNewData = string.IsNullOrEmpty(lastDateStr);
        _isTest = PlayerPrefs.GetInt("Test", 0) == 0 ? false : true;
        path = Path.Combine(Application.persistentDataPath, "userdata.json");
        Debug.Log($"userdata.json 주소 : {path}");
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

    public void OnTest()
    {
        _isTest = true;
        PlayerPrefs.SetInt("Test", 1);
    }

    public void OffTest()
    {
        _isTest = false;
        PlayerPrefs.SetInt("Test", 0);
    }

    public bool GetTest()
    {
        return _isTest;
    }

    public void SetUser()
    {
        if (_isNewData) NewUser();
        else HistoryUser();
    }

    public bool IsReturningUser()
    {
        return File.Exists(path);
    }

    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteAll();
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
        PlayerPrefs.SetString(NewUserKey, "newUser");
    }

    public void HistoryUser()
    {
        UserData loadData = Load();
        if (loadData != null)
            Manager.User.SetUser(Load());
        else
        {
            Debug.Log("데이터가 손상되었습니다.");
            NewUser();
        }
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
            Debug.Log(JsonConvert.DeserializeObject<UserData>(json));
            return JsonConvert.DeserializeObject<UserData>(json);
        }
        else
        {
            Debug.LogWarning("저장된 데이터 없음, 기본값 반환");
            return new UserData();
        }
    }
}

