using System;

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
        if (currentData.UserInfo.Star - value >= 0) return true;
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
        if (currentData.UserInfo.Coin - value >= 0) return true;
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

    public bool CanUseItem(ItemType item)
    {
        if (item == ItemType.Roller) return currentData.ItemInfo.Roller > 0;
        else if (item == ItemType.DonutBox) return currentData.ItemInfo.DonutBox > 0;
        else if (item == ItemType.Oven) return currentData.ItemInfo.Oven > 0;
        else if (item == ItemType.Whisk) return currentData.ItemInfo.Whisk > 0;
        else if (item == ItemType.Scissors) return currentData.ItemInfo.Scissors > 0;
        else if (item == ItemType.DonutPan) return currentData.ItemInfo.DonutPan > 0;
        else return currentData.ItemInfo.Coffee > 0;
    }

    public void UseItem(ItemType item)
    {
        if (item == ItemType.Roller) currentData.ItemInfo.Roller--;
        else if (item == ItemType.DonutBox) currentData.ItemInfo.DonutBox--;
        else if (item == ItemType.Oven) currentData.ItemInfo.Oven--;
        else if (item == ItemType.Whisk) currentData.ItemInfo.Whisk--;
        else if (item == ItemType.Scissors) currentData.ItemInfo.Scissors--;
        else if (item == ItemType.DonutPan) currentData.ItemInfo.DonutPan--;
        else currentData.ItemInfo.Coffee--;
    }

    public void AddItem(ItemType item, int num = 1)
    {
        if (item == ItemType.Roller) currentData.ItemInfo.Roller += num;
        else if (item == ItemType.DonutBox) currentData.ItemInfo.DonutBox += num;
        else if (item == ItemType.Oven) currentData.ItemInfo.Oven += num;
        else if (item == ItemType.Whisk) currentData.ItemInfo.Whisk += num;
        else if (item == ItemType.Scissors) currentData.ItemInfo.Scissors += num;
        else if (item == ItemType.DonutPan) currentData.ItemInfo.DonutPan += num;
        else currentData.ItemInfo.Coffee += num;
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

    public void SetCurPlace(MissonPlace curPlace)
    {
        currentData.PlaceInfo.CurPlace = (int)curPlace;
    }

    public void NewMissionList(int max)
    {
        currentData.PlaceInfo.CurMisson = new bool[max];
        for (int i = 0; i < max; i++)
        {
            currentData.PlaceInfo.CurMisson[i] = false;
        }
    }

    public MissonPlace GetCurPlace()
    {
        return (MissonPlace)currentData.PlaceInfo.CurPlace;
    }

    public void ClearCurMisson(int index)
    {
        currentData.PlaceInfo.CurMisson[index - 1] = true;
    }

    public bool[] GetCurMisson()
    {
        return currentData.PlaceInfo.CurMisson;
    }


}
public enum ItemType
{
    Roller,
    DonutBox,
    Oven,
    Whisk,
    Scissors,
    DonutPan,
    Coffee
}

[Serializable]
public class UserData
{
    public UserInfo UserInfo { get; set; } = new UserInfo();
    public int Stage { get; set; } = 0;
    public string PlayerName { get; set; } = "";
    public ItemInfo ItemInfo { get; set; } = new ItemInfo();
    public PlaceInfo PlaceInfo { get; set; } = new PlaceInfo();
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

[Serializable]
public class PlaceInfo
{
    public int CurPlace { get; set; } = 0;
    public bool[] CurMisson { get; set; } = new bool[16];
}