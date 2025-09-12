using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CustomerImporter
{
    private static string csvPath = "Assets/CSV/Customers.csv"; // 손님 CSV 경로
    private static string recipeSoDir = "Assets/ScriptableObject/Recipes"; // 레시피 SO경로
    private static string stringSoDir = "Assets/ScriptableObject/String"; // 레시피 SO경로
    private static string customerSoDir = "Assets/ScriptableObject/Customers"; // 손님 SO 저장 경로
    private static int startRow = 3; // 데이터 시작 행
    private static int columnCount = 14; //열 개수
                                         //private static bool isNew;

    // 메뉴 경로
    public static void StartImportCustomers()
    {
        ImportCustomerCSV();
    }

    private static void ImportCustomerCSV()
    {
        if (File.Exists(csvPath) == false) // CSV 파일 없으면
        {
            Debug.LogError("손님 CSV 파일 없음: " + csvPath);
            return;
        }

        string[] lines = File.ReadAllLines(csvPath);

        if (lines.Length < startRow) // 데이터가 시작되는 행보다 줄이 적으면
        {
            Debug.LogError("손님 CSV에 데이터 없음");
            return;
        }

        if (Directory.Exists(customerSoDir) == false) //폴더 없으면
        {
            Directory.CreateDirectory(customerSoDir); // 폴더 생성
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

            int.TryParse(splitData[0], out int customer_id);
            string customer_name = splitData[1];
            string customer_type = splitData[2];
            string spritePath = splitData[3];

            //선호 레시피 파싱
            int like_menu1 = (splitData[4].ToLower() == "null") ? 0 : int.Parse(splitData[4]); //값이 null(소문자 변경)이면 0, 아니면 파싱
            int like_menu2 = (splitData[5].ToLower() == "null") ? 0 : int.Parse(splitData[5]);
            int like_menu3 = (splitData[6].ToLower() == "null") ? 0 : int.Parse(splitData[6]);
            int like_menu4 = (splitData[7].ToLower() == "null") ? 0 : int.Parse(splitData[7]);
            int like_menu5 = (splitData[8].ToLower() == "null") ? 0 : int.Parse(splitData[8]);
            int like_menu6 = (splitData[9].ToLower() == "null") ? 0 : int.Parse(splitData[9]);
            int like_menu7 = (splitData[10].ToLower() == "null") ? 0 : int.Parse(splitData[10]);
            int like_menu8 = (splitData[11].ToLower() == "null") ? 0 : int.Parse(splitData[11]);
            int like_menu9 = (splitData[12].ToLower() == "null") ? 0 : int.Parse(splitData[12]);
            int like_menu10 = (splitData[13].ToLower() == "null") ? 0 : int.Parse(splitData[13]);

            string soPath = customerSoDir + "/Customer_" + customer_id + ".asset"; // SO파일 저장경로/파일이름

            CustomerSO customer = AssetDatabase.LoadAssetAtPath<CustomerSO>(soPath); // 기존 손님 SO 불러오기

            if (customer == null) // 경로에 "/Ingredient_" + id + ".asset" 이름의 SO가 없으면
            {
                customer = ScriptableObject.CreateInstance<CustomerSO>(); // 새 SO
                AssetDatabase.CreateAsset(customer, soPath); // SO 생성
                Debug.Log("새 손님SO 생성: " + customer_name);
                //isNew = true;
            }
            else //이미 파일이 있으면
            {
                Debug.Log("기존 손님SO 갱신: " + customer_name);
                //isNew = false;
            }

            customer.ID = customer_id;
            customer.Name = customer_name;
            customer.CustomerPic = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            if (customer.CustomerPic == null)
            {
                Debug.LogError($"손님 이미지 없음 {customer.Name}, 경로 : {spritePath}");
                //return;
            }

            CustomerType customerType;

            if (System.Enum.TryParse<CustomerType>(customer_type, true, out customerType)) // string > enum 파싱 시도
            {
                customer.Type = customerType;
            }
            else
            {
                Debug.LogError($"손님타입 파싱 실패 {i}행 확인 {customer_type}");
                return;
            }

            // 선호메뉴 목록
            List<RecipeSO> favList = new List<RecipeSO>();

            AddFavRecipe(like_menu1, favList, i); //레시피1 추가 i는 디버깅용
            AddFavRecipe(like_menu2, favList, i); //레시피2 추가
            AddFavRecipe(like_menu3, favList, i); //레시피3 추가
            AddFavRecipe(like_menu4, favList, i);
            AddFavRecipe(like_menu5, favList, i);
            AddFavRecipe(like_menu6, favList, i);
            AddFavRecipe(like_menu7, favList, i);
            AddFavRecipe(like_menu8, favList, i);
            AddFavRecipe(like_menu9, favList, i);
            AddFavRecipe(like_menu10, favList, i);

            customer.FavoriteRecipes = favList.ToArray();

            customer.DialogueEnter = AddDialogue(customer_id, "enter");
            customer.DialogueHigh = AddDialogue(customer_id, "high");
            customer.DialogueMid = AddDialogue(customer_id, "mid");
            customer.DialogueLeft = AddDialogue(customer_id, "left");


            EditorUtility.SetDirty(customer); // 변경사항 저장에 포함
        }

        AssetDatabase.SaveAssets(); // 저장
        AssetDatabase.Refresh(); // 새로고침
        Debug.Log("== 손님 임포트 완료 ==");
    }
    private static void AddFavRecipe(int recipeID, List<RecipeSO> list, int line)
    {
        if (recipeID == 0) //레시피가 없으면 안넣음
        {
            return;
        }

        string recipeSOPath = recipeSoDir + "/Recipe_" + recipeID + ".asset"; // 레시피 SO 경로
        RecipeSO recipe = AssetDatabase.LoadAssetAtPath<RecipeSO>(recipeSOPath); // 레시피SO 로드

        if (recipe == null) //레시피가 없으면
        {
            Debug.LogError($"레시피SO 없음 : {line}행 확인. 레시피 경로: {recipeSOPath}");
            return;
        }

        list.Add(recipe);
    }

    private static StringSO AddDialogue(int customerId, string level)
    {
        string stringSOPath = stringSoDir + "/customer_" + customerId + "_" + level + ".asset";
        StringSO stringSO = AssetDatabase.LoadAssetAtPath<StringSO>(stringSOPath);

        if (stringSO == null)
        {
            Debug.LogError($"StringSO 없음 : {stringSOPath}");
            return stringSO;
        }
        else
        {
            return stringSO;
        }
    }
}
