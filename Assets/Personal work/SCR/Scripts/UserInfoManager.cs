using Firebase.Auth;
using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class UserInfoManager : Singleton<UserInfoManager>
{
    private UserData currentData;
    public Action<int> OnChangedHeart;
    public Action<int> OnChangedHeartTime;
    public Action<int> OnChangedStage;
    public Action<int> OnChangedStar;
    public Action<int> OnChangedCoin;
    public Action<int> OnChangedProfile;
    public Action OnUseHeart;

    public void SetUser(UserData user)
    {
        currentData = user;
    }

    // 새로운 유저가 접속할 때 새로운 정보를 만듦
    public async void NewUser(string uid, string now)
    {
        var newUserData = new UserData();
        newUserData.PlayerName = "guest";
        newUserData.UserInfo.Heart.currentHeart = 5;
        newUserData.UserInfo.Heart.lastSaveTime = now;

        string json = JsonConvert.SerializeObject(newUserData);
        try
        {
            await Manager.DB.GetUserPath(uid).SetRawJsonValueAsync(json);
        }
        catch (Exception ex)
        {
            Debug.LogError("유저 데이터 생성 실패: " + ex.Message);
        }
        currentData = newUserData;
    }

    // 기존 유저가 접속할 때 기존 데이터를 가져와서 CurrentData에 넣음
    public async Task SetUser(string uid)
    {
        currentData = await DownloadUserData(uid);
        if (currentData != null)
        {
            Debug.Log(currentData.Stage);
            Debug.Log("사용자 데이터 로드 및 할당 성공!");
        }
        else
        {
            Debug.LogError("사용자 데이터 로드 실패. currentData가 null입니다.");
        }
    }

    public int GetStage()
    {
        return currentData.Stage;
    }

    public void ClearStage()
    {
        currentData.Stage++;
    }

    public void SetStage(int value)
    {
        currentData.Stage = value;
    }

    public int GetStar()
    {
        return currentData.UserInfo.Star;
    }

    public void AddStar(int value)
    {
        currentData.UserInfo.Star += value;
        OnChangedStar?.Invoke(currentData.UserInfo.Star);
    }

    public bool CanUseStar(int value)
    {
        if (currentData.UserInfo.Star - value > 0) return true;
        else return false;
    }

    public void UseStar(int value)
    {
        currentData.UserInfo.Star -= value;
        OnChangedStar?.Invoke(currentData.UserInfo.Star);
    }

    public int GetCoin()
    {
        return currentData.UserInfo.Coin;
    }

    public void AddCoin(int value)
    {
        currentData.UserInfo.Coin += value;
        OnChangedCoin?.Invoke(currentData.UserInfo.Coin);
    }

    public bool CanUseCoin(int value)
    {
        if (currentData.UserInfo.Coin - value > 0) return true;
        else return false;
    }

    public void UseCoin(int value)
    {
        currentData.UserInfo.Coin -= value;
        OnChangedCoin?.Invoke(currentData.UserInfo.Coin);
    }


    public void SetHeart(int value)
    {
        currentData.UserInfo.Heart.currentHeart = value;
    }

    public int GetHeart()
    {
        return currentData.UserInfo.Heart.currentHeart;
    }

    public void AddHeart()
    {
        if (currentData.UserInfo.Heart.currentHeart < 5)
        {
            currentData.UserInfo.Heart.currentHeart++;
            if (GetHeart() == 5) SetHeartTime(0);
        }
        OnChangedHeart?.Invoke(currentData.UserInfo.Heart.currentHeart);
    }

    public void UseHeart()
    {
        if (currentData.UserInfo.Heart.currentHeart > 0)
        {
            currentData.UserInfo.Heart.currentHeart--;
            OnChangedHeart?.Invoke(currentData.UserInfo.Heart.currentHeart);
            OnUseHeart?.Invoke();
        }

    }

    public void SetLeaveTime(string value)
    {
        currentData.UserInfo.Heart.lastSaveTime = value;
    }

    public string GetLeaveTime()
    {
        return currentData.UserInfo.Heart.lastSaveTime;
    }

    public void SetHeartTime(int value)
    {
        currentData.UserInfo.Heart.remainingSeconds = value;
        OnChangedHeartTime?.Invoke(currentData.UserInfo.Heart.remainingSeconds);
    }

    public int GetHeartTime()
    {
        return currentData.UserInfo.Heart.remainingSeconds;
    }

    public bool CheckHeart()
    {
        if (currentData.UserInfo.Heart.currentHeart > 0) return true;
        else return false;
    }

    public void SetItem(ItemInfo itemInfo)
    {
        currentData.ItemInfo = itemInfo;
    }

    public void SetName(string name)
    {
        currentData.PlayerName = name;
    }

    public ItemInfo GetItem()
    {
        return currentData.ItemInfo;
    }

    public UserData GetCurrentUserData()
    {
        return currentData;
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

    private async void OnApplicationQuit()
    {
        await UploadUserDataAsync();
    }

    // 기기에 JSON 파일로 저장
    public void SaveUserData()
    {

    }

    // 파이어베이스에 업로드
    public async Task UploadUserDataAsync()
    {
        SetLeaveTime(DateTime.Now.ToString("O"));
        string json = JsonConvert.SerializeObject(currentData);
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

[Serializable]
public class UserData
{
    public UserInfo UserInfo { get; set; } = new UserInfo();
    public int Stage { get; set; } = 0;
    public string PlayerName { get; set; } = "";
    public ItemInfo ItemInfo { get; set; } = new ItemInfo();
}

[Serializable]
public class UserInfo
{
    public HeartInfo Heart { get; set; } = new HeartInfo();
    public int Coin { get; set; } = 0;
    public int Star { get; set; } = 0;
    public int Profile { get; set; } = 0;
}

[Serializable]
public class HeartInfo
{
    public int currentHeart { get; set; } = 0;
    public string lastSaveTime { get; set; } = "";
    public int remainingSeconds { get; set; } = 0;
}

[Serializable]
public class ItemInfo
{
    public int Roller { get; set; } = 0;
    public int DonutBox { get; set; } = 0;
    public int Oven { get; set; } = 0;
    public int Whisk { get; set; } = 0;
    public int Scissors { get; set; } = 0;
    public int DonutPan { get; set; } = 0;
    public int Coffee { get; set; } = 0;
}