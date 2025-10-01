using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class RewardImporter // 보상 CSV를 RewardSO로 변환하는 클래스
{
    private static string csvPath = "Assets/CSV/Reward.csv"; // 레시피 CSV 경로
    private static string rewardSoDir = "Assets/ScriptableObject/Reward"; // 레시피 SO 저장 경로
    private static int startRow = 3; // 데이터 시작 행
    private static int columnCount = 3; //열 개수
                                        //private static bool isNew;

    // 메뉴 경로
    public static void StartImportReward()
    {
        ImportRewardCSV();
    }

    private static void ImportRewardCSV()
    {
        if (File.Exists(csvPath) == false) // CSV 파일 없으면
        {
            Debug.LogError("보상 CSV 파일 없음: " + csvPath);
            return;
        }

        string[] lines = File.ReadAllLines(csvPath);

        if (lines.Length < startRow) // 데이터가 시작되는 행보다 줄이 적으면
        {
            Debug.LogError("보상 CSV에 데이터 없음");
            return;
        }

        if (Directory.Exists(rewardSoDir) == false) //폴더 없으면
        {
            Directory.CreateDirectory(rewardSoDir); // 폴더 생성
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

            int.TryParse(splitData[0], out int reward_id);
            string reward_explane = splitData[1];


            for (int j = 2; j < splitData.Length; j += 2)
            {

            }

            string soPath = rewardSoDir + "/" + reward_id + ".asset"; // SO파일 저장경로/파일이름

            RewardSO rewards = AssetDatabase.LoadAssetAtPath<RewardSO>(soPath); // 기존 레시피 SO 불러오기

            if (rewards == null) // 경로에 "/Ingredient_" + id + ".asset" 이름의 SO가 없으면
            {
                rewards = ScriptableObject.CreateInstance<RewardSO>(); // 새 SO
                rewards.Rewards = new();
                AssetDatabase.CreateAsset(rewards, soPath); // SO 생성
                Debug.Log("새 스트링SO 생성: " + reward_id);
                //isNew = true;
            }
            else //이미 파일이 있으면
            {
                Debug.Log("기존 스트링SO 갱신: " + reward_id);
                //isNew = false;
            }

            rewards.RewardID = reward_id;
            rewards.RewardExplane = reward_explane;
            rewards.Rewards.Clear();
            for (int j = 2; j < splitData.Length; j += 2)
            {
                if (splitData[j] == "") break;
                int.TryParse(splitData[j], out int goodsNum);
                string numberPart = Regex.Match(splitData[j + 1], @"\d+").Value;
                int.TryParse(numberPart, out int count);
                Rewards newReward = new()
                {
                    GoodsType = GetGoods(goodsNum),
                    Count = count
                };

                rewards.Rewards.Add(newReward);
            }

            EditorUtility.SetDirty(rewards); // 변경사항 저장에 포함
        }

        AssetDatabase.SaveAssets(); // 저장
        AssetDatabase.Refresh(); // 새로고침
        Debug.Log("== 스트링 임포트 완료 ==");
    }

    private static Goods GetGoods(int num)
    {
        int index = num - 10001;
        if (index == 1) index = 9;
        else if (index > 1) index--;

        return (Goods)index;
    }

}
