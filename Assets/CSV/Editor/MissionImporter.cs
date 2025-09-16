using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MissionImporter
{
    private static string csvPathone = "Assets/CSV/Mission/"; // 미션 CSV 경로
    private static string stringSoPath = "Assets/ScriptableObject/String/"; // 스트링 SO 경로
    private static string missionSoDir = "Assets/ScriptableObject/Mission/"; // 미션 SO 저장 경로
    private static int startRow = 1; // 데이터 시작 행
    private static int columnCount = 6; //열 개수
                                        //private static bool isNew;
    private static List<string> places;

    // 메뉴 경로
    [MenuItem("PangTastic/Import Mission")]
    public static void StartImportMissions()
    {
        AddPlace();
        foreach (string place in places)
            ImportMissionCSV(place);
    }

    private static void AddPlace()
    {
        places = new()
        {
            "Donut",
            "MiniCafe",
            "Cafe",
            "IceCream",
            "Bakery",
            "Pizzeria",
            "Bar",
            "Greengrocery"
        };
    }

    private static void ImportMissionCSV(string place)
    {
        string csvPath = $"{csvPathone}{place}.csv";
        if (File.Exists(csvPath) == false) // CSV 파일 없으면
        {
            Debug.LogError("미션 CSV 파일 없음: " + csvPath);
            return;
        }

        string[] lines = File.ReadAllLines(csvPath);

        if (lines.Length < startRow) // 데이터가 시작되는 행보다 줄이 적으면
        {
            Debug.LogError("미션 CSV에 데이터 없음");
            return;
        }

        if (Directory.Exists(missionSoDir) == false) //폴더 없으면
        {
            Directory.CreateDirectory(missionSoDir); // 폴더 생성
        }

        string soPath = $"{missionSoDir}{place}.asset"; // SO파일 저장경로/파일이름
        Debug.Log(soPath);
        MissionSO mission = AssetDatabase.LoadAssetAtPath<MissionSO>(soPath); // 기존 손님 SO 불러오기
        Debug.Log(mission);
        if (mission == null) // 경로에 "/Ingredient_" + id + ".asset" 이름의 SO가 없으면
        {
            mission = ScriptableObject.CreateInstance<MissionSO>(); // 새 SO
            AssetDatabase.CreateAsset(mission, soPath); // SO 생성
            Debug.Log("새 미션SO 생성: ");
        }
        else //이미 파일이 있으면
        {
            Debug.Log("기존 미션SO 갱신: ");
        }
        mission.Place = (MissionPlace)Enum.Parse(typeof(MissionPlace), place);
        mission.Mission = new();
        for (int i = startRow; i < lines.Length; i++) // 데이터 행부터 끝까지 순회
        {
            Mission missionList = new();

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

            int.TryParse(splitData[0], out missionList.MissionID);
            int.TryParse(splitData[1], out missionList.Star);
            int.TryParse(splitData[2], out missionList.Prerequisites);
            missionList.Explane = AssetDatabase.LoadAssetAtPath<StringSO>(stringSoPath + splitData[3] + ".asset");

            mission.Mission.Add(missionList);

        }
        EditorUtility.SetDirty(mission); // 변경사항 저장에 포함

        AssetDatabase.SaveAssets(); // 저장
        AssetDatabase.Refresh(); // 새로고침
        Debug.Log("== 미션 임포트 완료 ==");
    }
}
