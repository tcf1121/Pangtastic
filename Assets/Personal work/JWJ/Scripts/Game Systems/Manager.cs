using UnityEngine;

public static class Manager
{
    public static StageManager Stage => StageManager.GetInstance();
    public static UserInfoManager User => UserInfoManager.GetInstance();
    public static AudioManager Audio => AudioManager.GetInstance();

    public static ScriptingSystem Scripting => ScriptingSystem.GetInstance();

    //public static DataManager Data => DataManager.GetInstance();

    public static FirebaseManager DB => FirebaseManager.GetInstance();
    public static GPGSManager GPGS => GPGSManager.GetInstance();
    public static AdSystem Ad => AdSystem.GetInstance();

    public static EffectSystem Effect => EffectSystem.GetInstance();
    public static TimerManager Timer => TimerManager.GetInstance();

    public static IAPManager IAP => IAPManager.GetInstance();
    public static LanguageSystem Language => LanguageSystem.GetInstance();


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()  //인스펙터 세팅해야하는건 전부 첫씬으로 배치 (일단 주석처리 했음. 오브젝트 있는 씬부터 돈디스트로이 걸림)
    {
        StageManager.CreateManager();
        UserInfoManager.CreateManager();
        AudioManager.CreateManager();
        ScriptingSystem.CreateManager();

        //DataManager.CreateManager();
        FirebaseManager.CreateManager();
        GPGSManager.CreateManager();
        AdSystem.CreateManager();
        EffectSystem.CreateManager();
        TimerManager.CreateManager();
        IAPManager.CreateManager();

        LanguageSystem.CreateManager();
    }
}
