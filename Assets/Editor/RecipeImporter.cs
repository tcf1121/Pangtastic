using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RecipeImporter // 레시피 CSV를 RecipeSO로 변환하는 클래스
{
    private static string csvPath = "Assets/CSV/recipes.csv"; // 레시피 CSV 경로
    private static string ingredientSoDir = "Assets/ScriptableObject/Ingredients"; // 재료 SO경로
    private static string recipeSoDir = "Assets/ScriptableObject/Recipes"; // 레시피 SO 저장 경로
    private static int startRow = 3; // 데이터 시작 행
    private static int columnCount = 9; //열 개수
    //private static bool isNew;

    [MenuItem("PangTastic/Import Recipes CSV")] // 메뉴 경로
    public static void StartImportRecipes()
    {
        ImportRecipeCSV();
    }

    private static void ImportRecipeCSV()
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

        if (Directory.Exists(recipeSoDir) == false) //폴더 없으면
        {
            Directory.CreateDirectory(recipeSoDir); // 폴더 생성
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

            int recipe_id = int.Parse(splitData[0]);
            string recipe_name = splitData[1];
            int material1_id = int.Parse(splitData[2]);
            int material1_quantity = int.Parse(splitData[3]);
            int material2_id = (splitData[4].ToLower() == "null") ? 0 : int.Parse(splitData[4]); //값이 null(소문자 변경)이면 0, 아니면 파싱
            int material2_quantity = (splitData[5].ToLower() == "null") ? 0 : int.Parse(splitData[5]);
            int material3_id = (splitData[6].ToLower() == "null") ? 0 : int.Parse(splitData[6]);
            int material3_quantity = (splitData[7].ToLower() == "null") ? 0 : int.Parse(splitData[7]);
            string spritePath = splitData[8];

            string soPath = recipeSoDir + "/Recipe_" + recipe_id + ".asset"; // SO파일 저장경로/파일이름

            RecipeSO recipe = AssetDatabase.LoadAssetAtPath<RecipeSO>(soPath); // 기존 레시피 SO 불러오기

            if (recipe == null) // 경로에 "/Ingredient_" + id + ".asset" 이름의 SO가 없으면
            {
                recipe = ScriptableObject.CreateInstance<RecipeSO>(); // 새 SO
                AssetDatabase.CreateAsset(recipe, soPath); // SO 생성
                Debug.Log("새 레시피SO 생성: " + recipe_name);
                //isNew = true;
            }
            else //이미 파일이 있으면
            {
                Debug.Log("기존 레시피SO 갱신: " + recipe_name);
                //isNew = false;
            }

            recipe.ID = recipe_id;
            recipe.Name = recipe_name;
            recipe.FoodPic = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            if (recipe.FoodPic == null)
            {
                Debug.LogError($"레시피 이미지 없음 {recipe.Name}, 경로 : {spritePath}");
            }

            // 재료 목록 리스트
            List<RecipeSO.IngredientRequirement> reqList = new List<RecipeSO.IngredientRequirement>();

            AddIngredient(material1_id, material1_quantity, reqList, i); //재료1 추가 i는 디버깅용
            AddIngredient(material2_id, material2_quantity, reqList, i); //재료2 추가
            AddIngredient(material3_id, material3_quantity, reqList, i); //재료3 추가

            recipe.Ingredients = reqList.ToArray();

            EditorUtility.SetDirty(recipe); // 변경사항 저장에 포함
        }

        AssetDatabase.SaveAssets(); // 저장
        AssetDatabase.Refresh(); // 새로고침
        Debug.Log("== 레시피 임포트 완료 ==");
    }
    private static void AddIngredient(int ingredientID, int quantity, List<RecipeSO.IngredientRequirement> list, int line)
    {
        if (ingredientID == 0 || quantity == 0) //재료가 없으면 안넣음
        {
            return;
        }

        string ingredientSOPath = ingredientSoDir + "/" + ingredientID + ".asset"; // 재료SO 경로
        IngredientSO ing = AssetDatabase.LoadAssetAtPath<IngredientSO>(ingredientSOPath); // 재료SO 로드

        if (ing == null) //재료가 없으면
        {
            Debug.LogError($"재료SO 없음 : {line}행 확인. 재료 경로: {ingredientSOPath}");
            return;
        }

        RecipeSO.IngredientRequirement req = new RecipeSO.IngredientRequirement(); // 구조체 생성
        req.Ingredient = ing; // 재료 넣기
        req.Amount = quantity; // 수량 넣기
        list.Add(req); // 리스트에 추가
    }

}
