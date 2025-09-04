using UnityEngine;

public static class Manager
{
    public static StageManager Stage => StageManager.GetInstance();
    public static AudioSystem Audio => AudioSystem.GetInstance();
    public static ScriptingSystem Scripting => ScriptingSystem.GetInstance();
    public static DatabaseSystem DB => DatabaseSystem.GetInstance();
    public static GPGSManager GPGS => GPGSManager.GetInstance();
    public static AdSystem Ad => AdSystem.GetInstance();

    public static EffectSystem Effect => EffectSystem.GetInstance();
    public static CurrencySystem Currency => CurrencySystem.GetInstance();
    public static HeartSystem Heart => HeartSystem.GetInstance();


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()  //인스펙터 세팅해야하는건 전부 첫씬으로 배치 (일단 주석처리 했음. 오브젝트 있는 씬부터 돈디스트로이 걸림)
    {
        //StageManager.CreateManager();
        ////AudioSystem.CreateManager();
        ////ScriptingSystem.CreateManager();
        //DatabaseSystem.CreateManager();
        //GPGSManager.CreateManager();
        //AdSystem.CreateManager();
        //EffectSystem.CreateManager();
        ////CurrencySystem.CreateManager();
        ////HeartSystem.CreateManager();
    }
}
