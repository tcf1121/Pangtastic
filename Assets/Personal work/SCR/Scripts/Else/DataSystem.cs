using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataSystem : MonoBehaviour
{
    private string path;

    private void Awake()
    {
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
        // 기존 저장 데이터 확인
        var savedData = Load();

        var newUserData = new UserData();
        newUserData.PlayerName = "guest";
        newUserData.UserInfo.Heart.currentHeart = 5;
        newUserData.UserInfo.Heart.lastSaveTime = DateTime.Now.ToString("O");

        // 최초 한번만 실행 - 광고에서 사용하기 위해 추가
        // if (!string.IsNullOrEmpty(savedData.StartUtcDay))
        // {
        //     newUserData.StartUtcDay = savedData.StartUtcDay;
        // }
        // else
        // {
        //     newUserData.StartUtcDay = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss");
        // }

        // 접속 기록 남기기
        // 기존 로그 가져오기 (없으면 새 리스트)
        // string today = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss");
        // newUserData.Logs.AccessDays.Add(today);
        // Manager.User.IsStartGame = true;
        // newUserData.Logs.AccaesDay = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss");

        Manager.User.SetUser(newUserData);
        Manager.User.NewMissionList(16);
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

