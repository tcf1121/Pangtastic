using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CustomerImporter : MonoBehaviour //작업중
{
    private static string csvPath = "Assets/CSV/Customers.csv"; //csv 저장위치

    [MenuItem("PangTastic/Import Customer CSV")] // 메뉴 경로
    public static void StartImportCustomers()
    {
        ImportCustomerCSV();
    }

    private static void ImportCustomerCSV()
    {
        if (File.Exists(csvPath) == false) // 경로에 파일 없으면
        {
            Debug.LogError("CSV 파일 없음: " + csvPath);
            return;
        }

        string[] lines = File.ReadAllLines(csvPath); // CSV 파일 모든 줄 읽기

        if (lines.Length <= 1) // 헤더만 있고 데이터가 없으면
        {
            Debug.LogWarning("CSV에 데이터가 없음");
            return;
        }

        string saveDir = "Assets/ScriptableObject/Customers"; // SO 저장 경로

        if (Directory.Exists(saveDir) == false) // 폴더 없으면
        {
            Directory.CreateDirectory(saveDir); // 폴더 생성
        }

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더니까 1부터 시작
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line)) // 빈 줄이면
            {
                continue; // 건너뛰기
            }

            string[] splitData = line.Split(','); // 콤마로 열 나누기

            int id = int.Parse(splitData[0]);
            string name = splitData[1];
            string spritePath = splitData[2];

            string soPath = saveDir + "/Ingredient_" + id + ".asset"; // SO파일 저장경로/파일이름

            IngredientSO so = AssetDatabase.LoadAssetAtPath<IngredientSO>(soPath); // 기존 SO 불러오기

            if (so == null) // 경로에 "/Ingredient_" + id + ".asset" 이름의 SO가 없으면
            {
                so = ScriptableObject.CreateInstance<IngredientSO>(); // 새 SO
                AssetDatabase.CreateAsset(so, soPath); // SO 생성
                Debug.Log("새 재료SO 생성: " + name);
            }
            else //이미 파일이 있으면
            {
                EditorUtility.SetDirty(so); //덮어쓴 파일 저장하라고 알림
                Debug.Log("기존 재료SO 갱신: " + name);
            }

            //데이터 적용
            so.ID = id;
            so.Name = name;
            so.IngredientPic = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath); // 이미지 설정

            if (so.IngredientPic == null)
            {
                Debug.LogError($"재료 이미지 없음 {so.Name}, 경로 : {spritePath}");
            }
        }

        AssetDatabase.SaveAssets(); // 저장
        AssetDatabase.Refresh(); // 새로고침
        Debug.Log("==재료 임포트 완료==");
    }
}
