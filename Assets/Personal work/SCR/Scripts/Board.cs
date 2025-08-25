using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
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
        public Tilemap BlankTilemap;
        [SerializeField] TileBase blankTile;
        [SerializeField] private int loadCell = 0;
        private int _cellCount;
        public List<Vector3Int> SpawnPoint = new();
        public List<Vector3Int> CellList = new();
        private List<Vector3Int> _emptyPositions = new List<Vector3Int>();
        private List<Vector3Int> _matchedPositions = new List<Vector3Int>();
        private List<Vector3Int> _splashDamageTargets = new List<Vector3Int>();
        public Dictionary<Vector3Int, GemType> CellGemType = new();
        public Dictionary<Vector3Int, BoardCell> CellContent = new();
        [SerializeField] private PrefabList prefabList;

        private Vector3Int _clickPos;
        private Vector3Int _dragDir;

        private Coroutine allCheckCor;
        private Coroutine allEmptyCor;

        private Grid _grid;

        public void Awake()
        {
            instance = this;
            GetReference();
            instance._cellCount = GetTotalTilesOnMap();
        }

        public void Update()
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            //     AllCheck();


            if (Input.GetKeyDown(KeyCode.Q))
            {
                SortCells();
            }

        }

        public static void SetClickPos(Vector3Int pos)
        {
            instance._clickPos = pos;
        }

        public static void SetDragDir(Vector3Int Dir)
        {
            instance._dragDir = Dir;
            if (instance._clickPos != null && instance._dragDir != Vector3Int.zero)
            {
                instance.StartCoroutine(SwapCor(instance._clickPos, instance._clickPos + instance._dragDir));
            }
        }
        public static Dictionary<Vector3Int, GemType> GetPuzzleInfo()
        {
            return instance.CellGemType;
        }

        public static List<Vector3Int> GetSpawnPoint()
        {
            return instance.SpawnPoint;
        }

        public static void SetPuzzleInfo(Dictionary<Vector3Int, GemType> PuzzleInfo, List<Vector3Int> SpawnPoint)
        {
            do
            {
                instance.CellList.Clear();
                instance.CellGemType.Clear();
                instance.CellContent.Clear();
                foreach (var data in PuzzleInfo)
                {
                    AddCell(data.Key);
                    AddObject(data.Key, data.Value);
                }

            } while (instance.IsStartMatch());

            instance.InitObject();

            foreach (var data in SpawnPoint)
            {
                AddSpawner(data);
            }
        }

        private bool IsStartMatch()
        {
            foreach (Vector3Int pos in CellList)
                CheckMatch(pos);
            _matchedPositions = _matchedPositions.Distinct().ToList();
            if (_matchedPositions.Count > 0)
            {
                _matchedPositions.Clear();
                return true;
            }
            else
            {
                _matchedPositions.Clear();
                return false;
            }
        }

        private int GetTotalTilesOnMap()
        {
            int count = 0;
            // 타일맵의 유효한 셀 영역을 순회
            BoundsInt bounds = BlankTilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (BlankTilemap.HasTile(pos))
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
            instance.BlankTilemap.SetTile(pos, instance.blankTile);
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

        public void InitObject()
        {
            foreach (var data in CellContent)
                data.Value.Init();
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

        // 
        public static GameObject GetPrefab(GemType gemType)
        {
            return instance.prefabList.GemDatas[(int)gemType].GemPrefab;
        }

        // 움직일 수 있는 블럭인지 확인
        public static bool IsCanMove(Vector3Int pos)
        {
            if (!instance.CellContent.ContainsKey(pos))
                return false;

            return instance.CellContent[pos].CanMove();
        }


        // 두 컨텐츠의 위치를 바꿈
        public static IEnumerator SwapCor(Vector3Int pos, Vector3Int moveToPos, bool back = false)
        {
            if (!instance.CellContent[moveToPos].CanMove() || instance.CellContent[moveToPos].IsEmpty()) yield break;
            Donut changeDonut = instance.CellContent[moveToPos].GetDonut();
            Obstacle changeObstacle = instance.CellContent[moveToPos].GetObstacle();
            instance.CellContent[pos].MoveObject(instance.CellContent[moveToPos]);
            instance.CellContent[pos].SetDonut(changeDonut);
            instance.CellContent[pos].SetObstacle(changeObstacle);
            instance.StartCoroutine(instance.CellContent[pos].SetPos(0.2f));
            instance.StartCoroutine(instance.CellContent[moveToPos].SetPos(0.2f));
            yield return new WaitForSeconds(0.2f);
            if (!back)
                instance.CheckSwap(pos, moveToPos);
            if (instance.allCheckCor == null)
                instance.allCheckCor = instance.StartCoroutine(instance.AllCheck());

        }

        public static IEnumerator DownCor(Vector3Int pos, Vector3Int moveToPos)
        {
            instance.CellContent[pos].MoveObject(instance.CellContent[moveToPos]);
            instance.CellContent[pos].SetDonut(null);
            instance.CellContent[pos].SetObstacle(null);
            instance.StartCoroutine(instance.CellContent[moveToPos].SetPos(0.1f));
            yield return new WaitForSeconds(0.2f);
        }

        public static Dictionary<Vector3Int, BoardCell> GetDictionary()
        {
            return instance.CellContent;
        }

        public void SortCells()
        {
            CellList = CellList.OrderBy(pos => pos.y).ThenBy(pos => pos.x).ToList();
        }

        private void CheckSwap(Vector3Int firstPos, Vector3Int secondPos)
        {
            _matchedPositions.Clear();
            _splashDamageTargets.Clear();
            CheckMatch(firstPos);
            CheckMatch(secondPos);
            _matchedPositions = _matchedPositions.Distinct().ToList();
            _splashDamageTargets = _splashDamageTargets
                    .Distinct().Except(_matchedPositions.Distinct()).ToList();
            if (_matchedPositions.Count > 0)
            {
                DamageCheck();
            }
            else
            {
                StartCoroutine(SwapCor(firstPos, secondPos, true));
            }
        }

        private IEnumerator AllCheckEmpty()
        {
            while (true)
            {
                foreach (Vector3Int pos in CellList)
                    if (instance.CellContent.ContainsKey(pos))
                        if (instance.CellContent[pos].IsEmpty())
                        {
                            _emptyPositions.Add(pos);
                        }
                if (_emptyPositions.Count == 0)
                {
                    allEmptyCor = null;
                    if (allCheckCor == null) allCheckCor = StartCoroutine(AllCheck());
                    yield break;
                }
                else if (_emptyPositions.Count > 0) yield return StartCoroutine(FullEmpty());

            }

        }

        private IEnumerator FullEmpty()
        {
            foreach (Vector3Int pos in _emptyPositions)
            {
                IsEmpty(pos);
                yield return new WaitForFixedUpdate();
            }
            _emptyPositions.Clear();
        }

        public void IsEmpty(Vector3Int pos)
        {
            Vector3Int upPos = pos + Vector3Int.up;
            if (instance.SpawnPoint.Contains(upPos))
            {
                AddObject(pos, GemType.Random);
                return;
            }
            if (instance.CellContent.ContainsKey(upPos))
            {

                if (instance.CellContent[upPos].CanMove())
                    if (!instance.CellContent[upPos].IsEmpty())
                    {
                        StartCoroutine(DownCor(upPos, pos));
                        return;
                    }
                    else
                    {
                        return;
                    }
            }
            upPos = pos + Vector3Int.left;
            if (instance.CellContent.ContainsKey(upPos))
            {
                if (instance.CellContent[upPos].CanMove())
                    if (!instance.CellContent[upPos].IsEmpty())
                    {
                        StartCoroutine(DownCor(upPos, pos));
                        return;
                    }
                    else
                    {
                        return;
                    }
            }
            upPos = pos + Vector3Int.right + Vector3Int.right;
            if (instance.CellContent.ContainsKey(upPos))
            {
                if (instance.CellContent[upPos].CanMove())
                    if (!instance.CellContent[upPos].IsEmpty())
                    {
                        StartCoroutine(DownCor(upPos, pos));
                        return;
                    }
                    else
                    {
                        return;
                    }
            }
        }

        private IEnumerator AllCheck()
        {
            while (true)
            {
                foreach (Vector3Int pos in CellList)
                    CheckMatch(pos);
                if (_matchedPositions.Count == 0)
                {
                    allCheckCor = null;
                    yield break;
                }
                else if (_matchedPositions.Count > 0)
                {
                    _matchedPositions = _matchedPositions.Distinct().ToList();
                    if (_splashDamageTargets.Count > 0)
                        _splashDamageTargets = _splashDamageTargets
                                .Distinct().Except(_matchedPositions.Distinct()).ToList();
                    DamageCheck();
                }
                yield return new WaitForSeconds(0.2f);
                if (instance.allEmptyCor == null)
                    instance.allEmptyCor = instance.StartCoroutine(instance.AllCheckEmpty());
            }

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
                _matchedPositions = _matchedPositions.Distinct().ToList();

                foreach (var matchedPos in _matchedPositions)
                {
                    AddSplashTargets(matchedPos);
                }

                // 스플래시 데미지 대상 리스트에서 매치 블록과 중복을 제거
                if (_splashDamageTargets.Count > 0)
                    _splashDamageTargets = _splashDamageTargets
                        .Distinct().Except(_matchedPositions.Distinct()).ToList();
            }
        }



        private void DamageCheck()
        {
            int addEmpty = _matchedPositions.Count + _splashDamageTargets.Count;
            foreach (var matchedPos in _matchedPositions)
            {
                if (instance.CellContent.ContainsKey(matchedPos))
                {
                    if (instance.CellContent[matchedPos].GetCatStatuse() == GemType.CatStatues_s)
                    {
                        if (!_matchedPositions.Contains(CatStatuesPos(matchedPos)))
                            instance.CellContent[CatStatuesPos(matchedPos)].Damage();
                    }
                    else instance.CellContent[matchedPos].Damage();
                }
            }
            foreach (var splashPos in _splashDamageTargets)
            {
                if (instance.CellContent.ContainsKey(splashPos))
                {
                    if (instance.CellContent[splashPos].GetCatStatuse() == GemType.CatStatues_s)
                    {
                        if (!_splashDamageTargets.Contains(CatStatuesPos(splashPos)))
                            instance.CellContent[CatStatuesPos(splashPos)].SplashDamage();
                    }
                    else instance.CellContent[splashPos].SplashDamage();
                }
            }

            _matchedPositions.Clear(); // 다음 매치 확인을 위해 리스트 초기화
            _splashDamageTargets.Clear(); // 다음 매치 확인을 위해 리스트 초기화
        }

        private Vector3Int CatStatuesPos(Vector3Int cat_sPos)
        {
            Vector3Int pos;
            pos = cat_sPos + Vector3Int.down;
            if (instance.CellContent.ContainsKey(pos))
            {
                if (instance.CellContent[pos].GetCatStatuse() == GemType.CatStatues)
                {
                    return pos;
                }
            }
            pos = cat_sPos + Vector3Int.left;
            if (instance.CellContent.ContainsKey(pos))
            {
                Debug.Log(instance.CellContent[pos].getCellType());
                if (instance.CellContent[pos].GetCatStatuse() == GemType.CatStatues)
                {
                    return pos;
                }
            }
            pos = pos + Vector3Int.down;
            if (instance.CellContent.ContainsKey(pos))
            {
                if (instance.CellContent[pos].GetCatStatuse() == GemType.CatStatues)
                {
                    return pos;
                }
            }
            return Vector3Int.zero;
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
                    _splashDamageTargets.Add(targetPos);
                }
            }
        }




        private void GetReference()
        {
            _grid = GetComponent<Grid>();
        }
    }
}
