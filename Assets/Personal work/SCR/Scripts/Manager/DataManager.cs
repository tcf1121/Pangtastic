using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    private string path;
    private string backupPath;
    private bool _isNewData;
    private bool _isTest;
    private const string NewUserKey = "NewUserData";
    private const string InstallIDKey = "InstallID"; // 재설치 감지용

    protected override void Awake()
    {
        base.Awake();
        path = Path.Combine(Application.persistentDataPath, "userdata.json");
        backupPath = path + ".bak";

        // [1] 재설치 감지
        string installID = PlayerPrefs.GetString(InstallIDKey, "");
        if (string.IsNullOrEmpty(installID))
        {
            // 완전 새 설치로 판단
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("재설치 감지 → 이전 userdata.json 삭제 완료");
            }

            PlayerPrefs.SetString(InstallIDKey, Guid.NewGuid().ToString());
            PlayerPrefs.Save();
        }

        // [2] 신규 유저 여부 판단
        string lastDateStr = PlayerPrefs.GetString(NewUserKey, "");
        _isNewData = string.IsNullOrEmpty(lastDateStr);

        _isTest = PlayerPrefs.GetInt("Test", 0) == 1;
        Debug.Log($"userdata.json 경로 : {path}");
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SafeSave();
            PlayerPrefs.Save();
        }
    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        SafeSave();
        PlayerPrefs.Save();
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

    public bool GetTest() => _isTest;

    public void SetUser()
    {
        // PlayerPrefs는 사라졌는데 파일이 남아있는 경우 (백업 복원)
        if (_isNewData && File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("PlayerPrefs 초기화 + 파일 남음 → 구버전 데이터 삭제");
        }

        if (_isNewData)
            NewUser();
        else
            HistoryUser();
    }

    public bool IsReturningUser() => File.Exists(path);

    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteAll();
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(backupPath)) File.Delete(backupPath);
        Debug.Log("저장 데이터 완전 삭제 완료");
    }

    // 신규 유저 생성
    public void NewUser()
    {
        var newUserData = new UserData
        {
            PlayerName = "guest"
        };

        newUserData.UserInfo.Heart.currentHeart = 5;
        newUserData.UserInfo.Heart.lastSaveTime = DateTime.Now.ToString("O");

        Manager.User.SetUser(newUserData);
        Manager.User.NewMissionList(16);
        PlayerPrefs.SetString(NewUserKey, "newUser");
        PlayerPrefs.Save();
        Manager.Date.NewUser();
    }

    // 기존 유저 로드
    public void HistoryUser()
    {
        UserData loadData = Load();

        if (loadData != null)
        {
            Manager.User.SetUser(loadData);
        }
        else
        {
            Debug.LogWarning("데이터 손상 감지 → 백업 시도");

            // 기존 파일 백업
            if (File.Exists(path))
            {
                try
                {
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath); // 기존 백업 삭제
                    }
                    File.Move(path, backupPath);
                    Debug.Log($"손상된 데이터 백업 완료: {backupPath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"백업 실패: {e.Message}");
                }
            }

            NewUser();
        }
    }

    // 안전한 저장 방식 (임시 파일 + 교체)
    public void SafeSave()
    {
        try
        {
            var data = Manager.User?.GetCurrentUserData();
            if (data == null)
            {
                Debug.LogWarning("Save 스킵: UserData가 null");
                return;
            }

            string json = JsonConvert.SerializeObject(data);
            string encrypted = Crypto.Encrypt(json);

            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, encrypted);

            // 기존 파일을 백업 후 교체
            if (File.Exists(path))
                File.Copy(path, backupPath, true);

            File.Copy(tempPath, path, true);
            File.Delete(tempPath);

            Debug.Log("저장 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError("SafeSave 오류: " + ex.Message);
        }
    }

    // 데이터 로드
    public UserData Load()
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("저장된 데이터 없음");
            return null;
        }

        try
        {
            string encrypted = File.ReadAllText(path);
            if (string.IsNullOrEmpty(encrypted))
            {
                Debug.LogError("파일이 비어있음");
                return null;
            }

            string json = Crypto.Decrypt(encrypted);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("복호화 실패 (빈 json)");
                return null;
            }

            var data = JsonConvert.DeserializeObject<UserData>(json);
            if (data == null)
            {
                Debug.LogError("역직렬화 실패 (null 반환)");
                return null;
            }

            return data;
        }
        catch (Exception e)
        {
            Debug.LogError("Load 실패: " + e.Message);
            return null;
        }
    }
}
