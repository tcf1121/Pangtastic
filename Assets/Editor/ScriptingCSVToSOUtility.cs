#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ScriptingCSVToSOUtility : EditorWindow
{
    private string csvPath = "";

    [MenuItem("Tools/ScriptingCSV → SO 변환기")]
    static void Init()
    {
        GetWindow<ScriptingCSVToSOUtility>("CSV → SO 변환기");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV → ScriptableObject 변환기", EditorStyles.boldLabel);

        if (GUILayout.Button("CSV 파일 선택"))
        {
            csvPath = EditorUtility.OpenFilePanel("CSV 파일 선택", Application.dataPath, "csv");
        }

        EditorGUILayout.LabelField("선택된 파일:", string.IsNullOrEmpty(csvPath) ? "없음" : csvPath);

        if (!string.IsNullOrEmpty(csvPath))
        {
            if (GUILayout.Button("Dialog CSV → SO_ScriptDialog")) ConvertDialog(File.ReadAllText(csvPath), Path.GetFileNameWithoutExtension(csvPath));
            if (GUILayout.Button("String CSV → SO_ScriptString")) ConvertString(File.ReadAllText(csvPath), Path.GetFileNameWithoutExtension(csvPath));
            if (GUILayout.Button("Speaker CSV → SO_ScriptSpeaker")) ConvertSpeaker(File.ReadAllText(csvPath), Path.GetFileNameWithoutExtension(csvPath));
            if (GUILayout.Button("Sound CSV → SO_ScriptSound")) ConvertSound(File.ReadAllText(csvPath), Path.GetFileNameWithoutExtension(csvPath));
        }
    }

    private void ConvertDialog(string csvText, string fileName)
    {
        var so = CreateInstance<SO_ScriptDialog>();
        so.dialogs = new List<ScriptingSystem.DialogData>();

        var rows = csvText.Split('\n');
        for (int i = 3; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;
            var cols = ParseCsvLine(rows[i].Trim());

            var data = new ScriptingSystem.DialogData
            {
                dialogId = SafeInt(cols, 0),
                dialogGroup = SafeInt(cols, 1),
                order = SafeInt(cols, 2),
                centerSpeakerId = Safe(cols, 3),
                centerSpeakerCheck = Safe(cols, 4),
                leftSpeakerId = Safe(cols, 5),
                leftSpeakerCheck = Safe(cols, 6),
                rightSpeakerId = Safe(cols, 7),
                rightSpeakerCheck = Safe(cols, 8),
                dialogStringId = Safe(cols, 9),
                spritePath = Safe(cols, 10),
                bgmSoundId = Safe(cols, 11),
                sfxSoundId = Safe(cols, 12)
            };
            so.dialogs.Add(data);
        }

        SaveSO(so, $"{fileName}_Dialog.asset");
    }

    private void ConvertString(string csvText, string fileName)
    {
        var so = CreateInstance<SO_ScriptString>();
        so.strings = new List<ScriptingSystem.StringData>();

        var rows = csvText.Split('\n');
        for (int i = 3; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;
            var cols = ParseCsvLine(rows[i].Trim());

            var data = new ScriptingSystem.StringData
            {
                stringId = Safe(cols, 0),
                korean = Safe(cols, 1),
                english = Safe(cols, 2),
                chinese = Safe(cols, 3),
                japanese = Safe(cols, 4)
            };
            so.strings.Add(data);
        }

        SaveSO(so, $"{fileName}_String.asset");
    }

    private void ConvertSpeaker(string csvText, string fileName)
    {
        var so = CreateInstance<SO_ScriptSpeaker>();
        so.speakers = new List<ScriptingSystem.SpeakerData>();

        var rows = csvText.Split('\n');
        for (int i = 3; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;
            var cols = ParseCsvLine(rows[i].Trim());

            var data = new ScriptingSystem.SpeakerData
            {
                speakerId = Safe(cols, 0),
                nameStringId = Safe(cols, 1),
                spritePath = Safe(cols, 2),
                description = Safe(cols, 3)
            };
            so.speakers.Add(data);
        }

        SaveSO(so, $"{fileName}_Speaker.asset");
    }

    private void ConvertSound(string csvText, string fileName)
    {
        var so = CreateInstance<SO_ScriptSound>();
        so.sounds = new List<ScriptingSystem.SoundData>();

        var rows = csvText.Split('\n');
        for (int i = 3; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue;
            var cols = ParseCsvLine(rows[i].Trim());

            var data = new ScriptingSystem.SoundData
            {
                soundId = Safe(cols, 0),
                name = Safe(cols, 1),
                filePath = Safe(cols, 2),
                volume = SafeFloat(cols, 3, 1f),
                loopCheck = SafeBool(cols, 4),
                description = Safe(cols, 5)
            };
            so.sounds.Add(data);
        }

        SaveSO(so, $"{fileName}_Sound.asset");
    }

    // ===== Helper =====
    private string Safe(string[] cols, int index) =>
        (cols.Length > index) ? cols[index] : "";

    private int SafeInt(string[] cols, int index) =>
        (cols.Length > index && int.TryParse(cols[index], out var v)) ? v : 0;

    private float SafeFloat(string[] cols, int index, float def) =>
        (cols.Length > index && float.TryParse(cols[index], out var v)) ? v : def;

    private bool SafeBool(string[] cols, int index) =>
        (cols.Length > index && bool.TryParse(cols[index], out var v)) && v;

    private void SaveSO(ScriptableObject so, string fileName)
    {
        string savePath = "Assets/ScriptableObject/Script";
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string path = Path.Combine(savePath, fileName);
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CSVToSO] 저장 완료: {path}");
    }
    
    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // "" → " 로 변환
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++; // 다음 따옴표 스킵
                }
                else
                {
                    insideQuotes = !insideQuotes; // 따옴표 열기/닫기
                }
            }
            else if (c == ',' && !insideQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        result.Add(sb.ToString());

        // 후처리: 따옴표 제거 & 이스케이프 해제 & Trim
        for (int i = 0; i < result.Count; i++)
        {
            string field = result[i].Trim();

            if (field.StartsWith("\"") && field.EndsWith("\""))
            {
                field = field.Substring(1, field.Length - 2);
            }

            // "" → " 치환
            result[i] = field.Replace("\"\"", "\"");
        }

        return result.ToArray();
    }

}
#endif
