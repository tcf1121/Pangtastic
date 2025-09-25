using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DialogueImporter
{
    private static string _csvPath = "Assets/CSV/Dialogue.csv";

    private static string _dialogueGoupSODir = "Assets/ScriptableObject/DialogueGroup";
    private static string _stringSOPath = "Assets/ScriptableObject/String";

    private static int startRow = 3;

    private static Dictionary<int, DialogueGroupSO> _groupMap = new Dictionary<int, DialogueGroupSO>();

    //[MenuItem("PangTastic/Import DialogueGroupSO")]
    public static void StartImportDialogue()
    {
        ImportDialogueCSV();
    }

    private static void ImportDialogueCSV()
    {
        _groupMap.Clear();

        if (File.Exists(_csvPath) == false)
        {
            Debug.LogError("다이얼로그 CSV 파일 없음: " + _csvPath);
            return;
        }

        string[] dialogueLines = File.ReadAllLines(_csvPath);

        if (dialogueLines.Length < startRow)
        {
            Debug.LogError("다이얼로그 CSV에 데이터 없음");
            return;
        }

        if (Directory.Exists(_dialogueGoupSODir) == false)
        {
            Directory.CreateDirectory(_dialogueGoupSODir);
        }

        for (int i = startRow; i < dialogueLines.Length; i++)
        {
            string line = dialogueLines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] splitData = line.Split(',');

            int dialog_id = int.Parse(splitData[0]);
            int dialog_group = int.Parse(splitData[1]);
            int order = int.Parse(splitData[2]);

            string left_speaker = splitData[3];
            string center_speaker = splitData[4];
            string right_speaker = splitData[5];
            int.TryParse(splitData[6], out int left_speaker_check);

            string dialog_string_id = splitData[7];

            string sprite_path = splitData[8];

            string bgm_sound_id = splitData[9];
            string sfx_sound_id = splitData[10];

            DialogueLine dialogueLine = new DialogueLine();

            dialogueLine.DialogId = dialog_id;
            dialogueLine.Order = order;
            dialogueLine.SpeakerCheck = left_speaker_check;
            dialogueLine.BgmSoundId = bgm_sound_id;
            dialogueLine.SfxSoundId = sfx_sound_id;
            //dialogueLine.BackgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sprite_path); //주석 해제해야함
            dialogueLine.BackgroundSprite = "TEST"; // 테스트용. 삭제해야함

            string stringSoPath = _stringSOPath + "/" + dialog_string_id + ".asset";

            StringSO stringSo;

            stringSo = AssetDatabase.LoadAssetAtPath<StringSO>(stringSoPath);

            if (stringSo == null)
            {
                Debug.LogError($"스트링SO 없음 : {stringSoPath}");
                return;
            }

            dialogueLine.DialogueStringId = stringSo;

            string leftSpeakerName = left_speaker.Replace("speaker_", "").Trim();
            string centerSpeakerName = center_speaker.Replace("speaker_", "").Trim();
            string rightSpeakerName = right_speaker.Replace("speaker_", "").Trim();

            Speaker speaker;

            if (System.Enum.TryParse<Speaker>(leftSpeakerName, true, out speaker))
            {
                dialogueLine.LeftSpeaker = speaker;
            }
            else
            {
                Debug.LogWarning($"스피커 파싱 빈칸/실패 {i}행 확인 {leftSpeakerName}");
            }

            if (System.Enum.TryParse<Speaker>(centerSpeakerName, true, out speaker))
            {
                dialogueLine.CenterSpeaker = speaker;
            }
            else
            {
                Debug.LogWarning($"스피커 파싱 빈칸/실패 {i}행 확인 {centerSpeakerName}");
            }

            if (System.Enum.TryParse<Speaker>(rightSpeakerName, true, out speaker))
            {
                dialogueLine.RightSpeaker = speaker;
            }
            else
            {
                Debug.LogWarning($"스피커 파싱 빈칸/실패 {i}행 확인 {rightSpeakerName}");
            }

            string soPath = _dialogueGoupSODir + "/DialogueGroup_" + dialog_group + ".asset";

            DialogueGroupSO dialogueGroupSO;

            if (_groupMap.TryGetValue(dialog_group, out dialogueGroupSO) == false) //다이얼로그 그룹 없으면
            {
                dialogueGroupSO = AssetDatabase.LoadAssetAtPath<DialogueGroupSO>(soPath);

                if (dialogueGroupSO == null)
                {
                    dialogueGroupSO = ScriptableObject.CreateInstance<DialogueGroupSO>();
                    dialogueGroupSO.dialogGroup = dialog_group;
                    dialogueGroupSO.lines = new List<DialogueLine>();
                    AssetDatabase.CreateAsset(dialogueGroupSO, soPath);
                    Debug.Log("새 다이얼로그그룹SO 생성: " + dialog_group);
                }
                else
                {
                    if (dialogueGroupSO.lines == null)
                    {
                        dialogueGroupSO.lines = new List<DialogueLine>();
                    }
                    else
                    {
                        dialogueGroupSO.lines.Clear();
                    }
                    dialogueGroupSO.dialogGroup = dialog_group;

                    Debug.Log("기존 다이얼로그그룹SO 갱신: " + dialog_group);
                }
                _groupMap.Add(dialog_group, dialogueGroupSO);
            }

            dialogueGroupSO.lines.Add(dialogueLine);
            EditorUtility.SetDirty(dialogueGroupSO);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CSV >> DialogueGroupSO 변환 완료");
    }
}