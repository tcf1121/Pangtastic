using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
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
        public string sfx;
        public string bgm;
        public string backgroundIgm;
        public string centerSpeakerImgPath;
        public string leftSpeakerImgPath;
        public string rightSpeakerImgPath;
        public string rightSpeakerOnOff;
        public string centerSpeakerOnOff;
        public string leftSpeakerOnOff;
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
    private string dialogName = "dialog.csv";
    private string stringName = "string.csv";
    private string speakerName = "speaker.csv";
    
    void Start()
    {
        StartCoroutine(DialogLoadCSV(6002));
    }

    public IEnumerator DialogLoadCSV(int id)
    {
        string dialogPath = Path.Combine(Application.streamingAssetsPath, dialogName);
        string stringPath = Path.Combine(Application.streamingAssetsPath, stringName);
        string speakerPath = Path.Combine(Application.streamingAssetsPath, speakerName);
        
        string langCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        
        string dialogCsvText;
        string stringCsvText;
        string speakerCsvText;

        #if UNITY_ANDROID && !UNITY_EDITOR
                UnityWebRequest dialogWww = UnityWebRequest.Get(dialogPath);
                UnityWebRequest stringWww = UnityWebRequest.Get(stringPath);
                UnityWebRequest speakerWww = UnityWebRequest.Get(speakerPath);
                
                yield return dialogWww.SendWebRequest();
                yield return stringWww.SendWebRequest();
                yield return speakerWww.SendWebRequest();

               if (dialogWww.result == UnityWebRequest.Result.Success &&
                    stringWww.result == UnityWebRequest.Result.Success &&
                    speakerWww.result == UnityWebRequest.Result.Success)
                {
                    dialogCsvText = dialogWww.downloadHandler.text;
                    dialogAllDialogs = DialogParseCSV(dialogCsvText);

                    stringCsvText = stringWww.downloadHandler.text;
                    stringAllDialogs = StringParseCSV(stringCsvText);

                    speakerCsvText = speakerWww.downloadHandler.text;
                    speakerAllDialogs = SpeakerParseCSV(speakerCsvText);
                }
                else
                {
                    Debug.LogError("CSV 불러오기 실패: " + dialogWww.error);
                    yield break;
                }
       #else
               // PC, iOS, 에디터
               dialogCsvText = File.ReadAllText(dialogPath);
               dialogAllDialogs = DialogParseCSV(dialogCsvText);
               
               stringCsvText = File.ReadAllText(stringPath);
               stringAllDialogs = StringParseCSV(stringCsvText);
               
               speakerCsvText = File.ReadAllText(speakerPath);
               speakerAllDialogs = SpeakerParseCSV(speakerCsvText);
               
               yield return null;
        #endif
        
        
        DialogData[] result = GetDialogGroupById(id);
        List<DialogLine> dialogList = new List<DialogLine>();
       
        // 빠른 검색을 위해 Dictionary 생성
        var stringDict = stringAllDialogs.ToDictionary(x => x.stringId, x => x);
        var speakerDict = speakerAllDialogs.ToDictionary(x => x.speakerId, x => x);

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

                dialogList.Add(new DialogLine 
                {
                    script = ResolveString(dialog_d.dialogStringId, langCode),
                    centerCharacter = centerCharacter,
                    leftCharacter = leftCharacter,
                    rightCharacter = rightCharacter,
                    backgroundIgm = dialog_d.spritePath,
                    bgm = dialog_d.bgmSoundId,
                    sfx = dialog_d.sfxSoundId,
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

        // 확인용 로그
        foreach (var line in dialogList)
        {
            Debug.Log(
                $"script: {line.script}, " +
                $"center: {line.centerCharacter}, " +
                $"left: {line.leftCharacter}, " +
                $"right: {line.rightCharacter}, " +
                $"backgroundImg: {line.backgroundIgm}, " +
                $"bgm: {line.bgm}, " +
                $"sfx: {line.sfx}, " +
                $"centerSpeakerImgPath: {line.centerSpeakerImgPath}, " +
                $"leftSpeakerImgPath: {line.leftSpeakerImgPath}, " +
                $"rightSpeakerImgPath: {line.rightSpeakerImgPath}, " +
                $"centerSpeakerOnOff: {line.centerSpeakerOnOff}, " +
                $"leftSpeakerOnOff: {line.leftSpeakerOnOff}, " +
                $"rightSpeakerOnOff: {line.rightSpeakerOnOff}"
            );
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
       if (currentDialogList == null || currentDialogList.Count == 0) return;
       if (currentIndex >= currentDialogList.Count)
       {
           TotalDialogShow(false);
           return;
       }
       TotalDialogShow(true);

        DialogLine line = currentDialogList[currentIndex];

        // ===== 여러 화자가 동시에 on일 때 처리 =====
        List<string> activeSpeakers = new List<string>();
        if (line.centerSpeakerOnOff == "on")
        {
            SetChildActive("CenterImg", true);
            activeSpeakers.Add(line.centerCharacter);
        }
        else {
            SetChildActive("CenterImg", false);
        }

        if (line.leftSpeakerOnOff == "on")
        {
            SetChildActive("LeftImg", true);
            activeSpeakers.Add(line.leftCharacter);
        }
        else
        {
            SetChildActive("LeftImg", false);
        }

        if (line.rightSpeakerOnOff == "on")
        {
            activeSpeakers.Add(line.rightCharacter);
            SetChildActive("RightImg", true);
        }
        else
        {
            SetChildActive("RightImg", false);
        }

        string activeSpeakerNames = string.Join(", ", activeSpeakers);
        
        Manager.Audio.PlayBGMByName(line.bgm);
        Manager.Audio.PlaySFXByName(line.sfx);

        // UI 표시
        dialogueText.text = line.script;
        speakerNameText.text = activeSpeakerNames;

        // 로그 출력
        Debug.Log(
            $"화자: {activeSpeakerNames}, " +
            $"script: {line.script}, " +
            $"bgm: {line.bgm}, " +
            $"sfx: {line.sfx}, " +
            $"backgroundImg: {line.backgroundIgm}"
        );

        currentIndex++;
    }

    // 리소스 로드 함수 (예: Resources 폴더에 이미지 넣었을 때)
    private Sprite LoadSprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        return Resources.Load<Sprite>(path);
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