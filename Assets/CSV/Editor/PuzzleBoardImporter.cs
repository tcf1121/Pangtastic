using SCR;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PuzzleBoardImporter
{
    private static string csvPath = "Assets/CSV/Puzzle Board.csv"; //csv 저장위치
    private static int startRow = 2;
    private static int columnCount = 3;

    // 메뉴 경로
    public static void StartImportBoardMap()
    {
        ImportPuzzleBoardCSV();
    }

    private static void ImportPuzzleBoardCSV()
    {
        if (File.Exists(csvPath) == false) // 경로에 파일 없으면
        {
            Debug.LogError("CSV 파일 없음: " + csvPath);
            return;
        }

        string[] lines = File.ReadAllLines(csvPath); // CSV 파일 모든 줄 읽기

        if (lines.Length < startRow) // 헤더만 있고 데이터가 없으면
        {
            Debug.LogError("재료 CSV에 데이터 없음");
            return;
        }

        string saveDir = "Assets/ScriptableObject/PuzzleBoards"; // SO 저장 경로

        if (Directory.Exists(saveDir) == false) // 폴더 없으면
        {
            Directory.CreateDirectory(saveDir); // 폴더 생성
        }

        for (int i = startRow; i < lines.Length; i++) // startRow 부터 시작
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line)) // 빈 줄이면
            {
                continue; // 건너뛰기
            }

            string[] splitData = line.Split(','); // 콤마로 열 나누기

            if (splitData.Length < columnCount) // 열이 부족하면
            {
                Debug.LogError("열 개수 부족함(행 " + i + "): " + line);
                return;
            }

            int id = int.Parse(splitData[0]);
            string arr = splitData[1];
            string pos = splitData[2];
            string reserve = "";

            if (splitData.Length >= 4)
            {
                reserve = splitData[3];
            }

            string soPath = saveDir + "/PuzzleBoard_" + id + ".asset"; // SO파일 저장경로/파일이름

            PuzzleBoardSO so = AssetDatabase.LoadAssetAtPath<PuzzleBoardSO>(soPath); // 기존 SO 불러오기

            if (so == null) // 경로에 "/PuzzleBoard_" + id + ".asset" 이름의 SO가 없으면
            {
                so = ScriptableObject.CreateInstance<PuzzleBoardSO>(); // 새 SO
                AssetDatabase.CreateAsset(so, soPath); // SO 생성
                Debug.Log("새 보드SO 생성: " + id);
            }
            else //이미 파일이 있으면
            {

                Debug.Log("기존 보드SO 갱신: " + id);
            }

            //데이터 적용
            so.BoardId = id;

            if (so.Cells == null) // 신규생성시 
            {
                so.Cells = new List<CellData>();
            }
            else // 덮어씌우기 할때
            {
                so.Cells.Clear();
            }

            if (so.SpawnPoints == null)
            {
                so.SpawnPoints = new List<Vector3Int>();
            }
            else
            {
                so.SpawnPoints.Clear();
            }

            if (so.ReservePositions == null)
            {
                so.ReservePositions = new List<Vector3Int>();
            }
            else
            {
                so.ReservePositions.Clear();
            }

            FillCells(arr, so.Cells); // 배열정보 파싱
            FillPositions(pos, so.SpawnPoints); // 스폰포인트 파싱

            FillPositions(reserve, so.ReservePositions); // 예비배열 파싱

            EditorUtility.SetDirty(so);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("== 퍼즐 보드SO 임포트 완료 ==");
    }

    private static void FillCells(string arr, List<CellData> list)
    {
        if (string.IsNullOrEmpty(arr)) //배열이 비었으면 리턴
        {
            Debug.LogError($"{arr} 비었음");
            return;
        }

        string[] parts = arr.Split('|');
        if (parts == null || parts.Length == 0)
        {
            Debug.LogError($"셀 비었음. {arr}");
            return;
        }

        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];

            if (string.IsNullOrWhiteSpace(part)) // 마지막에 |남는거 오류 방지
            {
                continue;
            }

            string[] posAndType = part.Split(':');

            if (posAndType == null || posAndType.Length < 4) //x,y,z,타입 아니면 리턴
            {
                Debug.LogError($"셀에 정보 부족 {part}");
                return;
            }

            int x;
            int y;
            int z;

            bool okX = int.TryParse(posAndType[0].Trim(), out x);
            bool okY = int.TryParse(posAndType[1].Trim(), out y);
            bool okZ = int.TryParse(posAndType[2].Trim(), out z);

            if (okX == false || okY == false || okZ == false) // 하나라도 실패하면
            {
                Debug.LogError($"좌표 파싱 실패: {part}");
                return;
            }

            GemType gem;
            bool okType = System.Enum.TryParse<GemType>(posAndType[3].Trim(), true, out gem);

            if (okType == false)
            {
                Debug.LogWarning($"GemType 파싱 실패: {posAndType[3]}, 셀 : {part}");
                return;
            }

            CellData cellData = new CellData();
            cellData.Position = new Vector3Int(x, y, z);
            cellData.gemType = gem;
            list.Add(cellData);
        }
    }

    private static void FillPositions(string pos, List<Vector3Int> outList)
    {
        if (string.IsNullOrEmpty(pos))
        {
            return;
        }

        string[] parts = pos.Split('|');
        if (parts == null || parts.Length == 0)
        {
            Debug.LogError($"{pos} 비었음");
            return;
        }

        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];

            if (string.IsNullOrWhiteSpace(part))
            {
                continue;
            }

            string[] positions = part.Split(':');

            if (positions == null || positions.Length < 3)
            {
                Debug.LogWarning($"잘못된 좌표 형식: {part}");
                continue;
            }

            string sx = positions[0].Trim();
            string sy = positions[1].Trim();
            string sz = positions[2].Trim();

            int x;
            int y;
            int z;

            bool okx = int.TryParse(sx, out x);
            bool oky = int.TryParse(sy, out y);
            bool okz = int.TryParse(sz, out z);

            if (okx == false || oky == false || okz == false) //하나라도 실패시
            {
                Debug.LogWarning($"좌표 파싱 실패: {part}");
                return;
            }

            Vector3Int position = new Vector3Int(x, y, z);
            outList.Add(position);
        }
    }
}
