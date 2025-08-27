using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Roller_v,
        Roller_h,
        Milk,
        Oven,
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

    public class MatchData
    {
        public GemType MatchType;
        public List<Vector3Int> MatchPos;
        public List<Vector3Int> SplashPos;
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
        private List<Vector3Int> _emptyPositions = new();
        private List<Vector3Int> _matchedPositions = new();
        private List<Vector3Int> _splashDamageTargets = new();
        private List<Vector3Int> _milkTargets = new();
        private List<GemType> _spawnType = new();
        private List<MatchData> _matchData = new();
        private Dictionary<Vector3Int, GemType> _addSpecialPos = new();
        public Dictionary<Vector3Int, GemType> CellGemType = new();
        public Dictionary<Vector3Int, BoardCell> CellContent = new();

        [SerializeField] private PrefabList prefabList;
        private bool _isSpecialEffectActive = false;

        private Vector3Int _clickPos;
        private Vector3Int _dragDir;

        private Coroutine allCheckCor;
        private Coroutine allEmptyCor;
        private Coroutine _turnCor;

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
                instance.StartCoroutine(instance.HandleTurn(instance._clickPos, instance._clickPos + instance._dragDir));
            }
        }

        public static void SetUseItem(Vector3Int pos)
        {
            if (instance._clickPos != null)
            {
                instance.StartCoroutine(instance.HandleTurn(pos, pos, true));
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
                    if (data.Value <= GemType.Sugar ||
                        data.Value == GemType.Egg ||
                        data.Value == GemType.Coin ||
                        data.Value == GemType.Random)
                        if (!instance._spawnType.Contains(data.Value))
                        {
                            instance._spawnType.Add(data.Value);
                        }
                }

            } while (instance.IsStartMatch());
            instance.ArrangeSpawn();
            instance.InitObject();

            foreach (var data in SpawnPoint)
            {
                AddSpawner(data);
            }
        }

        public void InitObject()
        {
            foreach (var data in CellContent)
            {

                data.Value.Init();

            }
        }

        private bool IsStartMatch()
        {
            foreach (Vector3Int pos in CellList)
                CheckMatch(pos, true);
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

        public static void DrawObject(Vector3Int pos, GemType gem)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            if (!instance.CellGemType.ContainsKey(pos))
            {
                instance.CellGemType.Add(pos, gem);
            }
            else
                instance.CellGemType[pos] = gem;
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
        // 스폰 오브젝트 설정
        public GemType SetSpawn()
        {
            var obstacleTypes = instance._spawnType.Where(v => v == GemType.Coin || v == GemType.Egg).ToList();
            if (obstacleTypes.Count > 0)
            {
                int num = Random.Range(0, 10);
                if (num < 1)
                {
                    return obstacleTypes[Random.Range(0, obstacleTypes.Count)];
                }
            }

            var normalTypes = instance._spawnType.Where(v => v < GemType.Roller_v).ToList();

            if (normalTypes.Count > 0)
            {
                return normalTypes[Random.Range(0, normalTypes.Count)];
            }

            return GemType.Random;
        }
        // 스폰 오브젝트 정리
        public void ArrangeSpawn()
        {
            if (instance._spawnType.Contains(GemType.Random))
            {
                for (int i = 0; i < 6; i++)
                    if (instance._spawnType.Contains((GemType)i))
                        instance._spawnType.Remove((GemType)i);
            }
        }


        // 도넛 생성
        public static Donut GetDonut(Vector3Int pos, GemType donut)
        {
            var newDonut = Instantiate(Board.GetPrefab(donut));
            newDonut.transform.position = pos;
            return newDonut.GetComponent<Donut>();
        }

        // 방해 블록 생성
        public static Obstacle GetObstacle(Vector3Int pos, GemType obstacle)
        {
            var newDonut = Instantiate(Board.GetPrefab(obstacle));
            newDonut.transform.position = pos;
            return newDonut.GetComponent<Obstacle>();
        }

        // 특수 블록 생성
        public static Special GetSpecial(Vector3Int pos, GemType special)
        {
            var newSpecial = Instantiate(Board.GetPrefab(special));
            newSpecial.transform.position = pos;
            return newSpecial.GetComponent<Special>();
        }

        // 블록 삭제
        public static void RemoveCatStatues(Vector3Int pos)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            if (instance.CellContent[pos].GetCatStatuse() == GemType.CatStatues)
            {
                instance.CellContent[pos + Vector3Int.up].DestroyCat();
                instance.CellContent[pos + Vector3Int.right].DestroyCat();
                instance.CellContent[pos + Vector3Int.up + Vector3Int.right].DestroyCat();
            }
        }

        // 게임 오브젝트 생성
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

        public void StartPlayerTurn(Vector3Int pos1, Vector3Int pos2)
        {
            // 이미 턴이 진행 중이면 새로운 턴을 시작하지 않습니다.
            if (_turnCor != null) return;
            _turnCor = StartCoroutine(HandleTurn(pos1, pos2));
        }

        private IEnumerator HandleTurn(Vector3Int pos1, Vector3Int pos2, bool UseSpeical = false)
        {
            yield return StartCoroutine(SwapCor(pos1, pos2));
            bool first = true;
            while (true)
            {
                if (first)
                {
                    if (UseSpeical) yield return StartCoroutine(IsSpeical(pos1));
                    else
                    {
                        CheckSwap(pos1, pos2);

                        if (_matchData.Count == 0)
                        {
                            // 스왑을 되돌리는 로직 (매치 실패 시)
                            yield return StartCoroutine(SwapCor(pos1, pos2, true));
                            break;
                        }
                    }
                    first = false;
                    yield return StartCoroutine(AllCheck());
                }


                yield return StartCoroutine(DamageCheck());

                yield return new WaitForSeconds(0.5f);

                yield return StartCoroutine(AllCheckEmpty());

                yield return StartCoroutine(AllCheck());

                // 새로운 매치가 없으면 루프 종료
                if (_matchedPositions.Count == 0 && _emptyPositions.Count == 0)
                {
                    Debug.Log("매치된거 하나도 없다 마!");
                    _isSpecialEffectActive = false;
                    break;
                }
            }


            _turnCor = null;
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
                _emptyPositions = _emptyPositions.Distinct().ToList();
                if (_emptyPositions.Count == 0)
                {
                    yield break;
                }
                else if (_emptyPositions.Count > 0) yield return StartCoroutine(FullEmpty());

            }

        }

        private IEnumerator FullEmpty()
        {
            _emptyPositions = _emptyPositions.OrderBy(pos => pos.x).ThenBy(pos => pos.y).ToList();
            foreach (Vector3Int pos in _emptyPositions)
            {
                IsEmpty(pos + Vector3Int.up);
                yield return new WaitForFixedUpdate();
            }
            _addSpecialPos.Clear();
            _emptyPositions.Clear();
        }

        public static IEnumerator IsSpeical(Vector3Int pos)
        {
            if (instance.CellContent[pos].GetSpecial() != null)
            {
                instance.CellContent[pos].Damage();
            }
            yield return new WaitForSeconds(0.2f);
        }

        // 두 컨텐츠의 위치를 바꿈
        public static IEnumerator SwapCor(Vector3Int pos, Vector3Int moveToPos, bool back = false)
        {
            if (!instance.CellContent[moveToPos].CanMove() || instance.CellContent[moveToPos].IsEmpty()) yield break;
            Donut changeDonut = instance.CellContent[moveToPos].GetDonut();
            Obstacle changeObstacle = instance.CellContent[moveToPos].GetObstacle();
            Special changeSpecial = instance.CellContent[moveToPos].GetSpecial();
            instance.CellContent[pos].MoveObject(instance.CellContent[moveToPos]);
            instance.CellContent[pos].SetDonut(changeDonut);
            instance.CellContent[pos].SetObstacle(changeObstacle);
            instance.CellContent[pos].SetSpecial(changeSpecial);
            instance.StartCoroutine(instance.CellContent[pos].SetPos(0.2f));
            instance.StartCoroutine(instance.CellContent[moveToPos].SetPos(0.2f));
            yield return new WaitForSeconds(0.2f);

        }

        private void CheckSwap(Vector3Int firstPos, Vector3Int secondPos)
        {
            _matchedPositions.Clear();
            _splashDamageTargets.Clear();
            _addSpecialPos.Clear();

            if (instance.CellContent[firstPos].GetSpecial() != null &&
            instance.CellContent[secondPos].GetSpecial() != null)
            {
                instance.CellContent[secondPos].UseTwoSpecial(instance.CellContent[firstPos].GetSpecial());
                instance.CellContent[firstPos].UseTwoSpecial();
                return;
            }
            else if (instance.CellContent[firstPos].GetSpecial() != null ||
             instance.CellContent[secondPos].GetSpecial() != null)
            {
                Vector3Int specialPos = instance.CellContent[firstPos].GetSpecial() != null ? firstPos : secondPos;
                Vector3Int normalPos = instance.CellContent[firstPos].GetSpecial() == null ? firstPos : secondPos;
                _matchedPositions.Add(specialPos);
                CheckMatch(normalPos);
            }
            else
            {
                CheckMatch(firstPos);
                CheckMatch(secondPos);
            }

            _matchedPositions = _matchedPositions.Distinct().ToList();
            _splashDamageTargets = _splashDamageTargets
                    .Distinct().Except(_matchedPositions.Distinct()).ToList();
        }

        private IEnumerator AllCheck(bool isStart = false)
        {
            foreach (Vector3Int pos in CellList)
                CheckMatch(pos, isStart);

            _matchedPositions.Clear();
            _splashDamageTargets.Clear();

            _matchData = _matchData.Distinct(new MatchDataComparer()).ToList();

            foreach (var match in _matchData)
            {
                if (match.MatchType != GemType.Empty)
                {
                    MatchSpecial(match.MatchPos[Random.Range(0, match.MatchPos.Count)], match.MatchType);
                }

                _matchedPositions.AddRange(match.MatchPos);
                _splashDamageTargets.AddRange(match.SplashPos);
            }

            _matchedPositions = _matchedPositions.Distinct().ToList();
            _splashDamageTargets = _splashDamageTargets.Distinct().ToList();
            _matchData.Clear();
            yield break;
        }

        private void CheckMatch(Vector3Int pos, bool isStart = false)
        {
            // 초기화
            _matchedPositions.Clear();
            _splashDamageTargets.Clear();


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

            bool squareMatch = CheckSquare(pos);

            // 매치된 블록이 있을 경우
            if (_matchedPositions.Count > 0)
            {
                MatchData matchData = new();

                // 중복 제거
                _matchedPositions = _matchedPositions.Distinct().OrderBy(v => v.y).ThenBy(v => v.x).ToList();
                matchData.MatchPos = new List<Vector3Int>(_matchedPositions);
                foreach (var matchedPos in _matchedPositions)
                {
                    AddSplashTargets(matchedPos);
                }

                // 스플래시 데미지 대상 리스트에서 매치 블록과 중복을 제거
                if (_splashDamageTargets.Count > 0)
                    _splashDamageTargets = _splashDamageTargets
                        .Distinct().Except(_matchedPositions).OrderBy(v => v.y).ThenBy(v => v.x).ToList();

                matchData.SplashPos = new List<Vector3Int>(_splashDamageTargets);
                if (!isStart)
                {
                    if (!_isSpecialEffectActive && !_addSpecialPos.ContainsKey(pos))
                    {
                        if (horizontalCount >= 5 || verticalCount >= 5)
                        {
                            matchData.MatchType = GemType.Oven;
                        }
                        else
                        {
                            if (horizontalCount >= 3 && verticalCount >= 3)
                            {
                                matchData.MatchType = GemType.DonutBox;
                            }
                            else if (horizontalCount >= 4)
                            {
                                matchData.MatchType = GemType.Roller_v;
                            }
                            else if (verticalCount >= 4)
                            {
                                matchData.MatchType = GemType.Roller_h;
                            }
                            else if (squareMatch)
                            {
                                matchData.MatchType = GemType.Milk;
                            }
                            else
                                matchData.MatchType = GemType.Empty;
                        }
                    }

                }
                _matchData.Add(matchData);
            }
        }



        public static IEnumerator DownCor(Vector3Int pos, Vector3Int moveToPos)
        {
            instance.CellContent[pos].MoveObject(instance.CellContent[moveToPos]);
            instance.CellContent[pos].SetDonut(null);
            instance.CellContent[pos].SetObstacle(null);
            instance.CellContent[pos].SetSpecial(null);
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



        public void IsEmpty(Vector3Int pos)
        {
            Vector3Int upPos = pos;
            Vector3Int downPos = pos;
            if (instance._addSpecialPos.ContainsKey(downPos + Vector3Int.down))
            {
                AddObject(downPos + Vector3Int.down,
                instance._addSpecialPos[downPos + Vector3Int.down]);
                instance._addSpecialPos.Remove(downPos + Vector3Int.down);
                return;
            }

            if (instance.SpawnPoint.Contains(pos))
            {
                AddObject(downPos + Vector3Int.down, SetSpawn());
                return;
            }


            // 수직 낙하
            Vector3Int maxYPos = instance.CellList.Where(v => v.x == pos.x).OrderBy(v => v.y).Reverse().FirstOrDefault();
            while (upPos != maxYPos)
            {
                if (!instance.CellList.Contains(upPos))
                {
                    upPos += Vector3Int.up;
                }
                else
                {
                    break;
                }
            }

            Vector3Int minYPos = instance.CellList.Where(v => v.x == pos.x).OrderBy(v => v.y).FirstOrDefault();
            while (downPos != minYPos)
            {
                downPos += Vector3Int.down;
                if (instance.CellContent.ContainsKey(downPos))
                {
                    if (instance.CellContent[upPos].CanMove())
                        if (instance.CellContent[downPos].IsEmpty())
                        {
                            StartCoroutine(DownCor(upPos, downPos));
                            return;
                        }
                }
            }


            // 수직 낙하가 끝났지만 왼쪽 아래가 비어있으면 미끄러짐
            downPos = pos + Vector3Int.left + Vector3Int.down;
            if (instance.CellContent.ContainsKey(downPos))
            {
                if (instance.CellContent[pos].CanMove())
                    if (instance.CellContent[downPos].IsEmpty())
                    {
                        StartCoroutine(DownCor(pos, downPos));
                        return;
                    }
                    else
                    {
                        return;
                    }
            }

            // 수직 낙하가 끝났지만 오른쪽 아래가 비어있으면 미끄러짐
            downPos = pos + Vector3Int.right + Vector3Int.right;
            if (instance.CellContent.ContainsKey(downPos))
            {
                if (instance.CellContent[downPos].CanMove())
                    if (instance.CellContent[downPos].IsEmpty())
                    {
                        StartCoroutine(DownCor(pos, downPos));
                        return;
                    }
                    else
                    {
                        return;
                    }
            }
        }




        private void MatchSpecial(Vector3Int pos, GemType gemType)
        {
            if (!_addSpecialPos.ContainsKey(pos))
                _addSpecialPos.Add(pos, gemType);
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



        public static void UseSpeical(Vector3Int pos, GemType firstGem, GemType secondGem = GemType.Empty)
        {
            instance._isSpecialEffectActive = true;
            if (firstGem == GemType.Roller_v)
            {
                instance.CheckVertical(pos);
            }
            else if (firstGem == GemType.Roller_h)
            {
                instance.CheckHorizontal(pos);
            }
            else if (firstGem == GemType.Milk)
            {
                instance.CheckMilk();
            }
            else if (firstGem == GemType.Oven)
            {

            }
            else if (firstGem == GemType.DonutBox)
            {

            }
            instance.DamageCheck();
        }

        private IEnumerator DamageCheck()
        {
            var positionsToDamage = new List<Vector3Int>(_matchedPositions);
            foreach (var matchedPos in positionsToDamage)
            {
                if (instance.CellContent.ContainsKey(matchedPos))
                    instance.CellContent[matchedPos].Damage();
            }
            _matchedPositions.Clear();

            var splashTargets = new List<Vector3Int>(_splashDamageTargets);
            foreach (var splashPos in splashTargets)
            {
                if (instance.CellContent.ContainsKey(splashPos))
                    instance.CellContent[splashPos].SplashDamage();
            }
            _splashDamageTargets.Clear();

            var milkTargets = new List<Vector3Int>(_milkTargets);
            foreach (var milkPos in milkTargets)
            {
                if (instance.CellContent.ContainsKey(milkPos))
                    instance.CellContent[milkPos].Damage();
            }
            _milkTargets.Clear();

            yield break;
        }

        private void CheckHorizontal(Vector3Int pos)
        {
            Vector3Int hPos;
            for (int i = -8; i < 8; i++)
            {
                hPos = new Vector3Int(i, 0, 0);
                if (instance.CellContent.ContainsKey(pos + hPos))
                    _matchedPositions.Add(pos + hPos);
            }
        }

        private void CheckVertical(Vector3Int pos)
        {
            Vector3Int vPos;
            for (int i = -8; i < 8; i++)
            {
                vPos = new Vector3Int(0, i, 0);
                if (instance.CellContent.ContainsKey(pos + vPos))
                    _matchedPositions.Add(pos + vPos);
            }
        }

        private void CheckMilk()
        {
            var targetingGem = instance.CellContent.Where(x => x.Value.getCellType() == /*조건*/GemType.Lavender)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            if (targetingGem.Count() > 2)
            {
                var randomItems = targetingGem.Take(3).ToDictionary(item => item.Key, item => item.Value);
                foreach (var data in randomItems)
                    _milkTargets.Add(data.Key);
            }
            else
            {
                int n = 3 - targetingGem.Count();
                foreach (var data in targetingGem)
                    _milkTargets.Add(data.Key);
                var randomItems = instance.CellContent.Take(n).ToDictionary(item => item.Key, item => item.Value);
                foreach (var data in randomItems)
                    _milkTargets.Add(data.Key);
            }
        }







        private Vector3Int CatStatuesPos(Vector3Int cat_sPos)
        {
            List<Vector3Int> catPos = new()
            {
                Vector3Int.down,
                Vector3Int.left,
                Vector3Int.down + Vector3Int.left
            };
            foreach (Vector3Int pos in catPos)
            {
                if (instance.CellContent.ContainsKey(cat_sPos + pos))
                {
                    if (instance.CellContent[cat_sPos].GetCatStatuse() == GemType.CatStatues)
                    {
                        return pos;
                    }
                }
            }
            return default;
        }

        public static void CatStatuesDistroy(Vector3Int CatStatuesPos)
        {
            List<Vector3Int> catPos = new()
            {
                CatStatuesPos + Vector3Int.up,
                CatStatuesPos + Vector3Int.right,
                CatStatuesPos + Vector3Int.up + Vector3Int.right
            };
            foreach (Vector3Int pos in catPos)
            {
                if (instance.CellContent.ContainsKey(pos))
                {
                    if (instance.CellContent[pos].GetCatStatuse() == GemType.CatStatues_s)
                    {
                        instance.CellContent[pos].DestroyCat();

                    }
                }
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

        private bool CheckSquare(Vector3Int startPos)
        {
            bool isSquare = false;
            bool upPos = instance.CellContent.ContainsKey(startPos + Vector3Int.up) &&
            instance.CellContent[startPos + Vector3Int.up].getCellType() == instance.CellContent[startPos].getCellType();
            bool downPos = instance.CellContent.ContainsKey(startPos + Vector3Int.down) &&
            instance.CellContent[startPos + Vector3Int.down].getCellType() == instance.CellContent[startPos].getCellType();
            bool leftPos = instance.CellContent.ContainsKey(startPos + Vector3Int.left) &&
            instance.CellContent[startPos + Vector3Int.left].getCellType() == instance.CellContent[startPos].getCellType();
            bool rightPos = instance.CellContent.ContainsKey(startPos + Vector3Int.right) &&
            instance.CellContent[startPos + Vector3Int.right].getCellType() == instance.CellContent[startPos].getCellType();

            if (upPos)
            {
                if (leftPos)
                    if (instance.CellContent.ContainsKey(startPos + Vector3Int.up + Vector3Int.left) &&
                instance.CellContent[startPos + Vector3Int.up + Vector3Int.left].getCellType() == instance.CellContent[startPos].getCellType())
                    {
                        isSquare = true;
                        _matchedPositions.Add(startPos);
                        _matchedPositions.Add(startPos + Vector3Int.up);
                        _matchedPositions.Add(startPos + Vector3Int.left);
                        _matchedPositions.Add(startPos + Vector3Int.up + Vector3Int.left);
                    }
                if (rightPos)
                    if (instance.CellContent.ContainsKey(startPos + Vector3Int.up + Vector3Int.right) &&
                    instance.CellContent[startPos + Vector3Int.up + Vector3Int.right].getCellType() == instance.CellContent[startPos].getCellType())
                    {
                        isSquare = true;
                        _matchedPositions.Add(startPos);
                        _matchedPositions.Add(startPos + Vector3Int.up);
                        _matchedPositions.Add(startPos + Vector3Int.right);
                        _matchedPositions.Add(startPos + Vector3Int.up + Vector3Int.right);
                    }
            }

            if (downPos)
            {
                if (leftPos)
                    if (instance.CellContent.ContainsKey(startPos + Vector3Int.down + Vector3Int.left) &&
                instance.CellContent[startPos + Vector3Int.down + Vector3Int.left].getCellType() == instance.CellContent[startPos].getCellType())
                    {
                        isSquare = true;
                        _matchedPositions.Add(startPos);
                        _matchedPositions.Add(startPos + Vector3Int.down);
                        _matchedPositions.Add(startPos + Vector3Int.left);
                        _matchedPositions.Add(startPos + Vector3Int.down + Vector3Int.left);
                    }
                if (rightPos)
                    if (instance.CellContent.ContainsKey(startPos + Vector3Int.down + Vector3Int.right) &&
                    instance.CellContent[startPos + Vector3Int.down + Vector3Int.right].getCellType() == instance.CellContent[startPos].getCellType())
                    {
                        isSquare = true;
                        _matchedPositions.Add(startPos);
                        _matchedPositions.Add(startPos + Vector3Int.down);
                        _matchedPositions.Add(startPos + Vector3Int.right);
                        _matchedPositions.Add(startPos + Vector3Int.down + Vector3Int.right);
                    }
            }

            return isSquare;
        }



        // 매치된 블록의 주변 블록을 splashDamageTargets 리스트에 추가하는 함수
        private void AddSplashTargets(Vector3Int centerPos)
        {
            Vector3Int[] directions = new Vector3Int[]
            {
                Vector3Int.up, Vector3Int.down,
                Vector3Int.left, Vector3Int.right
            };

            foreach (var direction in directions)
            {
                Vector3Int targetPos = centerPos + direction;
                if (instance.CellContent.ContainsKey(targetPos))
                {
                    if (instance.CellContent[targetPos].GetCatStatuse() == GemType.CatStatues_s)
                    {
                        if (!instance.CellContent.ContainsKey(CatStatuesPos(targetPos)))
                        {
                            _splashDamageTargets.Add(CatStatuesPos(targetPos));
                        }

                    }
                    else
                    {
                        _splashDamageTargets.Add(targetPos);
                    }

                }
            }
        }




        private void GetReference()
        {
            _grid = GetComponent<Grid>();
        }
    }
}
