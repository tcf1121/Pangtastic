using KDJ;
using LHJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    public class SpecialBlockCombo : MonoBehaviour
    {
        public static SpecialBlockCombo Instance { get; private set; }
        private int _lastResolvedFrame = -1;
        private Vector2Int _lastStart;
        private Vector2Int _lastEnd;

        // 우유/팝콘이 대상으로 삼는 재료
        private static readonly GemType[] _ingredientTypes =
        {
            GemType.Lavender, GemType.Chocolate, GemType.Blueberry, GemType.Cheese, GemType.Strawberry
        };

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }


        public bool TryResolveFromActivate(BoardManager board, GameObject selfGo)
        {
            if (board == null || board.Spawner == null || selfGo == null) return false;

            Vector2Int startPos = board.BlockMover.StartBlockPos;
            Vector2Int endPos = board.BlockMover.EndBlockPos;

            var aGo = GetGo(board, startPos);
            var bGo = GetGo(board, endPos);
            if (aGo == null || bGo == null) return false;

            // 스왑된 두 칸 중 하나의 Activate만 콤보 진입 허용
            if (aGo != selfGo && bGo != selfGo) return false;

            var sa = aGo.GetComponent<SpecialBlock>();
            var sb = bGo.GetComponent<SpecialBlock>();
            if (sa == null || sb == null) return false; // 둘 다 특수여야 콤보

            bool sameSwap =
                (_lastStart == startPos && _lastEnd == endPos) ||
                (_lastStart == endPos && _lastEnd == startPos);

            if (_lastResolvedFrame == Time.frameCount && sameSwap)
            {
                return true;
            }

            bool handled = ResolveRollerCombos(board, startPos, endPos);
            if (handled)
            {
                _lastResolvedFrame = Time.frameCount;
                _lastStart = startPos;
                _lastEnd = endPos;
            }
            return handled;
        }

        /// <summary>
        /// 1. 밀대+밀대: +자
        /// 2. 밀대+도넛: 밀대 방향 3줄
        /// 3. 밀대+우유: 우유가 고른 재료 3칸에서 밀대 방향 라인 즉시 발동
        /// 4. 밀대+팝콘: 재료 1종 랜덤 선택, 같은 타입 모든 칸에서 H/V 랜덤 라인 즉시 발동
        /// 5. 우유+우유: 필요 재료가 있으면 그 타입 랜덤 5개, 없으면 아무 재료 5개 제거
        /// 6. 우유+팝콘: 필요 재료 중 1종 전부 제거
        /// 7. 우유+도넛: 우유 위치 중심 5x5 제거
        /// </summary>
        public bool ResolveRollerCombos(BoardManager board, Vector2Int startPos, Vector2Int endPos)
        {
            if (board == null || board.Spawner == null) return false;

            var aGo = GetGo(board, startPos);
            var bGo = GetGo(board, endPos);
            if (aGo == null || bGo == null) return false;

            var a = aGo.GetComponent<SpecialBlock>();
            var b = bGo.GetComponent<SpecialBlock>();
            if (a == null && b == null) return false;

            // 1. 밀대 + 밀대
            if (IsRoller(a) && IsRoller(b))
            {
                var c = endPos;
                ClearRow(board, c.y, aGo, bGo, true);
                ClearCol(board, c.x, aGo, bGo, true);
                return true;
            }

            // 2. 밀대 + 도넛상자
            if ((IsRoller(a) && IsDonut(b)) || (IsDonut(a) && IsRoller(b)))
            {
                bool horizontal = IsRollerH(a) || IsRollerH(b);
                var c = endPos;

                if (horizontal)
                {
                    ClearRow(board, c.y - 1, aGo, bGo, true);
                    ClearRow(board, c.y, aGo, bGo, true);
                    ClearRow(board, c.y + 1, aGo, bGo, true);
                }
                else
                {
                    ClearCol(board, c.x - 1, aGo, bGo, true);
                    ClearCol(board, c.x, aGo, bGo, true);
                    ClearCol(board, c.x + 1, aGo, bGo, true);
                }
                return true;
            }

            // 3. 밀대 + 우유
            if ((IsRoller(a) && IsMilk(b)) || (IsMilk(a) && IsRoller(b)))
            {
                bool horizontal = IsRollerH(a) || IsRollerH(b);

                var spawner = board.Spawner;
                int w = spawner.BlockPlate.BlockPlateWidth;
                int h = spawner.BlockPlate.BlockPlateHeight;

                // 후보: 재료 계열 & 일반 블록(특수 제외)
                var candidates = new List<Vector2Int>();
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        var cell = spawner.BlockArray[y, x];
                        if (cell == null || cell.BlockInstance == null) continue;
                        if (cell.BlockInstance.GetComponent<SpecialBlock>() != null) continue;
                        if (!IsIngredient(cell.GemType)) continue;

                        candidates.Add(new Vector2Int(x, y));
                    }

                int toFire = Mathf.Min(3, candidates.Count);
                for (int i = 0; i < toFire; i++)
                {
                    int idx = Random.Range(0, candidates.Count);
                    var pos = candidates[idx];
                    candidates.RemoveAt(idx);

                    if (horizontal) ClearRow(board, pos.y, aGo, bGo, true);
                    else ClearCol(board, pos.x, aGo, bGo, true);
                }

                ConsumeSwapped(board, startPos, endPos);
                return true;
            }

            // 4. 밀대 + 팝콘
            if ((IsRoller(a) && IsPopcorn(b)) || (IsPopcorn(a) && IsRoller(b)))
            {
                var spawner = board.Spawner;
                int w = spawner.BlockPlate.BlockPlateWidth;
                int h = spawner.BlockPlate.BlockPlateHeight;

                var targetType = _ingredientTypes[Random.Range(0, _ingredientTypes.Length)];

                var targets = new List<Vector2Int>();
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        var cell = spawner.BlockArray[y, x];
                        if (cell == null || cell.BlockInstance == null) continue;
                        if (cell.BlockInstance.GetComponent<SpecialBlock>() != null) continue;
                        if (cell.GemType != targetType) continue;

                        targets.Add(new Vector2Int(x, y));
                    }

                foreach (var p in targets)
                {
                    bool horizontal = Random.value < 0.5f;
                    if (horizontal) ClearRow(board, p.y, aGo, bGo, true);
                    else ClearCol(board, p.x, aGo, bGo, true);
                }

                ConsumeSwapped(board, startPos, endPos);
                return true;
            }

            // 5. 우유 + 우유
            if (IsMilk(a) && IsMilk(b))
            {
                var spawner = board.Spawner;
                int w = spawner.BlockPlate.BlockPlateWidth;
                int h = spawner.BlockPlate.BlockPlateHeight;

                var needTargets = new List<Vector2Int>();
                var anyIngredient = new List<Vector2Int>();

                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        var cell = spawner.BlockArray[y, x];
                        if (cell == null || cell.BlockInstance == null) continue;

                        // 특수 제외, 재료만 취급
                        if (cell.BlockInstance.GetComponent<SpecialBlock>() != null) continue;
                        if (!IsIngredient(cell.GemType)) continue;

                        anyIngredient.Add(new Vector2Int(x, y));
                        needTargets.Add(new Vector2Int(x, y));
                    }

                var pool = (needTargets.Count > 0) ? needTargets : anyIngredient;
                int count = Mathf.Min(5, pool.Count);

                int destroyed = 0;
                for (int i = 0; i < count; i++)
                {
                    int idx = Random.Range(0, pool.Count);
                    var p = pool[idx];
                    pool.RemoveAt(idx);

                    var go = spawner.BlockArray[p.y, p.x].BlockInstance;
                    if (go == null) continue;
                    var sb2 = go.GetComponent<SpecialBlock>();
                    if (sb2 != null) sb2.Activate(board);

                    Object.Destroy(go);
                    spawner.BlockArray[p.y, p.x].BlockInstance = null;
                    destroyed++;
                }

                if (destroyed > 0) board.UpdateUI(destroyed * 10);
                ConsumeSwapped(board, startPos, endPos);
                return true;
            }

            // 6. 우유 + 팝콘
            if ((IsMilk(a) && IsPopcorn(b)) || (IsPopcorn(a) && IsMilk(b)))
            {
                var spawner = board.Spawner;
                int w = spawner.BlockPlate.BlockPlateWidth;
                int h = spawner.BlockPlate.BlockPlateHeight;

                // 보드에 존재하는 재료 타입들
                var presentTypes = new List<GemType>();
                for (int i = 0; i < _ingredientTypes.Length; i++)
                {
                    var t = _ingredientTypes[i];
                    bool exists = false;
                    for (int y = 0; y < h && !exists; y++)
                        for (int x = 0; x < w && !exists; x++)
                        {
                            var c = spawner.BlockArray[y, x];
                            if (c == null || c.BlockInstance == null) continue;
                            if (c.BlockInstance.GetComponent<SpecialBlock>() != null) continue;
                            if (c.GemType == t) exists = true;
                        }
                    if (exists) presentTypes.Add(t);
                }

                var targetType = (presentTypes.Count > 0)
                    ? presentTypes[Random.Range(0, presentTypes.Count)]
                    : _ingredientTypes[Random.Range(0, _ingredientTypes.Length)];

                int destroyed = 0;
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        var cell = spawner.BlockArray[y, x];
                        if (cell == null || cell.BlockInstance == null) continue;
                        if (cell.BlockInstance.GetComponent<SpecialBlock>() != null) continue;
                        if (cell.GemType != targetType) continue;

                        Object.Destroy(cell.BlockInstance);
                        spawner.BlockArray[y, x].BlockInstance = null;
                        destroyed++;
                    }

                if (destroyed > 0) board.UpdateUI(destroyed * 10);
                ConsumeSwapped(board, startPos, endPos);
                return true;
            }

            // 7. 우유 + 도넛상자 
            if ((IsMilk(a) && IsDonut(b)) || (IsDonut(a) && IsMilk(b)))
            {
                var milk = IsMilk(a) ? aGo : bGo;
                var center = WorldToGrid(board, milk.transform.position);

                ClearSquare(board, center, 2, aGo, bGo, true);
                ConsumeSwapped(board, startPos, endPos);
                return true;
            }

            return false;
        }

        private GameObject GetGo(BoardManager b, Vector2Int p)
        {
            var arr = b.Spawner.BlockArray;
            if (p.y < 0 || p.y >= arr.GetLength(0) || p.x < 0 || p.x >= arr.GetLength(1)) return null;
            return arr[p.y, p.x]?.BlockInstance;
        }

        private Vector2Int WorldToGrid(BoardManager b, Vector3 w)
        {
            var plate = b.Spawner.BlockPlate;
            int W = plate.BlockPlateWidth, H = plate.BlockPlateHeight;
            return new Vector2Int(
                Mathf.RoundToInt(w.x + W / 2f - 0.5f),
                Mathf.RoundToInt(w.y + H / 2f - 0.5f)
            );
        }

        private bool IsIngredient(GemType t)
        {
            for (int i = 0; i < _ingredientTypes.Length; i++)
                if (_ingredientTypes[i] == t) return true;
            return false;
        }

        private void ConsumeSwapped(BoardManager board, Vector2Int startPos, Vector2Int endPos)
        {
            var spawner = board.Spawner;

            var a = spawner.BlockArray[startPos.y, startPos.x];
            if (a != null && a.BlockInstance != null)
            {
                Object.Destroy(a.BlockInstance);
                spawner.BlockArray[startPos.y, startPos.x].BlockInstance = null;
            }

            var b = spawner.BlockArray[endPos.y, endPos.x];
            if (b != null && b.BlockInstance != null)
            {
                Object.Destroy(b.BlockInstance);
                spawner.BlockArray[endPos.y, endPos.x].BlockInstance = null;
            }
        }

        private bool IsRoller(SpecialBlock s) => s is Roller_H || s is Roller_V;
        private bool IsRollerH(SpecialBlock s) => s is Roller_H;
        private bool IsDonut(SpecialBlock s) => s is DonutBox;
        private bool IsMilk(SpecialBlock s) => s is Milk;
        private bool IsPopcorn(SpecialBlock s) => s is Popcorn;

        private void ClearRow(BoardManager b, int y, GameObject aGo, GameObject bGo, bool chain)
        {
            var sp = b.Spawner;
            int W = sp.BlockPlate.BlockPlateWidth, H = sp.BlockPlate.BlockPlateHeight;
            if (y < 0 || y >= H) return;

            int destroyed = 0;
            for (int x = 0; x < W; x++)
            {
                var blk = sp.BlockArray[y, x];
                if (blk == null || blk.BlockInstance == null) continue;

                var go = blk.BlockInstance;
                if (chain && go != aGo && go != bGo)
                {
                    var sb = go.GetComponent<SpecialBlock>();
                    if (sb != null) sb.Activate(b);
                }

                Object.Destroy(go);
                sp.BlockArray[y, x].BlockInstance = null;
                destroyed++;
            }
            if (destroyed > 0) b.UpdateUI(destroyed * 10);
        }

        private void ClearCol(BoardManager b, int x, GameObject aGo, GameObject bGo, bool chain)
        {
            var sp = b.Spawner;
            int W = sp.BlockPlate.BlockPlateWidth, H = sp.BlockPlate.BlockPlateHeight;
            if (x < 0 || x >= W) return;

            int destroyed = 0;
            for (int y = 0; y < H; y++)
            {
                var blk = sp.BlockArray[y, x];
                if (blk == null || blk.BlockInstance == null) continue;

                var go = blk.BlockInstance;
                if (chain && go != aGo && go != bGo)
                {
                    var sb = go.GetComponent<SpecialBlock>();
                    if (sb != null) sb.Activate(b);
                }

                Object.Destroy(go);
                sp.BlockArray[y, x].BlockInstance = null;
                destroyed++;
            }
            if (destroyed > 0) b.UpdateUI(destroyed * 10);
        }

        // 중심(center) 기준 (2r+1) x (2r+1) 정사각형 클리어
        private void ClearSquare(BoardManager b, Vector2Int center, int r, GameObject aGo, GameObject bGo, bool chain)
        {
            var sp = b.Spawner;
            int W = sp.BlockPlate.BlockPlateWidth;
            int H = sp.BlockPlate.BlockPlateHeight;

            int destroyed = 0;
            for (int y = center.y - r; y <= center.y + r; y++)
            {
                if (y < 0 || y >= H) continue;
                for (int x = center.x - r; x <= center.x + r; x++)
                {
                    if (x < 0 || x >= W) continue;

                    var blk = sp.BlockArray[y, x];
                    if (blk == null || blk.BlockInstance == null) continue;

                    var go = blk.BlockInstance;

                    if (chain && go != aGo && go != bGo)
                    {
                        var sb = go.GetComponent<SpecialBlock>();
                        if (sb != null) sb.Activate(b);
                    }

                    Object.Destroy(go);
                    sp.BlockArray[y, x].BlockInstance = null;
                    destroyed++;
                }
            }
            if (destroyed > 0) b.UpdateUI(destroyed * 10);
        }
    }
}
