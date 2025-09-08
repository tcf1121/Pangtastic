using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class StageImporter
{
    private static string csvPath = "Assets/CSV/Stages.csv"; // 스테이지 CSV 경로
    private static string recipeBundleCsvPath = "Assets/CSV/RecipeBundles.csv"; // 레시피 번들 CSV 경로
    private static string levelMultiplierCsvPath = "Assets/CSV/LevelMultiplier.csv"; // 레벨 재료 배수 CSV 경로

    private static string customerSoDir = "Assets/ScriptableObject/Customers"; // 손님 SO 경로
    private static string recipeSoDir = "Assets/ScriptableObject/Recipes"; // 레시피 SO 경로
    private static string ingredientSoDir = "Assets/ScriptableObject/Ingredients"; // 재료SO 경로
    private static string puzzleBoardSoDir = "Assets/ScriptableObject/PuzzleBoards"; //퍼즐 보드 경로
    private static string stageSoDir = "Assets/ScriptableObject/Stages"; // 스테이지 SO 저장 경로

    private static int startRow = 3; // 데이터 시작 행
    private static int columnCount = 7; // 열 개수

    //private static bool isNew; 

    [MenuItem("PangTastic/Import Stage CSV")]
    public static void StartImportStages()
    {
        ImportStageCSV();
    }

    private static void ImportStageCSV()
    {
        if (File.Exists(csvPath) == false) // 스테이지 CSV 없으면
        {
            Debug.LogError("스테이지 CSV 파일 없음: " + csvPath);
            return;
        }

        string[] stageLines = File.ReadAllLines(csvPath); //스테이지 CSV 읽기

        if (stageLines.Length < startRow)
        {
            Debug.LogError("스테이지 CSV에 데이터 없음");
            return;
        }

        if (File.Exists(recipeBundleCsvPath) == false) // 번들 CSV 없으면
        {
            Debug.LogError("번들 CSV 파일 없음: " + recipeBundleCsvPath);
            return;
        }

        string[] bundleLines = File.ReadAllLines(recipeBundleCsvPath); //번들 읽기

        if (bundleLines.Length < startRow)
        {
            Debug.LogError("번들 CSV에 데이터 없음");
            return;
        }

        if (File.Exists(recipeBundleCsvPath) == false) // 번들 CSV 없으면
        {
            Debug.LogError("번들 CSV 파일 없음: " + recipeBundleCsvPath);
            return;
        }

        string[] levelLines = File.ReadAllLines(levelMultiplierCsvPath); //배수 읽기

        if (levelLines.Length < startRow)
        {
            Debug.LogError("배수 CSV에 데이터 없음");
            return;
        }

        if (Directory.Exists(stageSoDir) == false) // StageSO 저장 폴더가 없으면
        {
            Directory.CreateDirectory(stageSoDir); // 폴더 생성
        }

        for (int i = startRow; i < stageLines.Length; i++) //스테이지 CSV 순회
        {
            string line = stageLines[i];

            if (string.IsNullOrWhiteSpace(line)) // 빈 줄이면
            {
                continue; // 다음
            }

            string[] splitData = line.Split(',');

            if (splitData.Length < columnCount)
            {
                Debug.LogError("열 개수 부족함(행 " + i + "): " + line);
                return;
            }

            int stage_id = int.Parse(splitData[0]);
            int customer_id = int.Parse(splitData[1]);
            int order_count = int.Parse(splitData[2]);
            int.TryParse(splitData[3], out int level_id);
            int stage_recipe_bundle = int.Parse(splitData[4]);
            string stage_type = splitData[5];
            int puzzle_board_id = int.Parse(splitData[6]);
            int.TryParse(splitData[7], out int max_gold_gain);

            string soPath = stageSoDir + "/Stage_" + stage_id + ".asset"; // SO파일 저장경로/파일이름

            StageSO stage = AssetDatabase.LoadAssetAtPath<StageSO>(soPath); // 기존 StageSO 로드

            if (stage == null) // SO가 없으면
            {
                stage = ScriptableObject.CreateInstance<StageSO>(); //새 SO
                AssetDatabase.CreateAsset(stage, soPath); // 생성
                Debug.Log("새 StageSO 생성: " + stage_id);
                //isNew = true;
            }
            else // SO가 있으면
            {
                Debug.Log("기존 StageSO 갱신: " + stage_id);
                //isNew = false;
            }

            stage.StageID = stage_id;
            stage.MaxGoldGain = max_gold_gain;

            string customerSOPath = customerSoDir + "/Customer_" + customer_id + ".asset"; // 손님 SO 경로
            CustomerSO customerSO = AssetDatabase.LoadAssetAtPath<CustomerSO>(customerSOPath); // 손님 SO 로드
            if (customerSO == null) // 손님 SO가 없으면
            {
                Debug.LogError($"CustomerSO 없음 : {customerSOPath} / {i} 행 확인");
                return;
            }
            else // 손님 SO가 있으면
            {
                stage.Customer = customerSO; // 손님 설정
            }

            if (order_count < 1) // 주문 수가 1 미만이면
            {
                order_count = 1; // 1로 설정
            }

            stage.OrderCount = order_count; // 주문 수 설정

            StageType stageType;

            if (System.Enum.TryParse<StageType>(stage_type, true, out stageType)) // string > enum 파싱 시도
            {
                stage.Type = stageType;
            }
            else
            {
                Debug.LogError($"스테이지타입 파싱 실패 {i}행 확인 {stage_type}");
                return;
            }

            string puzzleBoardSOPath = puzzleBoardSoDir + "/PuzzleBoard_" + puzzle_board_id + ".asset"; // 퍼즐보드SO 경로

            PuzzleBoardSO puzzleBoardSO = AssetDatabase.LoadAssetAtPath<PuzzleBoardSO>(puzzleBoardSOPath); // 퍼즐보드SO 로드

            if (puzzleBoardSO == null) // 보드SO가 없으면
            {
                Debug.LogError($"PuzzleBoardSO 없음 : {puzzleBoardSOPath} / {i} 행 확인");
                return;
            }
            else // 보드SO가 있으면
            {
                stage.PuzzleBoard = puzzleBoardSO; // 퍼즐보드 설정
            }

            List<RecipeSO> recipeList = BuildRecipesFromBundle(stage_recipe_bundle, bundleLines); // 레시피 리스트 생성

            stage.StageRecipes = recipeList.ToArray(); // 배열로 대입

            List<StageSO.IngredientAdjustment> adjList = BuildAdjustmentsFromLevel(level_id, levelLines); // 배수 리스트 생성
            stage.IngredientAdjustments = adjList.ToArray(); // 배열로 대입

            //if (isNew == false) // 기존 SO였다면
            //{
            //    EditorUtility.SetDirty(stage); // 변경사항 저장 대상으로 표시
            //}

            EditorUtility.SetDirty(stage); // 변경사항 저장 대상으로 표시
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("== 스테이지 임포트 완료 ==");
    }

    private static List<RecipeSO> BuildRecipesFromBundle(int bundleId, string[] bundleLines) // 번들 ID로 레시피 리스트 만들기
    {
        List<RecipeSO> list = new List<RecipeSO>(); // 결과 리스트

        for (int i = startRow; i < bundleLines.Length; i++) // 데이터 행부터 끝까지 순회
        {
            string line = bundleLines[i];
            if (string.IsNullOrWhiteSpace(line)) // 빈 줄이면
            {
                continue; // 다음
            }

            string[] splitData = line.Split(',');

            int rowBundleId = int.Parse(splitData[0]);

            if (rowBundleId != bundleId) //이 id가 찾는게 아니면
            {
                continue; //다음
            }

            for (int j = 1; j < splitData.Length; j++) // 레시피 ID 열들 순회
            {
                string cell = splitData[j];

                if (cell.ToLower() == "null") // 값 없으면
                {
                    continue; // 다음
                }

                int recipe_id;

                if (int.TryParse(cell, out recipe_id) == false)
                {
                    Debug.LogWarning($"레시피 ID 파싱 실패 {bundleId} 의 {j} 열 확인 cell");
                    continue; // 스킵
                }

                string recipeSOPath = recipeSoDir + "/Recipe_" + recipe_id + ".asset"; // 레시피 SO 경로
                RecipeSO recipe = AssetDatabase.LoadAssetAtPath<RecipeSO>(recipeSOPath); // 레시피 SO 로드
                if (recipe == null) // SO 없으면
                {
                    Debug.LogError($"레시피SO 없음: {recipeSOPath} 번들: {bundleId}");
                    continue; // 스킵
                }

                list.Add(recipe); // 리스트에 추가
            }

            break; // 해당 번들 행을 찾았으니 루프 종료
        }

        return list; // 결과 리스트 반환
    }

    private static List<StageSO.IngredientAdjustment> BuildAdjustmentsFromLevel(int levelId, string[] levelLines) // 레벨 ID로 재료 배수 리스트 만들기
    {
        List<StageSO.IngredientAdjustment> list = new List<StageSO.IngredientAdjustment>(); // 결과 리스트

        if (levelLines == null || levelLines.Length < startRow) // CSV가 없거나 데이터 부족이면
        {
            Debug.LogError("레벨 배수 CSV 없음");
            return list;
        }

        for (int i = startRow; i < levelLines.Length; i++)
        {
            string line = levelLines[i];
            if (string.IsNullOrWhiteSpace(line)) // 빈 줄이면
            {
                continue; // 스킵
            }

            string[] splitData = line.Split(',');

            int rowLevelId = int.Parse(splitData[0]);  // 현재 행 레벨 ID

            if (rowLevelId != levelId) // 찾는 레벨이 아니면
            {
                continue; // 다음
            }

            for (int j = 1; j < splitData.Length; j += 2) // 1열부터 끝까지 2칸씩 증가
            {
                int ing_id; // 재료 ID
                float mul;  // 배수 값

                // 재료 ID 파싱
                if (int.TryParse(splitData[j], out ing_id) == false)
                {
                    Debug.LogError($"ingredient_id 파싱 실패 (레벨아이디 {levelId}  {j}열 {splitData[j]}");
                    continue;
                }

                // 배수 파싱
                if (float.TryParse(splitData[j + 1], out mul) == false)
                {
                    Debug.LogError($"mul 파싱 실패 (레벨아이디 {levelId}  {j}열 {splitData[j + 1]}");
                    continue;
                }

                // SO 로드
                string ingSOPath = ingredientSoDir + "/Ingredient_" + ing_id + ".asset";
                IngredientSO ing = AssetDatabase.LoadAssetAtPath<IngredientSO>(ingSOPath);

                if (ing == null)
                {
                    Debug.LogError($"재료SO 없음 : {ingSOPath} 레벨아이디 {levelId}");
                    continue;
                }

                // 조정값 추가
                StageSO.IngredientAdjustment adj = new StageSO.IngredientAdjustment();
                adj.Ingredient = ing;
                adj.MuliflyBy = mul;
                list.Add(adj);
            }

            break;
        }

        return list; // 결과 리스트 반환
    }
}
