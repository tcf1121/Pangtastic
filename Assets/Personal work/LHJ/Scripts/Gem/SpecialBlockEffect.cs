using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace LHJ
{
    public class SpecialBlockEffect : MonoBehaviour
    {
        private System.Random _rand = new System.Random();
        [SerializeField] private GameObject _rollerHorizontalFx;
        [SerializeField] private GameObject _rollerVerticalFx;
        public static bool effectRunning { get { return _running > 0; } }

        private static int _running;
        private bool IsSpecial(GemType t)
        {
            return t == GemType.Roller_h
                || t == GemType.Roller_v
                || t == GemType.DonutBox
                || t == GemType.Milk
                || t == GemType.Oven;
        }

        private static void BeginEffect()
        {
            _running++;  
        }

        private static void EndEffect()
        {
            if (_running > 0) _running--;
        }
        public void UseSpecial(Vector2Int pos, GemType specialType, GameBoardData gameBoard, List<Vector2Int> outDamage)
        {
            if (gameBoard == null || outDamage == null) return;

            // 우유
            if (specialType == GemType.Milk)
            {
                AddCell(gameBoard, pos.x, pos.y, outDamage);
                // var targetGems = InGameManager.GetTagetGem();
                // var candidates = GetTargetPos(gameBoard, targetGems);
                List<Vector2Int> candidates = new List<Vector2Int>();
                Shuffle(candidates);

                // 최대 3개 선택, 부족하면 랜덤 보충
                List<Vector2Int> picks = new List<Vector2Int>(3);
                int take = Mathf.Min(3, candidates.Count);
                for (int i = 0; i < take; i++) picks.Add(candidates[i]);

                while (picks.Count < 3)
                {
                    int x = Random.Range(0, gameBoard.Width);
                    int y = Random.Range(0, gameBoard.Height);
                    Vector2Int v = new Vector2Int(x, y);
                    if (!ContainsVec(picks, v)) picks.Add(v);
                }

                for (int i = 0; i < picks.Count; i++)
                    outDamage.Add(picks[i]);
                return;
            }

            // 세로 밀대
            if (specialType == GemType.Roller_v)
            {
                var bm = BoardManager.Instance;
                if (bm != null)
                    bm.StartCoroutine(RollerRoutine(pos, false, gameBoard, bm));
                return;
            }

            // 가로 밀대
            if (specialType == GemType.Roller_h)
            {
                var bm = BoardManager.Instance;
                if (bm != null)
                    bm.StartCoroutine(RollerRoutine(pos, true, gameBoard, bm));
                return;

            }

            // 도넛 박스 (5x5)
            if (specialType == GemType.DonutBox)
            {
                for (int x = pos.x - 2; x <= pos.x + 2; x++)
                    for (int y = pos.y - 2; y <= pos.y + 2; y++)
                        if (x >= 0 && x < gameBoard.Width && y >= 0 && y < gameBoard.Height)
                            AddCell(gameBoard, x, y, outDamage);
                return;
            }

            // 오븐
            if (specialType == GemType.Oven)
            {
                AddCell(gameBoard, pos.x, pos.y, outDamage);
                Vector2Int? other = null;
                // 1) BoardManager의 스왑 좌표 사용
                var bm = BoardManager.Instance; // 싱글턴 사용
                if (bm != null && bm.BlockMover != null)
                {
                    Vector2Int start = bm.BlockMover.StartBlockPos;
                    Vector2Int end = bm.BlockMover.EndBlockPos;

                    if (pos == start) other = end;
                    else if (pos == end) other = start;
                }

                if (other == null)
                {
                    Vector2Int[] dirs = new Vector2Int[]
                    {
                        new Vector2Int(-1, 0),
                        new Vector2Int( 1, 0),
                        new Vector2Int( 0, 1),
                        new Vector2Int( 0,-1),
                    };

                    for (int i = 0; i < dirs.Length; i++)
                    {
                        int nx = pos.x + dirs[i].x;
                        int ny = pos.y + dirs[i].y;
                        if (nx < 0 || nx >= gameBoard.Width || ny < 0 || ny >= gameBoard.Height)
                            continue;

                        var nb = gameBoard.GetBlock(nx, ny);
                        if (nb != null && nb.BlockInstance != null && nb.GemType < GemType.Milk)
                        {
                            other = new Vector2Int(nx, ny);
                            break;
                        }
                    }
                }

                // 스왑 상대 타입 전체 누적
                if (other != null)
                {
                    var ob = gameBoard.GetBlock(other.Value.x, other.Value.y);
                    if (ob != null && ob.BlockInstance != null && ob.GemType < GemType.Milk)
                    {
                        var targetType = ob.GemType;

                        for (int y = 0; y < gameBoard.Height; y++)
                        {
                            for (int x = 0; x < gameBoard.Width; x++)
                            {
                                var b = gameBoard.GetBlock(x, y);
                                if (b != null && b.BlockInstance != null && b.GemType == targetType)
                                {
                                    AddCell(gameBoard, x, y, outDamage);
                                }
                            }
                        }
                    }
                }
                return;
            }
        }
        public IEnumerator RollerRoutine(Vector2Int pos, bool isHorizontal, GameBoardData gameBoard, BoardManager board)
        {
            if (gameBoard == null || board == null) yield break;
            BeginEffect();

            GameObject fxPrefab = isHorizontal ? _rollerHorizontalFx : _rollerVerticalFx;
            GameObject fx = Instantiate(fxPrefab, board.transform);

            Vector3 startWorld, endWorld;
            if (isHorizontal)
            {
                startWorld = GridToWorld(board, gameBoard, 0, pos.y);
                endWorld = GridToWorld(board, gameBoard, gameBoard.Width - 1, pos.y);
            }
            else
            {
                startWorld = GridToWorld(board, gameBoard, pos.x, gameBoard.Height - 1);
                endWorld = GridToWorld(board, gameBoard, pos.x, 0);
            }
            fx.transform.position = startWorld;
            if (fx.TryGetComponent<SpriteRenderer>(out var sr)) sr.sortingOrder = 9999;

            float stepTime = 0.15f;
            float duration = (isHorizontal ? gameBoard.Width : gameBoard.Height) * stepTime;
            fx.transform.DOMove(endWorld, duration).SetEase(Ease.Linear);

            // 시작칸 먼저 제거
            {
                var origin = new List<Vector2Int>();
                AddCell(gameBoard, pos.x, pos.y, origin);
                if (origin.Count > 0) ApplyDamageAndScore(board, origin);
            }

            // 라인 진행 중 만난 특수블록 예약
            List<(Vector2Int p, GemType t)> queuedSpecials = new List<(Vector2Int, GemType)>();

            if (isHorizontal)
            {
                for (int x = 0; x < gameBoard.Width; x++)
                {
                    bool isOriginCell = (x == pos.x);  
                    var cur = gameBoard.GetBlock(x, pos.y);

                    if (!isOriginCell && cur != null && cur.BlockInstance != null && IsSpecial(cur.GemType))
                    {
                        bool exists = false;
                        for (int i = 0; i < queuedSpecials.Count; i++)
                            if (queuedSpecials[i].p.x == x && queuedSpecials[i].p.y == pos.y) { exists = true; break; }
                        if (!exists) queuedSpecials.Add((new Vector2Int(x, pos.y), cur.GemType));
                    }

                    var hits = new List<Vector2Int>();
                    AddCell(gameBoard, x, pos.y, hits);
                    if (hits.Count > 0) ApplyDamageAndScore(board, hits);

                    yield return new WaitForSeconds(stepTime);
                }
            }
            else
            {
                for (int y = gameBoard.Height - 1; y >= 0; y--)
                {
                    bool isOriginCell = (y == pos.y); 
                    var cur = gameBoard.GetBlock(pos.x, y);

                    if (!isOriginCell && cur != null && cur.BlockInstance != null && IsSpecial(cur.GemType))
                    {
                        bool exists = false;
                        for (int i = 0; i < queuedSpecials.Count; i++)
                            if (queuedSpecials[i].p.x == pos.x && queuedSpecials[i].p.y == y) { exists = true; break; }
                        if (!exists) queuedSpecials.Add((new Vector2Int(pos.x, y), cur.GemType));
                    }

                    var hits = new List<Vector2Int>();
                    AddCell(gameBoard, pos.x, y, hits);
                    if (hits.Count > 0) ApplyDamageAndScore(board, hits);

                    yield return new WaitForSeconds(stepTime);
                }
            }

            for (int i = 0; i < queuedSpecials.Count; i++)
            {
                var (p, t) = queuedSpecials[i];

                if (t == GemType.Roller_h)
                    board.StartCoroutine(RollerRoutine(p, true, gameBoard, board));
                else if (t == GemType.Roller_v)
                    board.StartCoroutine(RollerRoutine(p, false, gameBoard, board));
                else
                {
                    var extraHits = new List<Vector2Int>();
                    UseSpecial(p, t, gameBoard, extraHits);
                    if (extraHits.Count > 0) ApplyDamageAndScore(board, extraHits);
                }
            }

            Destroy(fx);
            EndEffect();
        }
        // 요구재료 좌표 목록 
        private List<Vector2Int> GetTargetPos(GameBoardData gameBoard, List<GemType> gemTypes)
        {
            var list = new List<Vector2Int>();
            for (int y = 0; y < gameBoard.Height; y++)
            {
                for (int x = 0; x < gameBoard.Width; x++)
                {
                    var blk = gameBoard.GetBlock(x, y);
                    if (blk != null && gemTypes.Contains(blk.GemType))
                        list.Add(new Vector2Int(x, y));
                }
            }
            return list;
        }

        // 점수 및 콤보 시간
        public int ApplyDamageAndScore(BoardManager board, List<Vector2Int> hits)
        {
            if (board == null || hits == null) return 0;

            var gameBoard = board.Spawner.GameBoardData;

            // 중복 좌표 제거
            List<Vector2Int> unique = new List<Vector2Int>();
            for (int i = 0; i < hits.Count; i++)
            {
                var v = hits[i];
                bool exists = false;
                for (int j = 0; j < unique.Count; j++)
                {
                    if (unique[j].x == v.x && unique[j].y == v.y)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists) unique.Add(v);
            }
            int destroyedCount = 0;

            // 실제 파괴
            for (int i = 0; i < unique.Count; i++)
            {
                var c = unique[i];
                var b = gameBoard.GetBlock(c.x, c.y);
                if (b != null && b.BlockInstance != null)
                {
                    //if (b.GemType < GemType.Milk)
                    //    InGameManager.AddIngredientSta(b.GemType);

                    if (b is ObstacleBlock ob)
                    {
                        ob.TakeDamage();
                        continue;
                    }

                    if (b.BlockInstance.TryGetComponent<PooledObject>(out var pooledObject))
                    {
                        pooledObject.ReturnToPool();
                    }
                    else
                    {
                        Destroy(b.BlockInstance);
                    }
                    
                    gameBoard.SetBlock(c.x, c.y, null);
                    destroyedCount++;
                }
            }

            // 점수: 부숴진 개수 × 10
            if (destroyedCount > 0)
            {
                int score = destroyedCount * 10;
                board.UpdateUI(score);
            }
            if (board.MatchCombo != null)
                board.MatchCombo.ResetTimer();

            return destroyedCount;
        }
        public void UseCombo(Vector2Int firstPos, Vector2Int secondPos, GemType firstType, GemType secondType, GameBoardData gameBoard, List<Vector2Int> outDamage)
        {
            if (gameBoard == null || outDamage == null) return;

            // 스왑된 특수블록 자신도 소모
            AddCell(gameBoard, firstPos.x, firstPos.y, outDamage);
            AddCell(gameBoard, secondPos.x, secondPos.y, outDamage);

            GemType a = firstType;
            GemType b = secondType;


            // 1) 밀대 + 밀대 
            if ((a == GemType.Roller_h && b == GemType.Roller_h) ||
                (a == GemType.Roller_v && b == GemType.Roller_v) ||
                (a == GemType.Roller_h && b == GemType.Roller_v) ||
                (a == GemType.Roller_v && b == GemType.Roller_h))
            {
                UseSpecial(secondPos, GemType.Roller_h, gameBoard, outDamage);
                UseSpecial(secondPos, GemType.Roller_v, gameBoard, outDamage);
                return;
            }

            // 2) 우유 + 우유
            if (a == GemType.Milk && b == GemType.Milk)
            {
                // var targets = InGameManager.GetTagetGem();
                // var candidates = GetTargetPos(gameBoard, targets);
                List<Vector2Int> candidates = new List<Vector2Int>();
                Shuffle(candidates);

                List<Vector2Int> picks = new List<Vector2Int>(5);
                int take = Mathf.Min(5, candidates.Count);
                for (int i = 0; i < take; i++) picks.Add(candidates[i]);

                while (picks.Count < 5)
                {
                    int rx = Random.Range(0, gameBoard.Width);
                    int ry = Random.Range(0, gameBoard.Height);
                    Vector2Int v = new Vector2Int(rx, ry);
                    bool exists = false;
                    for (int j = 0; j < picks.Count; j++) if (picks[j] == v) { exists = true; break; }
                    if (!exists) picks.Add(v);
                }

                for (int i = 0; i < picks.Count; i++) outDamage.Add(picks[i]);
                return;
            }

            // 3) 도넛 + 도넛
            if (a == GemType.DonutBox && b == GemType.DonutBox)
            {
                for (int x = secondPos.x - 4; x <= secondPos.x + 4; x++)
                    for (int y = secondPos.y - 4; y <= secondPos.y + 4; y++)
                        AddCell(gameBoard, x, y, outDamage);
                return;
            }

            // 4) 오븐 + 오븐
            if (a == GemType.Oven && b == GemType.Oven)
            {
                for (int x = 0; x < gameBoard.Width; x++)
                    for (int y = 0; y < gameBoard.Height; y++)
                        AddCell(gameBoard, x, y, outDamage);
                return;
            }

            // 5) 밀대(세로) + 우유
            if ((a == GemType.Roller_v && b == GemType.Milk) ||
                (a == GemType.Milk && b == GemType.Roller_v))
            {
                // 우유 선택 로직
                // List<GemType> targets = InGameManager.GetTagetGem();
                // List<Vector2Int> candidates = GetTargetPos(gameBoard, targets);
                List<Vector2Int> candidates = new List<Vector2Int>();
                Shuffle(candidates);

                List<Vector2Int> picks = new List<Vector2Int>(3);
                int take = (candidates.Count < 3) ? candidates.Count : 3;
                for (int i = 0; i < take; i++) picks.Add(candidates[i]);

                while (picks.Count < 3)
                {
                    int rx = Random.Range(0, gameBoard.Width);
                    int ry = Random.Range(0, gameBoard.Height);
                    Vector2Int v = new Vector2Int(rx, ry);

                    bool exists = false;
                    for (int j = 0; j < picks.Count; j++)
                        if (picks[j].x == v.x && picks[j].y == v.y) { exists = true; break; }

                    if (!exists) picks.Add(v);
                }

                for (int i = 0; i < picks.Count; i++)
                    UseSpecial(picks[i], GemType.Roller_v, gameBoard, outDamage);

                return;
            }

            // 6) 밀대(가로) + 우유 
            if ((a == GemType.Roller_h && b == GemType.Milk) ||
                (a == GemType.Milk && b == GemType.Roller_h))
            {
                // 우유 선택 로직
                // List<GemType> targets = InGameManager.GetTagetGem();
                // List<Vector2Int> candidates = GetTargetPos(gameBoard, targets);
                List<Vector2Int> candidates = new List<Vector2Int>();
                Shuffle(candidates);

                List<Vector2Int> picks = new List<Vector2Int>(3);
                int take = (candidates.Count < 3) ? candidates.Count : 3;
                for (int i = 0; i < take; i++) picks.Add(candidates[i]);

                while (picks.Count < 3)
                {
                    int rx = Random.Range(0, gameBoard.Width);
                    int ry = Random.Range(0, gameBoard.Height);
                    Vector2Int v = new Vector2Int(rx, ry);

                    bool exists = false;
                    for (int j = 0; j < picks.Count; j++)
                        if (picks[j].x == v.x && picks[j].y == v.y) { exists = true; break; }

                    if (!exists) picks.Add(v);
                }

                for (int i = 0; i < picks.Count; i++)
                    UseSpecial(picks[i], GemType.Roller_h, gameBoard, outDamage);

                return;
            }

            // 7) 밀대(세로) + 도넛 
            if ((a == GemType.Roller_v && b == GemType.DonutBox) ||
                (a == GemType.DonutBox && b == GemType.Roller_v))
            {
                for (int dx = -1; dx <= 1; dx++)
                    UseSpecial(new Vector2Int(secondPos.x + dx, secondPos.y), GemType.Roller_v, gameBoard, outDamage);
                return;
            }

            // 8) 밀대(가로) + 도넛 
            if ((a == GemType.Roller_h && b == GemType.DonutBox) ||
                (a == GemType.DonutBox && b == GemType.Roller_h))
            {
                for (int dy = -1; dy <= 1; dy++) 
                    UseSpecial(new Vector2Int(secondPos.x, secondPos.y + dy), GemType.Roller_h, gameBoard, outDamage);
                return;
            }

            // 9) 우유 + 도넛
            if ((a == GemType.Milk && b == GemType.DonutBox) ||
                (a == GemType.DonutBox && b == GemType.Milk))
            {
                // 우유 선택 로직
                // List<GemType> targets = InGameManager.GetTagetGem();
                // List<Vector2Int> candidates = GetTargetPos(gameBoard, targets);
                List<Vector2Int> candidates = new List<Vector2Int>();
                Shuffle(candidates);

                List<Vector2Int> picks = new List<Vector2Int>(3);
                int take = (candidates.Count < 3) ? candidates.Count : 3;
                for (int i = 0; i < take; i++) picks.Add(candidates[i]);

                while (picks.Count < 3)
                {
                    int rx = Random.Range(0, gameBoard.Width);
                    int ry = Random.Range(0, gameBoard.Height);
                    Vector2Int v = new Vector2Int(rx, ry);

                    bool exists = false;
                    for (int j = 0; j < picks.Count; j++)
                        if (picks[j].x == v.x && picks[j].y == v.y) { exists = true; break; }

                    if (!exists) picks.Add(v);
                }

                for (int i = 0; i < picks.Count; i++)
                    UseSpecial(picks[i], GemType.DonutBox, gameBoard, outDamage);

                return;
            }

            // 10) 오븐 + 우유
            if ((a == GemType.Oven && b == GemType.Milk) ||
                (a == GemType.Milk && b == GemType.Oven))
            {
                // 1) 현재 필요한 재료 타입 목록
                // List<GemType> need = InGameManager.GetTagetGem();
                List<GemType> need = new List<GemType>();

                for (int y = 0; y < gameBoard.Height; y++)
                {
                    for (int x = 0; x < gameBoard.Width; x++)
                    {
                        var bl = gameBoard.GetBlock(x, y);
                        if (bl == null || bl.BlockInstance == null) continue;

                        // 필요 재료 타입인지 확인
                        bool isNeeded = false;
                        for (int k = 0; k < need.Count; k++)
                        {
                            if (bl.GemType == need[k]) { isNeeded = true; break; }
                        }
                        if (!isNeeded) continue;

                        // 해당 칸에서 '우유' 특수 효과 즉시 발동
                        UseSpecial(new Vector2Int(x, y), GemType.Milk, gameBoard, outDamage);
                    }
                }
                return;
            }

            // 11) 오븐 + 밀대(가로/세로) 
            if ((a == GemType.Oven && (b == GemType.Roller_h || b == GemType.Roller_v)) ||
               ((a == GemType.Roller_h || a == GemType.Roller_v) && b == GemType.Oven))
            {
                GemType lineType = (a == GemType.Oven) ? b : a; 

                List<GemType> normalTypes = new List<GemType>();
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    for (int x = 0; x < gameBoard.Width; x++)
                    {
                        var bl = gameBoard.GetBlock(x, y);
                        if (bl != null && bl.BlockInstance != null && bl.GemType < GemType.Milk)
                        {
                            bool exist = false;
                            for (int k = 0; k < normalTypes.Count; k++)
                                if (normalTypes[k] == bl.GemType) { exist = true; break; }
                            if (!exist) normalTypes.Add(bl.GemType);
                        }
                    }
                }
                if (normalTypes.Count == 0) return;

                // 그 중 랜덤 하나 선택
                int pickIdx = Random.Range(0, normalTypes.Count);
                GemType chosenType = normalTypes[pickIdx];

                // 선택된 타입의 모든 좌표에서 '스왑된 밀대' 효과 즉시 발동
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    for (int x = 0; x < gameBoard.Width; x++)
                    {
                        var bl = gameBoard.GetBlock(x, y);
                        if (bl != null && bl.BlockInstance != null && bl.GemType == chosenType)
                            UseSpecial(new Vector2Int(x, y), lineType, gameBoard, outDamage);
                    }
                }
                return;
            }

            // 12) 오븐 + 도넛 
            if ((a == GemType.Oven && b == GemType.DonutBox) ||
                (a == GemType.DonutBox && b == GemType.Oven))
            {
                // 보드에 존재하는 '서로 다른 노말 젬 타입' 목록 만들기
                List<GemType> normalTypes = new List<GemType>();
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    for (int x = 0; x < gameBoard.Width; x++)
                    {
                        var bl = gameBoard.GetBlock(x, y);
                        if (bl != null && bl.BlockInstance != null && bl.GemType < GemType.Milk)
                        {
                            bool exist = false;
                            for (int k = 0; k < normalTypes.Count; k++)
                                if (normalTypes[k] == bl.GemType) { exist = true; break; }
                            if (!exist) normalTypes.Add(bl.GemType);
                        }
                    }
                }
                if (normalTypes.Count == 0) return;

                // 그 중 랜덤 하나 선택
                int pickIdx = Random.Range(0, normalTypes.Count);
                GemType chosenType = normalTypes[pickIdx];

                // 선택된 타입의 모든 좌표에서 '도넛' 효과 즉시 발동
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    for (int x = 0; x < gameBoard.Width; x++)
                    {
                        var bl = gameBoard.GetBlock(x, y);
                        if (bl != null && bl.BlockInstance != null && bl.GemType == chosenType)
                            UseSpecial(new Vector2Int(x, y), GemType.DonutBox, gameBoard, outDamage);
                    }
                }
                return;
            }
            UseSpecial(firstPos, firstType, gameBoard, outDamage);
            UseSpecial(secondPos, secondType, gameBoard, outDamage);
        }

        private void Shuffle(List<Vector2Int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rand.Next(i + 1);
                Vector2Int tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        private bool ContainsVec(List<Vector2Int> list, Vector2Int v)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].x == v.x && list[i].y == v.y) return true;
            return false;
        }

        private void AddCell(GameBoardData gb, int x, int y, List<Vector2Int> outDamage, bool includeWaitingRow = false)
        {
            if (x < 0 || x >= gb.Width) return;
            int hLimit = includeWaitingRow ? gb.Height + 1 : gb.Height;
            if (y < 0 || y >= hLimit) return;

            if (y < gb.Height && !gb.BlockPlate.BlockPlateArray[y, x]) return;

            outDamage.Add(new Vector2Int(x, y));
        }
        private Vector3 GridToWorld(BoardManager board, GameBoardData gb, int x, int y)
        {
            return board.BlockMover.GridToWorld(new Vector2Int(x, y), gb.Width, gb.Height);
        }
    }
}
