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
    public Action<float> OnInfinityHeart;

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
        if (currentData.UserInfo.Coin > 10000000)
            currentData.UserInfo.Coin = 10000000;
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

    public void InfinityHeart(float time, bool finish = false)
    {
        if (!finish)
        {
            currentData.UserInfo.Heart.currentHeart = 6;
            OnInfinityHeart?.Invoke(time);
        }
        else
        {
            currentData.UserInfo.Heart.currentHeart = 5;
            OnChangedHeart?.Invoke(currentData.UserInfo.Heart.currentHeart);
        }
    }

    public void AddHeart(int index = 1)
    {
        if (currentData.UserInfo.Heart.currentHeart == 6) return;
        if (currentData.UserInfo.Heart.currentHeart < 5)
        {
            currentData.UserInfo.Heart.currentHeart += index;
            if (GetHeart() > 5) currentData.UserInfo.Heart.currentHeart = 5;
            if (GetHeart() == 5) SetHeartTime(0);
        }
        OnChangedHeart?.Invoke(currentData.UserInfo.Heart.currentHeart);
    }

    public void UseHeart()
    {
        if (currentData.UserInfo.Heart.currentHeart == 6) return;
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
        if (item == ItemType.Roller)
        {
            currentData.ItemInfo.Roller += num;
            if (currentData.ItemInfo.Roller >= 99)
                currentData.ItemInfo.Roller = 99;
        }
        else if (item == ItemType.DonutBox)
        {
            currentData.ItemInfo.DonutBox += num;
            if (currentData.ItemInfo.DonutBox >= 99)
                currentData.ItemInfo.DonutBox = 99;
        }
        else if (item == ItemType.Oven)
        {
            currentData.ItemInfo.Oven += num;
            if (currentData.ItemInfo.Oven >= 99)
                currentData.ItemInfo.Oven = 99;
        }
        else if (item == ItemType.Whisk)
        {
            currentData.ItemInfo.Whisk += num;
            if (currentData.ItemInfo.Whisk >= 99)
                currentData.ItemInfo.Whisk = 99;
        }
        else if (item == ItemType.Scissors)
        {
            currentData.ItemInfo.Scissors += num;
            if (currentData.ItemInfo.Scissors >= 99)
                currentData.ItemInfo.Scissors = 99;
        }
        else if (item == ItemType.DonutPan)
        {
            currentData.ItemInfo.DonutPan += num;
            if (currentData.ItemInfo.DonutPan >= 99)
                currentData.ItemInfo.DonutPan = 99;
        }
        else
        {
            currentData.ItemInfo.Coffee += num;
            if (currentData.ItemInfo.Coffee >= 99)
                currentData.ItemInfo.Coffee = 99;
        }
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

    public void SetCurPlace(MissionPlace curPlace)
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

    public MissionPlace GetCurPlace()
    {
        return (MissionPlace)currentData.PlaceInfo.CurPlace;
    }

    public void ClearCurMisson(int index)
    {
        currentData.PlaceInfo.CurMisson[index - 1] = true;
    }

    public bool MissionAllClear()
    {
        foreach (var tf in currentData.PlaceInfo.CurMisson)
            if (!tf) return false;
        return true;
    }

    public bool[] GetCurMisson()
    {
        return currentData.PlaceInfo.CurMisson;
    }

    public int[] GetCat()
    {
        int[] cat = new int[2];
        cat[0] = currentData.UserInfo.Body;
        cat[1] = currentData.UserInfo.Face;

        return cat;
    }

    public void SetCat(int body, int face)
    {
        currentData.UserInfo.Body = body;
        currentData.UserInfo.Face = face;
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
    public int Body { get; set; } = 0;
    public int Face { get; set; } = 0;
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