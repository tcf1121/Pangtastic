using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class ScriptingSystem : Singleton<ScriptingSystem>
{
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerNameText;

    [Header("Speaker Images")]
    [SerializeField] private Sprite  centerImage;
    [SerializeField] private Sprite   leftImage;
    [SerializeField] private Sprite   rightImage;
    
    [SerializeField] private TMP_FontAsset defaultFont; // 기본 폰트 (예: 한국어)
    [SerializeField] private TMP_FontAsset chineseFont; // 중국어용 폰트
    [SerializeField] private TMP_FontAsset englishFont; // 영어용 폰트
    
    private List<DialogLine> currentDialogList = new List<DialogLine>();
    private int currentIndex = 0; // 현재 대사 위치

    [Serializable]
    public class DialogData
    {
        public int dialogId;
        public int dialogGroup;
        public int order;

        public string centerSpeakerId;
        public string leftSpeakerId;
        public string rightSpeakerId;
        public string dialogStringId;
        public string spritePath;
        public string bgmSoundId;
        public string sfxSoundId;
    }
    
    [Serializable]
    public class StringData 
    {
        public string stringId;
        public string korean;
        public string english;
        public string chinese;
    }
    
    [Serializable]
    public class SpeakerData
    {
        public string speakerId;
        public string nameStringId;
        public string spritePath;
        public string description;
    }

    [System.Serializable]
    public class DialogLine
    {
        public string script;
        public string centerCharacter;
        public string leftCharacter;
        public string rightCharacter;
        public string bgm;
        public bool bgmLoop;
        public float bgmVolume;
        public string sfx;
        public bool sfxLoop;
        public float sfxVolume;
        public string backgroundIgm;
        public string centerSpeakerImgPath;
        public string leftSpeakerImgPath;
        public string rightSpeakerImgPath;
        public string rightSpeakerOnOff;
        public string centerSpeakerOnOff;
        public string leftSpeakerOnOff;
    }

    [System.Serializable]
    public class SoundData
    {
        public string soundId;
        public string name;
        public string filePath;
        public float volume;
        public bool loopCheck;
        public string description;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        // 마우스 왼쪽 클릭 시
        if (Input.GetMouseButtonDown(0))
        {
            ShowNextDialogue();
        }
    }

    private List<DialogData> dialogAllDialogs = new List<DialogData>();
    private List<StringData> stringAllDialogs = new List<StringData>();
    private List<SpeakerData> speakerAllDialogs = new List<SpeakerData>();
    private List<SoundData> soundAllDialogs = new List<SoundData>();
    private string dialogName = "dialog.bytes";
    private string stringName = "string.bytes";
    private string speakerName = "speaker.bytes";
    private string soundName = "sound.bytes";
    
    // void Start()
    // {
    //     StartCoroutine(DialogLoadCSV(6002));
    // }

    public IEnumerator DialogLoadCSV(int id)
    {
        
        string dialogPath = Path.Combine(Application.streamingAssetsPath, dialogName);
        string stringPath = Path.Combine(Application.streamingAssetsPath, stringName);
        string speakerPath = Path.Combine(Application.streamingAssetsPath, speakerName);
        string soundPath = Path.Combine(Application.streamingAssetsPath, soundName);
        
        string langCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        
        string dialogCsvText;
        string stringCsvText;
        string speakerCsvText;
        string soundCsvText;

        #if UNITY_ANDROID && !UNITY_EDITOR
                UnityWebRequest dialogWww = UnityWebRequest.Get(dialogPath);
                UnityWebRequest stringWww = UnityWebRequest.Get(stringPath);
                UnityWebRequest speakerWww = UnityWebRequest.Get(speakerPath);
                UnityWebRequest soundWww = UnityWebRequest.Get(soundPath);
                
                yield return dialogWww.SendWebRequest();
                yield return stringWww.SendWebRequest();
                yield return speakerWww.SendWebRequest();
                yield return soundWww.SendWebRequest();

               if (dialogWww.result == UnityWebRequest.Result.Success &&
                    stringWww.result == UnityWebRequest.Result.Success &&
                    soundWww.result == UnityWebRequest.Result.Success &&
                    speakerWww.result == UnityWebRequest.Result.Success)
                {
                    dialogCsvText = CryptoUtility.TextDecryptFromBytes(dialogWww.downloadHandler.data);
                    dialogAllDialogs = DialogParseCSV(dialogCsvText);

                    stringCsvText = CryptoUtility.TextDecryptFromBytes(stringWww.downloadHandler.data);
                    stringAllDialogs = StringParseCSV(stringCsvText);

                    speakerCsvText = CryptoUtility.TextDecryptFromBytes(speakerWww.downloadHandler.data);
                    speakerAllDialogs = SpeakerParseCSV(speakerCsvText);

                    soundCsvText = CryptoUtility.TextDecryptFromBytes(soundWww.downloadHandler.data);
                    soundAllDialogs = SoundParseCSV(soundCsvText);
                }
                else
                {
                    Debug.LogError("CSV 불러오기 실패: " + dialogWww.error);
                    yield break;
                }
       #else
               // PC, iOS, 에디터
               dialogCsvText = CryptoUtility.TextDecryptFromFile(File.ReadAllText(dialogPath));
               dialogAllDialogs = DialogParseCSV(dialogCsvText);
               
               stringCsvText = CryptoUtility.TextDecryptFromFile(File.ReadAllText(stringPath));
               stringAllDialogs = StringParseCSV(stringCsvText);
               
               speakerCsvText = CryptoUtility.TextDecryptFromFile(File.ReadAllText(speakerPath));
               speakerAllDialogs = SpeakerParseCSV(speakerCsvText);
               
               soundCsvText = CryptoUtility.TextDecryptFromFile(File.ReadAllText(soundPath));
               soundAllDialogs = SoundParseCSV(soundCsvText);
               
               yield return null;
        #endif
        
        
        DialogData[] result = GetDialogGroupById(id);
        List<DialogLine> dialogList = new List<DialogLine>();
       
        // 빠른 검색을 위해 Dictionary 생성
        var stringDict = stringAllDialogs.ToDictionary(x => x.stringId, x => x);
        var speakerDict = speakerAllDialogs.ToDictionary(x => x.speakerId, x => x);
        var soundDict = soundAllDialogs.ToDictionary(x => x.soundId, x => x);

        foreach (var dialog_d in result)
        {
            string centerCharacter = "";
            string leftCharacter = "";
            string rightCharacter = "";
            string leftSpeakerImgPath = "";
            string centerSpeakerImgPath = "";
            string rightSpeakerImgPath = "";
            
            string leftSpeakerOnOff= "";
            string centerSpeakerOnOff  = "";
            string rightSpeakerOnOff = "";
            
            string bgm= "";
            bool bgmLoop= false;
            float bgmVolume= 1;
            string sfx= "";
            bool sfxLoop = false;
            float sfxVolume= 1;

            if (!string.IsNullOrEmpty(dialog_d.centerSpeakerId) &&
                speakerDict.TryGetValue(dialog_d.centerSpeakerId, out var centerSpeaker))
            {
                centerCharacter = ResolveString(centerSpeaker.nameStringId, langCode);
                centerSpeakerImgPath = centerSpeaker.spritePath;
                centerSpeakerOnOff = centerSpeaker.speakerId.Split('_').Last();
            }

            // 좌측 화자
            if (!string.IsNullOrEmpty(dialog_d.leftSpeakerId) &&
                speakerDict.TryGetValue(dialog_d.leftSpeakerId, out var leftSpeaker))
            {
                leftCharacter = ResolveString(leftSpeaker.nameStringId, langCode);
                leftSpeakerImgPath = leftSpeaker.spritePath;
                leftSpeakerOnOff = leftSpeaker.speakerId.Split('_').Last();
            }

            // 우측 화자
            if (!string.IsNullOrEmpty(dialog_d.rightSpeakerId) &&
                speakerDict.TryGetValue(dialog_d.rightSpeakerId, out var rightSpeaker))
            {
                rightCharacter = ResolveString(rightSpeaker.nameStringId, langCode);
                rightSpeakerImgPath = rightSpeaker.spritePath;
                rightSpeakerOnOff = rightSpeaker.speakerId.Split('_').Last();
            }

            if (!string.IsNullOrEmpty(dialog_d.bgmSoundId) &&
                soundDict.TryGetValue(dialog_d.bgmSoundId, out var bgmSound))
            {
                bgm = bgmSound.name;
                bgmLoop = bgmSound.loopCheck;
                bgmVolume = bgmSound.volume;
            }
            
            if (!string.IsNullOrEmpty(dialog_d.sfxSoundId) &&
                soundDict.TryGetValue(dialog_d.sfxSoundId, out var sfxSound))
            {
                sfx = sfxSound.name;
                sfxLoop = sfxSound.loopCheck;
                sfxVolume = sfxSound.volume;
            }

                dialogList.Add(new DialogLine 
                {
                    script = ResolveString(dialog_d.dialogStringId, langCode),
                    centerCharacter = centerCharacter,
                    leftCharacter = leftCharacter,
                    rightCharacter = rightCharacter,
                    backgroundIgm = dialog_d.spritePath,
                    bgm = bgm,
                    bgmLoop = bgmLoop,
                    bgmVolume = bgmVolume,
                    sfx = sfx,
                    sfxLoop = sfxLoop,
                    sfxVolume = sfxVolume,
                    leftSpeakerImgPath= leftSpeakerImgPath,
                    centerSpeakerImgPath= centerSpeakerImgPath,
                    rightSpeakerImgPath= rightSpeakerImgPath,
                    centerSpeakerOnOff= centerSpeakerOnOff,
                    leftSpeakerOnOff= leftSpeakerOnOff,
                    rightSpeakerOnOff= rightSpeakerOnOff,
                });
        }
        
        currentDialogList = dialogList;
        currentIndex = 0;

        // 첫 대사 보여주기
        if (currentDialogList.Count > 0)
        {
            ShowNextDialogue();
        }
    }

    public void TotalDialogShow(bool val)
    {
           SetChildActive("BackImg", val);
           SetChildActive("LeftImg", val);
           SetChildActive("CenterImg", val);
           SetChildActive("RightImg", val);
           SetChildActive("CharName", val);
           SetChildActive("Content", val);
    }

    public void ShowNextDialogue()
    {
       // 리스트가 없거나 마지막 대사면 대사창 숨기기
       if (currentDialogList == null || currentDialogList.Count == 0) return;
       if (currentIndex >= currentDialogList.Count)
       {
           TotalDialogShow(false);
           return;
       }
        // 대사창 활성화
        TotalDialogShow(true);
        
        DialogLine line = currentDialogList[currentIndex];
        
        
        // ===== SFX 실행 =====
        if (!string.IsNullOrEmpty(line.sfx))
        {
            Manager.Audio.SfxAudioSource.loop = line.sfxLoop;
            Manager.Audio.SfxAudioSource.volume = line.sfxVolume;
            Manager.Audio.PlaySFXByName(line.sfx);
            
            // 기본값 초기화
            // Manager.Audio.SfxAudioSource.loop = true; 
            // Manager.Audio.SfxAudioSource.volume = 1f;
        }
        
        // ===== BGM 실행 =====
        if (!string.IsNullOrEmpty(line.bgm))
        {
            Manager.Audio.BgmAudioSource.loop = line.bgmLoop;
            Manager.Audio.BgmAudioSource.volume = line.bgmVolume;
            Manager.Audio.PlayBGMByName(line.bgm);
            
            // 기본값 초기화
            // Manager.Audio.BgmAudioSource.loop = false;
            // Manager.Audio.BgmAudioSource.volume = 1f;
        }
        
        // ===== 이미지 처리 =====
        List<string> activeSpeakers = new List<string>();
        // 중앙이미지
        UpdateSpeakerImage("CenterImg", line.centerSpeakerOnOff, line.centerSpeakerImgPath, line.centerCharacter, activeSpeakers);
        // 왼쪽이미지
        UpdateSpeakerImage("LeftImg",   line.leftSpeakerOnOff,   line.leftSpeakerImgPath,   line.leftCharacter,   activeSpeakers);
        // 오른쪽 이미지
        UpdateSpeakerImage("RightImg",  line.rightSpeakerOnOff,  line.rightSpeakerImgPath,  line.rightCharacter,  activeSpeakers);
        
        // 화좌 표시 포맷
        string activeSpeakerNames = string.Join(", ", activeSpeakers);
        
        // UI 표시
        dialogueText.text = line.script;
        speakerNameText.text = activeSpeakerNames;

        // 로그 출력
        Debug.Log(
            "sssssssssssssssssssssssssssss" +
            $"화자: {activeSpeakerNames}, " +
            $"script: {line.script}, " +
            $"bgm: {line.bgm}, " +
            $"bgmLoop: {line.bgmLoop}, " +
            $"bgmVolume: {line.bgmVolume}, " +
            $"sfx: {line.sfx}, " +
            $"sfxLoop: {line.sfxLoop}, " +
            $"sfxVolume: {line.sfxVolume}, " +
            $"centerSpeakerImgPath: {line.centerSpeakerImgPath}, " +
            $"leftSpeakerImgPath: {line.leftSpeakerImgPath}, " +
            $"rightSpeakerImgPath: {line.rightSpeakerImgPath}, " +
            $"backgroundImg: {line.backgroundIgm}"
        );

        currentIndex++;
    }
    
    private void UpdateSpeakerImage(string childName, string state, string spritePath, string characterName, List<string> activeSpeakers)
    {
        Transform child = transform.Find(childName);
        if (child == null) return;
    
        var img = child.GetComponent<UnityEngine.UI.Image>();
        if (img == null) return;
        
        if (string.IsNullOrEmpty(state))
        {
            child.gameObject.SetActive(false);
            return;
        }

        // 일단 비활성화
        child.gameObject.SetActive(false);
        
        // Addressables 로드
        LoadSprite(spritePath, sprite =>
        {
            if (sprite != null)
            {
                img.sprite = sprite;
                child.gameObject.SetActive(true); // 스프라이트 적용된 후에 보이게
            }
        });
    
        if (state == "on")
        {
            img.color = Color.white; // 선명
            activeSpeakers.Add(characterName);
        }
        else if (state == "off")
        {
            img.color = new Color(0.7f, 0.7f, 0.7f, 0.7f); // 검은 알파
        }
        else
        {
            child.gameObject.SetActive(false);
        }
    }
    

    private void LoadSprite(string path, System.Action<Sprite> onLoaded)
    {
        if (string.IsNullOrEmpty(path))
        {
            onLoaded?.Invoke(null);
            return;
        }

        var handle = Addressables.LoadAssetAsync<SO_SpriteData>(path);
        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                SO_SpriteData so = op.Result;
                if (so != null && so.loadedSprite != null)
                {
                    onLoaded?.Invoke(so.loadedSprite);
                }
                else
                {
                    Debug.LogError($"[LoadSprite] SpriteSO는 로드됐지만 Sprite가 비어있음: {path}");
                    onLoaded?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"[LoadSprite] SpriteSO 로드 실패: {path}");
                onLoaded?.Invoke(null);
            }

            Addressables.Release(op);
        };
    }
    
     public void SetChildActive(string childName, bool active)
     {
         Transform child = transform.Find(childName);
         if (child != null)
             child.gameObject.SetActive(active);
     }
    
    string ResolveString(string stringId, string langCode)
    {
        var s = stringAllDialogs.FirstOrDefault(x => x.stringId == stringId);
        if (s == null) return "";

        return langCode switch
        {
            "ko" => s.korean,
            "en" => s.english,
            "zh" => s.chinese,
            _    => s.korean  // 기본값 한국어
        };
    }
    
    List<DialogData> DialogParseCSV(string csvText)
    {
        var rows = csvText.Split('\n');
        var list = new List<DialogData>();

        for (int i = 3; i < rows.Length; i++) // 4번째 라인부터
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;

            var cols = rows[i].Trim().Split(',');

            DialogData data = new DialogData
            {
                dialogId = string.IsNullOrEmpty(cols[0]) ? 0 : int.Parse(cols[0]),
                dialogGroup = string.IsNullOrEmpty(cols[1]) ? 0 : int.Parse(cols[1]),
                order = string.IsNullOrEmpty(cols[2]) ? 0 : int.Parse(cols[2]),

                centerSpeakerId = cols.Length > 3 ? cols[3] : "",
                leftSpeakerId   = cols.Length > 4 ? cols[4] : "",
                rightSpeakerId  = cols.Length > 5 ? cols[5] : "",
                dialogStringId  = cols.Length > 6 ? cols[6] : "",
                spritePath      = cols.Length > 7 ? cols[7] : "",
                bgmSoundId      = cols.Length > 8 ? cols[8] : "",
                sfxSoundId      = cols.Length > 9 ? cols[9] : ""
            };

            list.Add(data);
        }

        return list;
    }
    
    List<StringData> StringParseCSV(string csvText)
    {
        var rows = csvText.Split('\n');
        var list = new List<StringData>();

        for (int i = 3; i < rows.Length; i++) // 4번째 라인부터
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;

            var cols = rows[i].Trim().Split(',');

            StringData data = new StringData()
            {
                stringId = cols.Length > 0 ? cols[0] : "",
                korean = cols.Length > 1 ? cols[1] : "",
                english = cols.Length > 2 ? cols[2] : "",
                chinese= cols.Length > 3 ? cols[3] : ""
            };

            list.Add(data);
        }

        return list;
    }
    
    List<SpeakerData> SpeakerParseCSV(string csvText)
    {
        var rows = csvText.Split('\n');
        var list = new List<SpeakerData>();

        for (int i = 3; i < rows.Length; i++) // 4번째 라인부터
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;

            var cols = rows[i].Trim().Split(',');

            SpeakerData data = new SpeakerData()
            {
                speakerId = cols.Length > 0 ? cols[0] : "",
                nameStringId = cols.Length > 1 ? cols[1] : "",
                spritePath = cols.Length > 2 ? cols[2] : "",
                description = cols.Length > 3 ? cols[3] : ""
            };

            list.Add(data);
        }

        return list;
    }
    
    List<SoundData> SoundParseCSV(string csvText)
    {
        var rows = csvText.Split('\n');
        var list = new List<SoundData>();

        for (int i = 3; i < rows.Length; i++) // 4번째 라인부터
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;

            var cols = rows[i].Trim().Split(',');

            SoundData data = new SoundData()
            {
                soundId=  cols.Length > 0 ? cols[0] : "",
                name = cols.Length > 1 ? cols[1] : "",
                filePath = cols.Length > 2 ? cols[2] : "",
                volume = cols.Length > 3 && float.TryParse(cols[3], out var vol) ? vol : 1f,
                loopCheck = cols.Length > 4 && bool.TryParse(cols[4], out var loop) ? loop : false,
                description = cols.Length > 5 ? cols[5] : ""
            };

            list.Add(data);
        }

        return list;
    }

    DialogData[] GetDialogGroupById(int dialogId)
    {
        var target = dialogAllDialogs.FirstOrDefault(d => d.dialogId == dialogId);
        if (target == null) return new DialogData[0];

        return dialogAllDialogs
            .Where(d => d.dialogGroup == target.dialogGroup)
            .OrderBy(d => d.order)
            .ToArray();
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드 이벤트 등록
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 씬 로드 이벤트 해제
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyFontToAllTMP(); // 씬 로드 후 모든 TMP_Text 폰트 재적용
    }

    private void ApplyFontToAllTMP()
    {
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;
        TMP_FontAsset targetFont = defaultFont;

        if (code.StartsWith("zh"))
            targetFont = chineseFont;
        else if (code.StartsWith("en"))
            targetFont = englishFont;

        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (TMP_Text text in texts)
        {
            if (text.gameObject.scene.IsValid())
            {
                // 씬에 존재하는 오브젝트만 (프로젝트 에셋 프리팹 제외)
                if (!text.gameObject.scene.IsValid()) continue;

                // "NoFontChange" 태그가 붙은 경우 제외
                if (text.CompareTag("NoFontChange")) continue;

                // 폰트 적용
                text.font = targetFont;
            }
        }
    }

}