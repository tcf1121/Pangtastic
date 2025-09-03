using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ScriptingSystem : MonoBehaviour
{
    public static ScriptingSystem Instance { get; private set; }

    [SerializeField] private TMP_Text _charName;          // 캐릭터 이름
    [SerializeField] private TMP_Text _dialogueText;          // 내용
    [SerializeField] private GameObject _backImg;          // 부각 이미지

    [SerializeField] private string _dialogueTableName; // String Table 이름
    [SerializeField] private string _characterTableName; // 캐릭터 Table 이름
    [SerializeField] private int startId;        // 시작 ID
    [SerializeField] private int endId;          // 끝 ID

    [SerializeField] private TMP_FontAsset defaultFont; // 기본 폰트 (예: 한국어)
    [SerializeField] private TMP_FontAsset chineseFont; // 중국어용 폰트
    [SerializeField] private TMP_FontAsset englishFont; // 영어용 폰트

    private int currentId;

    protected void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ScriptStart(string getDialogueTableName, string getCharacterTableName, int getStartId, int getEndId)
    {
        _backImg.SetActive(true);
        _dialogueText.gameObject.SetActive(true);
        _charName.gameObject.SetActive(true);

        _dialogueTableName = getDialogueTableName;
        _characterTableName = getCharacterTableName;
        startId = getStartId;
        endId = getEndId;

        currentId = startId;
        ShowDialogue(currentId);
    }

    private void Update()
    {
        // 마우스 왼쪽 클릭 시
        if (Input.GetMouseButtonDown(0))
        {
            ShowNextDialogue();
        }
    }

    public void ShowNextDialogue()
    {
        if (currentId >= endId)
        {
            _backImg.SetActive(false);
            _dialogueText.gameObject.SetActive(false);
            _charName.gameObject.SetActive(false);
            return; // 끝에 도달하면 종료
        }

        currentId++;
        ShowDialogue(currentId);
    }

    private void ShowDialogue(int id)
    {
        // 대사 불러오기
        var dialogueString = new LocalizedString(_dialogueTableName, id.ToString());
        dialogueString.StringChanged += (value) =>
        {
            _dialogueText.text = value;
        };

        // 캐릭터 이름 불러오기
        var charNameString = new LocalizedString(_characterTableName, id.ToString());
        charNameString.StringChanged += (value) =>
        {
            _charName.text = value;
        };
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