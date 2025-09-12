using System.IO;
using UnityEditor;
using UnityEngine;

public class StringImporter // 레시피 CSV를 RecipeSO로 변환하는 클래스
{
    private static string csvPath = "Assets/CSV/String.csv"; // 레시피 CSV 경로
    private static string stringSoDir = "Assets/ScriptableObject/String"; // 레시피 SO 저장 경로
    private static int startRow = 1; // 데이터 시작 행
    private static int columnCount = 5; //열 개수
                                        //private static bool isNew;

    // 메뉴 경로
    public static void StartImportString()
    {
        ImportStringCSV();
    }

    private static void ImportStringCSV()
    {
        if (File.Exists(csvPath) == false) // CSV 파일 없으면
        {
            Debug.LogError("레시피 CSV 파일 없음: " + csvPath);
            return;
        }

        string[] lines = File.ReadAllLines(csvPath);

        if (lines.Length < startRow) // 데이터가 시작되는 행보다 줄이 적으면
        {
            Debug.LogError("레시피 CSV에 데이터 없음");
            return;
        }

        if (Directory.Exists(stringSoDir) == false) //폴더 없으면
        {
            Directory.CreateDirectory(stringSoDir); // 폴더 생성
        }

        for (int i = startRow; i < lines.Length; i++) // 데이터 행부터 끝까지 순회
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line)) // 빈 줄이면
            {
                continue; // 건너뛰기
            }

            string[] splitData = line.Split(','); // 콤마로 칼럼 분리

            if (splitData.Length < columnCount) // 열이 부족하면
            {
                Debug.LogError("열 개수 부족함(행 " + i + "): " + line);
                return;
            }


            string String_id = splitData[0];
            string korean = splitData[1];
            string english = splitData[2];
            string chinese = splitData[3];
            string japanese = splitData[4];

            string soPath = stringSoDir + "/" + String_id + ".asset"; // SO파일 저장경로/파일이름

            StringSO strings = AssetDatabase.LoadAssetAtPath<StringSO>(soPath); // 기존 레시피 SO 불러오기

            if (strings == null) // 경로에 "/Ingredient_" + id + ".asset" 이름의 SO가 없으면
            {
                strings = ScriptableObject.CreateInstance<StringSO>(); // 새 SO
                strings.value = new();
                for (int j = 0; j < 13; j++)
                {
                    strings.value.Add("");
                }
                AssetDatabase.CreateAsset(strings, soPath); // SO 생성
                Debug.Log("새 스트링SO 생성: " + String_id);
                //isNew = true;
            }
            else //이미 파일이 있으면
            {
                Debug.Log("기존 스트링SO 갱신: " + String_id);
                //isNew = false;
            }

            strings.ID = String_id;
            strings.value[0] = english;
            strings.value[3] = chinese;
            strings.value[5] = japanese;
            strings.value[6] = korean;

            EditorUtility.SetDirty(strings); // 변경사항 저장에 포함
        }

        AssetDatabase.SaveAssets(); // 저장
        AssetDatabase.Refresh(); // 새로고침
        Debug.Log("== 스트링 임포트 완료 ==");
    }

}
