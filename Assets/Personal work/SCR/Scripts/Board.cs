using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR
{
    public enum GemType
    {
        Lavender,
        Chocolate,
        Blueberry,
        Cheese,
        Strawberry,
        Sugar,
        RollingPin_v,
        RollingPin_h,
        Milk,
        Popcorn,
        DonutBox,
        Dough,
        Syrup,
        Ice,
        Cloche,
        Coin,
        GiftBox,
        Egg,
        CatStatues,
        CatStatues_s,
        Empty,
        Random
    }

    [DefaultExecutionOrder(-9999)]
    public class Board : MonoBehaviour
    {
        private static Board instance;
        public Tilemap myTilemap;
        [SerializeField] private int loadCell = 0;
        private int _cellCount;
        public List<Vector3Int> SpawnPoint = new();
        public List<Vector3Int> CellList = new();
        private List<Vector3Int> _matchedPositions = new List<Vector3Int>();
        private List<Vector3Int> splashDamageTargets = new List<Vector3Int>();
        public Dictionary<Vector3Int, GemType> CellGemType = new();
        public Dictionary<Vector3Int, BoardCell> CellContent = new();
        [SerializeField] private PrefabList prefabList;

        private Grid _grid;

        public void Awake()
        {
            instance = this;
            GetReference();
            instance._cellCount = GetTotalTilesOnMap();
        }

        public void Update()
        {
            // if (instance._cellCount == instance.loadCell)
            //     AllCheck();
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SortCells();
            }
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     AllCheck();
            // }
        }

        public static Dictionary<Vector3Int, GemType> GetPuzzleInfo()
        {
            return instance.CellGemType;
        }

        public static void SetPuzzleInfo(Dictionary<Vector3Int, GemType> PuzzleInfo)
        {
            foreach (var data in PuzzleInfo)
            {
                AddCell(data.Key);
                AddObject(data.Key, data.Value);
            }
        }

        private int GetTotalTilesOnMap()
        {
            int count = 0;
            // 타일맵의 유효한 셀 영역을 순회
            BoundsInt bounds = myTilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (myTilemap.HasTile(pos))
                {
                    count++;
                }
            }
            return count;
        }



        // 빈칸 추가
        public static void AddCell(Vector3Int pos)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            instance.CellList.Add(pos);
        }

        // 스포너 추가
        public static void AddSpawner(Vector3Int pos)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            instance.SpawnPoint.Add(pos);
        }

        // 오브젝트 추가
        public static void AddObject(Vector3Int pos, GemType gem)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            if (!instance.CellContent.ContainsKey(pos))
            {
                instance.CellGemType.Add(pos, gem);
                instance.CellContent.Add(pos, new BoardCell(pos, gem));
                instance.loadCell++;
            }

            else instance.CellContent[pos].SetObject(gem);

        }


        // 도넛 생성
        public static Donut GetDonut(Vector3Int pos, GemType donut)
        {
            var newDonut = Instantiate(Board.GetPrefab(donut));
            newDonut.transform.position = pos;
            return newDonut.GetComponent<Donut>();
        }

        // 장애물 생성
        public static Obstacle GetObstacle(Vector3Int pos, GemType obstacle)
        {
            var newDonut = Instantiate(Board.GetPrefab(obstacle));
            newDonut.transform.position = pos;
            return newDonut.GetComponent<Obstacle>();
        }

        // 블록 삭제
        public static void RemoveGem(Vector3Int pos)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            instance.CellContent[pos].RemoveCell();
        }

        // 블록 삭제
        public static GameObject GetPrefab(GemType gemType)
        {
            return instance.prefabList.GemPrefab[(int)gemType];
        }

        // 움직일 수 있는 블럭인지 확인
        public static bool IsCanMove(Vector3Int pos)
        {
            if (!instance.CellContent.ContainsKey(pos))
                return false;

            return instance.CellContent[pos].CanMove();
        }
        // 움직이는 오브젝트일때 밑에가 비어있는지 확인 후 비어있으면 내려가기
        public static void IsEmpty(Vector3Int pos)
        {
            Vector3Int movePos;
            // 비어있으면
            if (!instance.CellContent.ContainsKey(pos) &&
                instance.CellContent[pos].IsEmpty())
            {
                // 아래 부터 확인
                if (IsCanMove(movePos = new Vector3Int(pos.x, pos.y - 1)))
                {
                    MoveDown(movePos, pos);
                }
                // 아니면 왼쪽 대각선 위
                else if (IsCanMove(movePos = new Vector3Int(pos.x - 1, pos.y - 1)))
                {
                    MoveDown(movePos, pos);
                }
                // 아니면 오른쪽 대각선 위
                else if (IsCanMove(movePos = new Vector3Int(pos.x + 1, pos.y - 1)))
                {
                    MoveDown(movePos, pos);
                }
                // 아무것도 찾지 못했으면 아무것도 하지 않는다.
                else
                {
                    return;
                }
            }
        }

        // 비어있으면 위에서 가져옴
        public static void MoveDown(Vector3Int pos, Vector3Int moveToPos)
        {
            instance.CellContent[pos].MoveObject(instance.CellContent[moveToPos]);
        }

        // 두 컨텐츠의 위치를 바꿈
        public static void Change(Vector3Int pos, Vector3Int moveToPos)
        {
            Donut changeDonut = instance.CellContent[moveToPos].GetDonut();
            Obstacle changeObstacle = instance.CellContent[moveToPos].GetObstacle();
            instance.CellContent[pos].MoveObject(instance.CellContent[moveToPos]);
            instance.CellContent[pos].SetDonut(changeDonut);
            instance.CellContent[pos].SetObstacle(changeObstacle);
        }

        public static Dictionary<Vector3Int, BoardCell> GetDictionary()
        {
            return instance.CellContent;
        }

        public void SortCells()
        {
            CellList = CellList.OrderBy(pos => pos.y).ThenBy(pos => pos.x).ToList();
        }

        private void AllCheck()
        {
            foreach (Vector3Int pos in CellList)
                CheckMatch(pos);
        }

        private void CheckMatch(Vector3Int pos)
        {
            // 가로 매치 확인
            int horizontalCount = CheckDirection(pos, Vector3Int.left) + CheckDirection(pos, Vector3Int.right) + 1;
            if (horizontalCount >= 3)
            {
                AddMatchToList(pos, Vector3Int.left);
                AddMatchToList(pos, Vector3Int.right);
            }

            // 세로 매치 확인
            int verticalCount = CheckDirection(pos, Vector3Int.up) + CheckDirection(pos, Vector3Int.down) + 1;
            if (verticalCount >= 3)
            {
                AddMatchToList(pos, Vector3Int.up);
                AddMatchToList(pos, Vector3Int.down);
            }

            // 매치된 블록이 있을 경우
            if (_matchedPositions.Count > 0)
            {
                // 중복 제거
                List<Vector3Int> distinctMatches = _matchedPositions.Distinct().ToList();

                // 모든 매치 블록에 Damage 호출
                foreach (var matchedPos in distinctMatches)
                {
                    if (instance.CellContent.ContainsKey(matchedPos))
                    {
                        instance.CellContent[matchedPos].Damage();
                    }
                }

                foreach (var matchedPos in distinctMatches)
                {
                    AddSplashTargets(matchedPos);
                }

                // 4. 스플래시 데미지 대상 리스트에서 매치 블록과 중복을 제거
                List<Vector3Int> uniqueSplashTargets = splashDamageTargets
                    .Distinct().ToList();

                // 5. 스플래시 데미지 대상에 SplashDamage() 함수를 호출
                foreach (var splashPos in uniqueSplashTargets)
                {
                    if (instance.CellContent.ContainsKey(splashPos))
                    {
                        instance.CellContent[splashPos].SplashDamage();
                    }
                }

                _matchedPositions.Clear(); // 다음 매치 확인을 위해 리스트 초기화
            }
        }

        private int CheckDirection(Vector3Int startPos, Vector3Int direction)
        {
            int count = 0;
            Vector3Int currentPos = startPos + direction;
            if (instance.CellContent[startPos].getCellType() == GemType.Empty) return 0;
            while (instance.CellContent.ContainsKey(currentPos) &&
           instance.CellContent[currentPos].getCellType() == instance.CellContent[startPos].getCellType())
            {
                count++;
                currentPos += direction;
            }

            return count;
        }

        private void AddMatchToList(Vector3Int startPos, Vector3Int direction)
        {
            _matchedPositions.Add(startPos); // 시작 블록 추가
            Vector3Int currentPos = startPos + direction;

            while (instance.CellContent.ContainsKey(currentPos) &&
                   instance.CellContent[currentPos].getCellType() == instance.CellContent[startPos].getCellType())
            {
                _matchedPositions.Add(currentPos);
                currentPos += direction;
            }
        }

        // 매치된 블록의 주변 블록을 splashDamageTargets 리스트에 추가하는 함수
        private void AddSplashTargets(Vector3Int centerPos)
        {
            Vector3Int[] directions = new Vector3Int[]
            {
                new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
                new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0)
            };

            foreach (var direction in directions)
            {
                Vector3Int targetPos = centerPos + direction;
                if (instance.CellContent.ContainsKey(targetPos))
                {
                    splashDamageTargets.Add(targetPos);
                }
            }
        }




        private void GetReference()
        {
            _grid = GetComponent<Grid>();
        }
    }
}
