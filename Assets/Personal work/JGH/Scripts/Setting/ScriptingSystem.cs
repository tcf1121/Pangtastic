using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ScriptingSystem : Singleton<ScriptingSystem>
{
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text RightNameText;
    [SerializeField] private TMP_Text LeftNameText;

    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset defaultFont;   // 한국어
    [SerializeField] private TMP_FontAsset chineseFont;   // 중국어
    [SerializeField] private TMP_FontAsset englishFont;   // 영어

    [Header("S(Addressables)")]
    [SerializeField] private string dialogDBKey = "Assets/ScriptableObject/Script/dialog_Dialog.asset";
    [SerializeField] private string stringDBKey = "Assets/ScriptableObject/Script/string_String.asset";
    [SerializeField] private string speakerDBKey = "Assets/ScriptableObject/Script/speaker_Speaker.asset";
    [SerializeField] private string soundDBKey = "Assets/ScriptableObject/Script/sound_Sound.asset";

    private SO_ScriptDialog dialogScript;
    private SO_ScriptString stringScript;
    private SO_ScriptSpeaker speakerScript;
    private SO_ScriptSound soundScript;

    private List<DialogLine> currentDialogList = new List<DialogLine>();
    private int currentIndex = 0;

    // ===== 데이터 클래스 =====
    [Serializable]
    public class DialogData
    {
        public int dialogId;
        public int dialogGroup;
        public int order;

        public string centerSpeakerId;
        public string centerSpeakerCheck;

        public string leftSpeakerId;
        public string leftSpeakerCheck;

        public string rightSpeakerId;
        public string rightSpeakerCheck;

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
        public string japanese;
    }

    [Serializable]
    public class SpeakerData
    {
        public string speakerId;
        public string nameStringId;
        public string spritePath;
        public string description;
    }

    [Serializable]
    public class DialogLine
    {
        public string script;
        public string bgm;
        public bool bgmLoop;
        public float bgmVolume;
        public string sfx;
        public bool sfxLoop;
        public float sfxVolume;
        public string backgroundIgm;
        public string centerCharacter;
        public string leftCharacter;
        public string rightCharacter;
        public string centerSpeakerImgPath;
        public string leftSpeakerImgPath;
        public string rightSpeakerImgPath;
        public string rightSpeakerOnOff;
        public string centerSpeakerOnOff;
        public string leftSpeakerOnOff;
    }

    [Serializable]
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

        // Addressables 즉시 로드
        dialogScript = LoadSO<SO_ScriptDialog>(dialogDBKey);
        stringScript = LoadSO<SO_ScriptString>(stringDBKey);
        speakerScript = LoadSO<SO_ScriptSpeaker>(speakerDBKey);
        soundScript = LoadSO<SO_ScriptSound>(soundDBKey);

        if (dialogScript == null || stringScript == null || speakerScript == null || soundScript == null)
        {
            Debug.LogError("[ScriptingSystem] 하나 이상의 SO 로딩 실패!");
            return;
        }
    }

    private T LoadSO<T>(string key) where T : ScriptableObject
    {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        T result = handle.WaitForCompletion();

        if (result != null)
        {
            Debug.Log($"[ScriptingSystem] {typeof(T).Name} 로드 성공: {key}");
        }
        else
        {
            Debug.LogError($"[ScriptingSystem] {typeof(T).Name} 로드 실패: {key}");
        }

        return result;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ShowNextDialogue();
        }
    }

    // ========= SO 데이터 로드 - 데이터 배열로 담기 =========
    public void DialogLoadSO(int id)
    {
        //ApplyFontToAllTMP();
        // LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
        // LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("zh");
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("jp");
        Manager.Language.ChangeLanguage();

        var dialogAllDialogs = dialogScript.dialogs;
        var stringAllDialogs = stringScript.strings;
        var speakerAllDialogs = speakerScript.speakers;
        var soundAllDialogs = soundScript.sounds;

        string langCode = LocalizationSettings.SelectedLocale.Identifier.Code;

        DialogData[] result = GetDialogGroupById(dialogAllDialogs, id);
        List<DialogLine> dialogList = new List<DialogLine>();

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

            string leftSpeakerOnOff = "";
            string centerSpeakerOnOff = "";
            string rightSpeakerOnOff = "";

            string bgm = "";
            bool bgmLoop = false;
            float bgmVolume = 1;

            string sfx = "";
            bool sfxLoop = false;
            float sfxVolume = 1;

            /// === 캐릭터 ===
            if (!string.IsNullOrEmpty(dialog_d.centerSpeakerId) &&
                speakerDict.TryGetValue(dialog_d.centerSpeakerId, out var centerSpeaker))
            {
                centerCharacter = ResolveString(centerSpeaker.nameStringId, langCode, stringDict);
                centerSpeakerImgPath = centerSpeaker.spritePath;
            }

            if (!string.IsNullOrEmpty(dialog_d.leftSpeakerId) &&
                speakerDict.TryGetValue(dialog_d.leftSpeakerId, out var leftSpeaker))
            {
                leftCharacter = ResolveString(leftSpeaker.nameStringId, langCode, stringDict);
                leftSpeakerImgPath = leftSpeaker.spritePath;
            }

            if (!string.IsNullOrEmpty(dialog_d.rightSpeakerId) &&
                speakerDict.TryGetValue(dialog_d.rightSpeakerId, out var rightSpeaker))
            {
                rightCharacter = ResolveString(rightSpeaker.nameStringId, langCode, stringDict);
                rightSpeakerImgPath = rightSpeaker.spritePath;
            }

            // === 사운드 ===
            if (!string.IsNullOrEmpty(dialog_d.bgmSoundId) &&
                soundDict.TryGetValue(dialog_d.bgmSoundId, out var bgmSound))
            {
                bgm = bgmSound.name;
                bgmLoop = bgmSound.loopCheck;
                bgmVolume = bgmSound.volume;
            }

            // === 효과음 ===
            if (!string.IsNullOrEmpty(dialog_d.sfxSoundId) &&
                soundDict.TryGetValue(dialog_d.sfxSoundId, out var sfxSound))
            {
                sfx = sfxSound.name;
                sfxLoop = sfxSound.loopCheck;
                sfxVolume = sfxSound.volume;
            }

            // === 화자 표시 ===
            leftSpeakerOnOff = dialog_d.leftSpeakerCheck;
            centerSpeakerOnOff = dialog_d.centerSpeakerCheck;
            rightSpeakerOnOff = dialog_d.rightSpeakerCheck;

            dialogList.Add(new DialogLine
            {
                script = ResolveString(dialog_d.dialogStringId, langCode, stringDict),
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
                leftSpeakerImgPath = leftSpeakerImgPath,
                leftSpeakerOnOff = leftSpeakerOnOff,
                centerSpeakerImgPath = centerSpeakerImgPath,
                centerSpeakerOnOff = centerSpeakerOnOff,
                rightSpeakerImgPath = rightSpeakerImgPath,
                rightSpeakerOnOff = rightSpeakerOnOff,
            });
        }

        currentDialogList = dialogList;
        currentIndex = 0;

        // 확인용
        foreach (var line in currentDialogList)
        {
            Debug.Log($"[DialogLine] {JsonUtility.ToJson(line, true)}");
        }


        if (currentDialogList.Count > 0)
        {
            ShowNextDialogue();
        }
    }

    private string ResolveString(string stringId, string langCode, Dictionary<string, StringData> stringDict)
    {
        if (!stringDict.TryGetValue(stringId, out var s)) return "";

        return langCode switch
        {
            "ko" => s.korean,
            "en" => s.english,
            "zh" => s.chinese,
            "ja" => s.japanese,
            _ => s.korean
        };
    }

    private DialogData[] GetDialogGroupById(List<DialogData> allDialogs, int dialogId)
    {
        var target = allDialogs.FirstOrDefault(d => d.dialogId == dialogId);
        if (target == null) return new DialogData[0];

        return allDialogs
            .Where(d => d.dialogGroup == target.dialogGroup)
            .OrderBy(d => d.order)
            .ToArray();
    }

    public void TotalDialogShow(bool val)
    {
        SetChildActive("BackImg", val);
        SetChildActive("CenterImg", val);
        SetChildActive("LeftImg", val);
        SetChildActive("Content", val);
        SetChildActive("RightImg", val);
    }

    public void ShowNextDialogue()
    {
        if (currentDialogList == null || currentDialogList.Count == 0) return;
        if (currentIndex >= currentDialogList.Count)
        {
            TotalDialogShow(false);
            SetChildActive("RightName", false);
            SetChildActive("LeftName", false);
            return;
        }

        DialogLine line = currentDialogList[currentIndex];
        List<string> activeSpeakers = new List<string>();
        TotalDialogShow(true);

        // ===== SFX 실행 =====
        if (!string.IsNullOrEmpty(line.sfx))
        {
            Manager.Audio.SfxAudioSource.loop = line.sfxLoop;
            Manager.Audio.SfxAudioSource.volume = line.sfxVolume;
            //Manager.Audio.PlaySFXByName(line.sfx);
        }

        // ===== BGM 실행 =====
        if (!string.IsNullOrEmpty(line.bgm))
        {
            Manager.Audio.BgmAudioSource.loop = line.bgmLoop;
            Manager.Audio.BgmAudioSource.volume = line.bgmVolume;
            //Manager.Audio.PlayBGMByName(line.bgm);
        }


        // ===== 닉네임 왼족, 오른쪽 활성화
        SetChildActive("RightName", false);
        SetChildActive("LeftName", false);
        if (line.centerSpeakerOnOff.Trim().ToLower() == "true")
        {
            SetChildActive("RightName", true);
            RightNameText.text = line.centerCharacter;
        }

        if (line.rightSpeakerOnOff.Trim().ToLower() == "true")
        {
            SetChildActive("RightName", true);
            RightNameText.text = line.rightCharacter;
        }

        if (line.leftSpeakerOnOff.Trim().ToLower() == "true")
        {
            SetChildActive("LeftName", true);
            LeftNameText.text = line.leftCharacter;
        }


        // ===== 이미지 처리 =====
        UpdateSpeakerImage("CenterImg", line.centerSpeakerOnOff, line.centerSpeakerImgPath, line.centerCharacter, activeSpeakers);
        UpdateSpeakerImage("LeftImg", line.leftSpeakerOnOff, line.leftSpeakerImgPath, line.leftCharacter, activeSpeakers);
        UpdateSpeakerImage("RightImg", line.rightSpeakerOnOff, line.rightSpeakerImgPath, line.rightCharacter, activeSpeakers);

        // string activeSpeakerNames = string.Join(", ", activeSpeakers);

        // ===== 대사 처리 =====
        dialogueText.text = line.script;
        // speakerNameText.text = activeSpeakerNames;


        // Debug.Log($"[Dialogue] 화자: {activeSpeakerNames}, script: {line.script}");
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

        child.gameObject.SetActive(false);

        LoadSprite(spritePath, sprite =>
        {
            if (sprite != null)
            {
                img.sprite = sprite;
                child.gameObject.SetActive(true);
            }
        });

        if (state.Trim().ToLower() == "true")
        {
            img.color = Color.white;
            activeSpeakers.Add(characterName);
        }
        else if (state.Trim().ToLower() == "false")
        {
            img.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
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

        var handle = Addressables.LoadAssetAsync<Sprite>(path);
        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                Sprite sp = op.Result;
                onLoaded?.Invoke(sp);
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //ApplyFontToAllTMP();
        Manager.Language.ChangeLanguage();
    }

    /// <summary>
    /// 폰트 적용
    /// </summary>
    // private void ApplyFontToAllTMP()
    // {
    //     string code = LocalizationSettings.SelectedLocale.Identifier.Code;
    //     TMP_FontAsset targetFont = defaultFont;

    //     if (code.StartsWith("zh"))
    //         targetFont = chineseFont;
    //     else if (code.StartsWith("en"))
    //         targetFont = englishFont;

    //     TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
    //     foreach (TMP_Text text in texts)
    //     {
    //         if (!text.gameObject.scene.IsValid()) continue;
    //         if (text.CompareTag("NoFontChange")) continue;
    //         text.font = targetFont;
    //     }
    // }
}
