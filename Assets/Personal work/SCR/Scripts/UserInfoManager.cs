using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine;

public class UserInfoManager : Singleton<UserInfoManager>
{
    private UserData currentData;

    public async void SetUser(string uid)
    {
        currentData = await GetUserData(uid);
        if (currentData != null)
        {
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
    }

    public bool CanUseStar(int value)
    {
        if (currentData.UserInfo.Star - value > 0) return true;
        else return false;
    }

    public void UseStar(int value)
    {
        currentData.UserInfo.Star -= value;
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
            currentData.UserInfo.Heart.currentHeart++;
    }

    public void UseHeart()
    {
        if (currentData.UserInfo.Heart.currentHeart > 0)
            currentData.UserInfo.Heart.currentHeart--;
    }

    public void SetLeaveTime(string value)
    {
        currentData.UserInfo.Heart.lastSaveTime = value;
    }

    public string GetLeaveTime(string value)
    {
        return currentData.UserInfo.Heart.lastSaveTime;
    }

    public void SetHeartTime(int value)
    {
        currentData.UserInfo.Heart.remainingSeconds = value;
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

    public ItemInfo GetItem()
    {
        return currentData.ItemInfo;
    }

    public UserData GetCurrentUserData()
    {
        return currentData;
    }

    public async Task<UserData> GetUserData(string uid)
    {
        var task = Manager.DB.GetUserPath(uid).GetValueAsync();

        await task;

        if (task.IsFaulted)
        {
            Debug.LogError("데이터 로드 실패: " + task.Exception);
            return null;
        }

        DataSnapshot snapshot = task.Result;

        if (!snapshot.Exists)
        {
            Debug.LogWarning("해당 uid에 대한 사용자 데이터가 없습니다.");
            return null;
        }

        if (snapshot.Exists)
        {
            string json = snapshot.GetRawJsonValue();
            UserData userData = JsonUtility.FromJson<UserData>(json);

            return userData;
        }
        return null;
    }
}

[System.Serializable]
public class UserData
{
    public UserInfo UserInfo { get; set; } = new UserInfo();
    public int Stage { get; set; } = 1;
    public string PlayerName { get; set; } = "";
    public ItemInfo ItemInfo { get; set; } = new ItemInfo();
}

[System.Serializable]
public class UserInfo
{
    public HeartInfo Heart { get; set; } = new HeartInfo();
    public int Coin { get; set; } = 0;
    public int Star { get; set; } = 0;
    public int Profile { get; set; } = 0;
}

[System.Serializable]
public class HeartInfo
{
    public int currentHeart { get; set; } = 0;
    public string lastSaveTime { get; set; } = "";
    public int remainingSeconds { get; set; } = 0;
}

[System.Serializable]
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