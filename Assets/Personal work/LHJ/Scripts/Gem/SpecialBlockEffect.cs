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
        [Header("밀대")]
        [SerializeField] private GameObject _rollerHorizontalFx;
        [SerializeField] private GameObject _rollerVerticalFx;
        [SerializeField] private GameObject _rollerTrailFxH;
        [SerializeField] private GameObject _rollerTrailFxV;
        [SerializeField] private float _rollerEffectTime;

        [Header("우유")]
        [SerializeField] private GameObject _milkDropFx;
        [SerializeField] private GameObject _milkSplashFx;
        [SerializeField] private float _milkShrinkTime;
        [SerializeField] private float _milkTimePerCell;
        [SerializeField] private float _milkMinFlyTime;
        [SerializeField] private int _milkDropCount;
        [SerializeField] private int _milkDropCountDouble;

        [Header("오븐")]
        [SerializeField] private GameObject _ovenFx;
        [SerializeField] private Material _ovenStrokeMat;
        [SerializeField] private Material _ovenLineMat;
        [SerializeField] private float _ovenShakeTime;

        [Header("도넛상자")]
        [SerializeField] private GameObject _donutBurstFx;
        [SerializeField] private float _donutEffectTime;
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
        private static HashSet<Vector2Int> _activeEffects = new HashSet<Vector2Int>();

        private bool TryBeginEffectAt(Vector2Int pos)
        {
            if (_activeEffects.Contains(pos))
                return false;

            _activeEffects.Add(pos);
            return true;
        }

        private void EndEffectAt(Vector2Int pos)
        {
            _activeEffects.Remove(pos);
        }
        private void OnDisable()
        {
            _running = 0;
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
                var bm = BoardManager.Instance;
                if (bm != null)
                    bm.StartCoroutine(MilkRoutine(pos, gameBoard, bm, _milkDropCount, true));
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
                var bm = BoardManager.Instance;
                if (bm != null)
                    bm.StartCoroutine(DonutRoutine(pos, gameBoard, bm));
                return;
            }

            // 오븐
            if (specialType == GemType.Oven)
            {
                var bm = BoardManager.Instance;
                if (bm != null)
                    bm.StartCoroutine(OvenRoutine(pos, gameBoard, bm));
                return;
            }
        }

        public void StopAllSpecialEffects()
        {
            var b = BoardManager.Instance;
            b.StopAllCoroutines();
            DOTween.KillAll(true);
            foreach (Transform child in b.transform)
            {
                child.gameObject.SetActive(false);
            }
            Manager.Audio.StopSFX();
        }

        // 밀대(가로,세로)
        public IEnumerator RollerRoutine(Vector2Int pos, bool isHorizontal, GameBoardData gameBoard, BoardManager board)
        {
            if (gameBoard == null || board == null) yield break;
            BeginEffect();
            Manager.Audio.PlaySFX("Roller_Use");
            GameObject fxPrefab = isHorizontal ? _rollerHorizontalFx : _rollerVerticalFx;
            GameObject fx = Instantiate(fxPrefab, board.transform);

            // 잔상 생성 코루틴 시작: 방향 전달
            board.StartCoroutine(SpawnTrail(fx, board, isHorizontal));

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

            float stepTime = _rollerEffectTime;
            float duration = (isHorizontal ? gameBoard.Width : gameBoard.Height) * stepTime;
            fx.transform.DOMove(endWorld, duration).SetEase(Ease.Linear);

            // 시작칸 먼저 제거
            {
                var origin = new List<Vector2Int>();
                AddCell(gameBoard, pos.x, pos.y, origin);
                if (origin.Count > 0) ApplyDamageAndScore(board, origin);
            }

            if (isHorizontal)
            {
                for (int x = 0; x < gameBoard.Width; x++)
                {
                    bool isOriginCell = (x == pos.x);
                    var cur = gameBoard.GetBlock(x, pos.y);

                    if (!isOriginCell && cur != null && cur.BlockInstance != null && IsSpecial(cur.GemType))
                    {
                        UseSpecial(new Vector2Int(x, pos.y), cur.GemType, gameBoard, new List<Vector2Int>());
                        yield return new WaitForSeconds(stepTime);
                        continue;
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
                        UseSpecial(new Vector2Int(pos.x, y), cur.GemType, gameBoard, new List<Vector2Int>());
                        yield return new WaitForSeconds(stepTime);
                        continue;
                    }

                    var hits = new List<Vector2Int>();
                    AddCell(gameBoard, pos.x, y, hits);
                    if (hits.Count > 0) ApplyDamageAndScore(board, hits);

                    yield return new WaitForSeconds(stepTime);
                }
            }
            Destroy(fx);
            EndEffect();
        }
        private IEnumerator SpawnTrail(GameObject roller, BoardManager board, bool isHorizontal)
        {
            GameObject trailPrefab = isHorizontal ? _rollerTrailFxH : _rollerTrailFxV;
            if (trailPrefab == null) yield break;
            Vector3 moveDir = isHorizontal ? Vector3.right : Vector3.down;

            Vector3 c00 = GridToWorld(board, board.Spawner.GameBoardData, 0, 0);
            Vector3 c10 = GridToWorld(board, board.Spawner.GameBoardData, 1, 0);
            float cellSize = Mathf.Abs(c10.x - c00.x);

            float backOffset = cellSize * 0.3f;

            while (roller != null)
            {
                Vector3 spawnPos = roller.transform.position - moveDir * backOffset;

                var trail = Instantiate(trailPrefab, spawnPos, Quaternion.identity, board.transform);

                if (trail.TryGetComponent<SpriteRenderer>(out var sr))
                {
                    if (sr.sortingOrder < 9998) sr.sortingOrder = 9998;
                    var c = sr.color;
                    sr.color = new Color(c.r, c.g, c.b, Mathf.Min(c.a, 0.7f));
                }

                Destroy(trail, _rollerEffectTime);
                yield return null;
            }
        }

        private static HashSet<Vector2Int> _milkUsedTargets = new HashSet<Vector2Int>();

        private IEnumerator MilkRoutine(Vector2Int origin, GameBoardData gameBoard, BoardManager board,
            int count = 3, bool destroyOrigin = true, System.Action<List<Vector2Int>> onCompleted = null)
        {
            if (gameBoard == null || board == null) yield break;
            BeginEffect();
            Manager.Audio.PlaySFX("Milk_Use");
            List<Vector2Int> targets = GetMilkTargets(gameBoard, count, _milkUsedTargets);
            if (targets.Count == 0) { EndEffect(); yield break; }

            for (int i = 0; i < targets.Count; i++)
                _milkUsedTargets.Add(targets[i]);

            if (destroyOrigin)
            {
                StartCoroutine(ShrinkMilkOriginRoutine(origin, gameBoard, board));
            }

            Vector3 originWorld = GridToWorld(board, gameBoard, origin.x, origin.y);
            int finished = 0;

            for (int i = 0; i < targets.Count; i++)
            {
                Vector2Int tg = targets[i];
                Vector3 targetWorld = GridToWorld(board, gameBoard, tg.x, tg.y);

                GameObject drop = Instantiate(_milkDropFx, originWorld, Quaternion.identity, board.transform);
                if (drop.TryGetComponent<SpriteRenderer>(out var sr)) sr.sortingOrder = 9999;

                Vector3 c00 = GridToWorld(board, gameBoard, 0, 0);
                Vector3 c10 = GridToWorld(board, gameBoard, 1, 0);
                float cellSize = Mathf.Abs(c10.x - c00.x);
                float cellsDist = Mathf.Max(1f, Vector3.Distance(originWorld, targetWorld) / Mathf.Max(0.0001f, cellSize));
                float flyTime = Mathf.Max(_milkMinFlyTime, cellsDist * _milkTimePerCell);

                float baseScale = drop.transform.localScale.x;
                drop.transform
                    .DOScale(Vector3.one * baseScale * 1.3f, flyTime * 0.5f)
                    .SetEase(Ease.OutQuad)
                    .SetLoops(2, LoopType.Yoyo);

                drop.transform.DOMove(targetWorld, flyTime)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        if (_milkSplashFx != null)
                        {
                            var splash = Instantiate(_milkSplashFx, targetWorld, Quaternion.identity, board.transform);
                            Destroy(splash, 0.5f);
                            Manager.Audio.PlaySFX("Milk_Bubble");
                        }

                        var one = new List<Vector2Int>(1) { tg };
                        if (one.Count > 0) ApplyDamageAndScore(board, one);

                        Destroy(drop);
                        finished++;
                    });
            }

            float elapsed = 0f;
            float timeout = (_milkMinFlyTime + (Mathf.Max(gameBoard.Width, gameBoard.Height) * _milkTimePerCell)) + 0.25f;
            while (finished < targets.Count && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            onCompleted?.Invoke(targets);
            EndEffect();
        }
        private List<Vector2Int> GetMilkTargets(GameBoardData gb, int count, HashSet<Vector2Int> exclude = null)
        {
            List<GemType> needed = InGameManager.GetTagetGem();
            List<Vector2Int> result = new List<Vector2Int>();

            if (needed != null && needed.Count > 0)
            {
                List<Vector2Int> candidates = GetTargetPos(gb, needed);
                Shuffle(candidates);
                for (int i = 0; i < candidates.Count && result.Count < count; i++)
                {
                    if (exclude != null && exclude.Contains(candidates[i])) continue;
                    result.Add(candidates[i]);
                }
            }

            if (result.Count < count)
            {
                List<Vector2Int> normals = new List<Vector2Int>();
                for (int y = 0; y < gb.Height; y++)
                {
                    for (int x = 0; x < gb.Width; x++)
                    {
                        var b = gb.GetBlock(x, y);
                        if (b != null && b.BlockInstance != null && b.GemType < GemType.Milk)
                        {
                            Vector2Int v = new Vector2Int(x, y);
                            if (exclude != null && exclude.Contains(v)) continue;
                            normals.Add(v);
                        }
                    }
                }
                Shuffle(normals);
                for (int i = 0; i < normals.Count && result.Count < count; i++)
                {
                    result.Add(normals[i]);
                }
            }
            return result;
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
        private IEnumerator ShrinkMilkOriginRoutine(Vector2Int origin, GameBoardData gb, BoardManager board)
        {
            var blk = gb.GetBlock(origin.x, origin.y);
            if (blk == null || blk.BlockInstance == null) yield break;
            var t = blk.BlockInstance.transform;
            t.DOScale(Vector3.zero, _milkShrinkTime).SetEase(Ease.InBack);

            float elapsed = 0f;
            while (elapsed < _milkShrinkTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            var selfHit = new List<Vector2Int>(1);
            AddCell(gb, origin.x, origin.y, selfHit);
            if (selfHit.Count > 0) ApplyDamageAndScore(board, selfHit);
        }

        private IEnumerator OvenRoutine(Vector2Int origin, GameBoardData gameBoard, BoardManager board)
        {
            if (gameBoard == null || board == null) yield break;
            if (!TryBeginEffectAt(origin)) yield break;
            BeginEffect();
            Manager.Audio.PlaySFX("Oven_Shake");
            GemType? targetType = null;
            if (board.BlockMover != null)
            {
                Vector2Int start = board.BlockMover.StartBlockPos;
                Vector2Int end = board.BlockMover.EndBlockPos;
                Vector2Int? other = null;
                if (origin == start) other = end;
                else if (origin == end) other = start;

                if (other.HasValue)
                {
                    var ob = gameBoard.GetBlock(other.Value.x, other.Value.y);
                    if (ob != null && ob.BlockInstance != null && ob.GemType < GemType.Milk)
                        targetType = ob.GemType;
                }
            }
            if (!targetType.HasValue)
            {
                Vector2Int[] dirs = { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
                for (int i = 0; i < dirs.Length && !targetType.HasValue; i++)
                {
                    int nx = origin.x + dirs[i].x, ny = origin.y + dirs[i].y;
                    if (nx < 0 || nx >= gameBoard.Width || ny < 0 || ny >= gameBoard.Height) continue;
                    var nb = gameBoard.GetBlock(nx, ny);
                    if (nb != null && nb.BlockInstance != null && nb.GemType < GemType.Milk)
                        targetType = nb.GemType;
                }
            }

            Vector3 originWorld = GridToWorld(board, gameBoard, origin.x, origin.y);
            GameObject oven = Instantiate(_ovenFx, originWorld, Quaternion.identity, board.transform);

            Vector3 cell00 = GridToWorld(board, gameBoard, 0, 0);
            Vector3 cell10 = GridToWorld(board, gameBoard, 1, 0);
            float cellSize = Mathf.Abs(cell10.x - cell00.x);
            float baseScale = 1f;

            if (oven.TryGetComponent<SpriteRenderer>(out var ovenSr))
            {
                ovenSr.sortingOrder = 9999;
                float spriteSize = Mathf.Max(0.0001f, ovenSr.bounds.size.x);
                baseScale = cellSize / spriteSize;
                oven.transform.localScale = Vector3.one * baseScale;
                oven.transform.DOScale(Vector3.one * baseScale * 1.5f, 0.20f).SetEase(Ease.OutBack);
                oven.transform.DORotate(new Vector3(0, 0, 15f), 0.083f)
                    .SetLoops(Mathf.RoundToInt(1.0f / 0.083f), LoopType.Yoyo);
            }

            List<Vector2Int> targets = new List<Vector2Int>(32) { origin };
            if (targetType.HasValue)
            {
                GemType t = targetType.Value;
                for (int y = 0; y < gameBoard.Height; y++)
                    for (int x = 0; x < gameBoard.Width; x++)
                    {
                        var b = gameBoard.GetBlock(x, y);
                        if (b != null && b.BlockInstance != null && b.GemType == t)
                        {
                            if (x == origin.x && y == origin.y) continue;
                            targets.Add(new Vector2Int(x, y));
                        }
                    }
            }
            yield return new WaitForSeconds(_ovenShakeTime);
            Manager.Audio.PlaySFX("Oven_Use");
            ApplyStroke(gameBoard, targets);

            float lineWidth = cellSize * 0.30f;
            List<GameObject> lines = new List<GameObject>(targets.Count);

            const float growTime = 0.40f;
            const float stayTime = 0.20f;

            for (int i = 0; i < targets.Count; i++)
            {
                var p = targets[i];
                if (p == origin) continue;

                Vector3 to = GridToWorld(board, gameBoard, p.x, p.y);

                GameObject go = new GameObject("OvenTraceLine");
                go.transform.SetParent(board.transform, false);

                var lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.positionCount = 2;
                lr.numCapVertices = 4;
                lr.numCornerVertices = 4;
                lr.startWidth = lr.endWidth = lineWidth;
                lr.material = _ovenLineMat;
                //lr.startColor = new Color(1f, 1f, 1f, 0.3f);
                //lr.endColor = new Color(1f, 1f, 1f, 0.5f);


                var rend = lr.GetComponent<Renderer>();
                if (rend != null) { rend.sortingLayerName = "Effects"; rend.sortingOrder = 200; }

                lr.SetPosition(0, originWorld);
                lr.SetPosition(1, originWorld);
                DOTween.To(
                    () => 0f,
                    v => lr.SetPosition(1, Vector3.Lerp(originWorld, to, v)),
                    1f,
                    growTime
                ).SetEase(Ease.OutSine);

                lines.Add(go);

                var tb = gameBoard.GetBlock(p.x, p.y);
                if (tb != null && tb.BlockInstance != null)
                {
                    var t = tb.BlockInstance.transform;
                    var s0 = t.localScale;
                    t.DOScale(s0 * 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
                }
            }

            yield return new WaitForSeconds(growTime + stayTime);
            for (int i = 0; i < lines.Count; i++) Destroy(lines[i]);

            if (oven != null)
            {
                oven.transform.DOScale(Vector3.one * baseScale, 0.15f);
                Destroy(oven, 0.2f);
            }

            var hits = new List<Vector2Int>(targets.Count);
            for (int i = 0; i < targets.Count; i++)
                AddCell(gameBoard, targets[i].x, targets[i].y, hits);
            if (hits.Count > 0) ApplyDamageAndScore(board, hits);

            EndEffect();
            EndEffectAt(origin);
        }

        private IEnumerator DonutRoutine(Vector2Int origin, GameBoardData gameBoard, BoardManager board, int range = 2)
        {
            if (gameBoard == null || board == null) yield break;
            if (!TryBeginEffectAt(origin)) yield break;
            BeginEffect();
            Manager.Audio.PlaySFX("DonutBox_Use");
            Vector3 originWorld = GridToWorld(board, gameBoard, origin.x, origin.y);
            GameObject fx = null;
            if (_donutBurstFx != null)
            {
                fx = Object.Instantiate(_donutBurstFx, originWorld, Quaternion.identity, board.transform);
                if (fx.TryGetComponent<SpriteRenderer>(out var sr)) sr.sortingOrder = 9999;
            }

            yield return new WaitForSeconds(_donutEffectTime);

            var hits = new List<Vector2Int>();
            var queuedSpecials = new List<(Vector2Int pos, GemType type)>();

            for (int y = origin.y - range; y <= origin.y + range; y++)
            {
                for (int x = origin.x - range; x <= origin.x + range; x++)
                {
                    if (x < 0 || x >= gameBoard.Width || y < 0 || y >= gameBoard.Height)
                        continue;

                    var blk = gameBoard.GetBlock(x, y);
                    if (blk != null && blk.BlockInstance != null)
                    {
                        if (!(x == origin.x && y == origin.y) && IsSpecial(blk.GemType))
                        {
                            UseSpecial(new Vector2Int(x, y), blk.GemType, gameBoard, new List<Vector2Int>());
                            continue;
                        }
                    }
                    AddCell(gameBoard, x, y, hits);
                }
            }

            if (hits.Count > 0) ApplyDamageAndScore(board, hits);

            if (fx != null) Object.Destroy(fx);
            EndEffect();
            EndEffectAt(origin);
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
                var overlay = gameBoard.GetOverlayBlock(c.x, c.y);
                if (overlay is ObstacleBlock overlayOb && overlay.BlockInstance != null)
                {
                    var baseBlock = gameBoard.GetBlock(c.x, c.y);
                    if (baseBlock != null && baseBlock.BlockInstance != null)
                        RevertStroke(baseBlock.BlockInstance);
                    overlayOb.TakeDamage();
                    continue;
                }

                var b = gameBoard.GetBlock(c.x, c.y);
                if (b != null && b.BlockInstance != null)
                {
                    if (b.GemType < GemType.Milk)
                        InGameManager.AddIngredientSta(b.GemType);
                    RevertStroke(b.BlockInstance);
                    if (b is ObstacleBlock ob)
                    {
                        if (b.IsNormal != true)
                        {
                            ob.TakeDamage();
                            continue;
                        }
                        ob.TakeDamage();
                        continue;
                    }
                    BoardManager.Instance.PlayMatchExplosion(b.BlockInstance.transform.position);
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
                InGameManager.AddScore(score);
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


            if ((a == GemType.Roller_h && b == GemType.Roller_h) ||
                (a == GemType.Roller_v && b == GemType.Roller_v) ||
                (a == GemType.Roller_h && b == GemType.Roller_v) ||
                (a == GemType.Roller_v && b == GemType.Roller_h))
            {
                var bm = BoardManager.Instance;
                if (bm != null)
                {
                    var selfOnly = new List<Vector2Int>(1) { firstPos };
                    ApplyDamageAndScore(bm, selfOnly);
                    bm.StartCoroutine(RollerRoutine(secondPos, true, gameBoard, bm));
                    bm.StartCoroutine(RollerRoutine(secondPos, false, gameBoard, bm));
                }
                return;
            }

            // 2) 우유 + 우유
            if (a == GemType.Milk && b == GemType.Milk)
            {
                var bm = BoardManager.Instance;
                if (bm != null)
                {
                    bm.StartCoroutine(MilkRoutine(secondPos, gameBoard, bm, _milkDropCountDouble, false));
                }
                return;
            }

            // 3) 도넛 + 도넛
            if (a == GemType.DonutBox && b == GemType.DonutBox)
            {
                var bm = BoardManager.Instance;
                if (bm != null)
                {
                    bm.StartCoroutine(DonutRoutine(secondPos, gameBoard, bm, 4));
                }
                return;
            }

            // 4) 오븐 + 오븐
            if (a == GemType.Oven && b == GemType.Oven)
            {
                var bm = BoardManager.Instance;
                if (bm != null)
                {
                    IEnumerator OvenOvenRoutine()
                    {
                        BeginEffect();
                        Manager.Audio.PlaySFX("Oven_Use");

                        List<Vector2Int> allCells = new List<Vector2Int>();
                        for (int y = 0; y < gameBoard.Height; y++)
                            for (int x = 0; x < gameBoard.Width; x++)
                                allCells.Add(new Vector2Int(x, y));

                        // 보드 전체 스트로크 적용
                        ApplyStroke(gameBoard, allCells);

                        Vector3 originWorld = GridToWorld(bm, gameBoard, secondPos.x, secondPos.y);
                        float cellSize = Mathf.Abs(
                            GridToWorld(bm, gameBoard, 1, 0).x - GridToWorld(bm, gameBoard, 0, 0).x
                        );
                        float lineWidth = cellSize * 0.3f;

                        foreach (var p in allCells)
                        {
                            Vector3 to = GridToWorld(bm, gameBoard, p.x, p.y);
                            GameObject go = new GameObject("OvenOvenLine");
                            go.transform.SetParent(bm.transform, false);

                            var lr = go.AddComponent<LineRenderer>();
                            lr.useWorldSpace = true;
                            lr.positionCount = 2;
                            lr.startWidth = lr.endWidth = lineWidth;
                            lr.material = _ovenLineMat;
                            lr.SetPosition(0, originWorld);
                            lr.SetPosition(1, originWorld);

                            DOTween.To(() => 0f,
                                       v => lr.SetPosition(1, Vector3.Lerp(originWorld, to, v)),
                                       1f, 0.4f)
                                   .SetEase(Ease.OutSine);

                            Destroy(go, 0.7f);
                        }

                        yield return new WaitForSeconds(0.6f);

                        // 전체 블록 파괴
                        if (allCells.Count > 0) ApplyDamageAndScore(bm, allCells);

                        EndEffect();
                    }
                    bm.StartCoroutine(OvenOvenRoutine());
                }
                return;
            }

            // 5) 밀대(세로) + 우유
            if ((a == GemType.Roller_v && b == GemType.Milk) ||
                (a == GemType.Milk && b == GemType.Roller_v))
            {
                var bm = BoardManager.Instance;
                if (bm != null)
                {
                    Vector2Int milkPos = (a == GemType.Milk) ? firstPos : secondPos;

                    bm.StartCoroutine(MilkRoutine(milkPos, gameBoard, bm, _milkDropCount, true, (landed) =>
                    {
                        for (int i = 0; i < landed.Count; i++)
                            bm.StartCoroutine(RollerRoutine(landed[i], false, gameBoard, bm));
                    }));
                }
                return;
            }

            // 6) 밀대(가로) + 우유
            if ((a == GemType.Roller_h && b == GemType.Milk) ||
                (a == GemType.Milk && b == GemType.Roller_h))
            {
                var bm = BoardManager.Instance;
                if (bm != null)
                {
                    Vector2Int milkPos = (a == GemType.Milk) ? firstPos : secondPos;

                    bm.StartCoroutine(MilkRoutine(milkPos, gameBoard, bm, _milkDropCount, true, (landed) =>
                    {
                        // 우유 3발 모두 끝난 뒤 → 그 자리에서 세로 라인 발동
                        for (int i = 0; i < landed.Count; i++)
                            bm.StartCoroutine(RollerRoutine(landed[i], true, gameBoard, bm));
                    }));
                }
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
                var bm = BoardManager.Instance;
                if (bm != null)
                {
                    Vector2Int milkPos = (a == GemType.Milk) ? firstPos : secondPos;
                    bm.StartCoroutine(MilkRoutine(milkPos, gameBoard, bm, _milkDropCount, true, (landed) =>
                    {
                        for (int i = 0; i < landed.Count; i++)
                        {
                            bm.StartCoroutine(DonutRoutine(landed[i], gameBoard, bm, 2));
                        }
                    }));
                }
                return;
            }

            // 10) 오븐 + 우유
            if ((a == GemType.Oven && b == GemType.Milk) ||
                (a == GemType.Milk && b == GemType.Oven))
            {
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

                int pickIdx = Random.Range(0, normalTypes.Count);
                GemType chosenType = normalTypes[pickIdx];

                List<Vector2Int> spawnPositions = new List<Vector2Int>();
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    for (int x = 0; x < gameBoard.Width; x++)
                    {
                        var bl = gameBoard.GetBlock(x, y);
                        if (bl != null && bl.BlockInstance != null && bl.GemType == chosenType)
                            spawnPositions.Add(new Vector2Int(x, y));
                    }
                }

                var bm = BoardManager.Instance;
                if (bm != null)
                {
                    IEnumerator OvenThenMilk()
                    {
                        yield return StartCoroutine(OvenRoutine(secondPos, gameBoard, bm));
                        for (int i = 0; i < spawnPositions.Count; i++)
                        {
                            UseSpecial(spawnPositions[i], GemType.Milk, gameBoard, outDamage);
                        }
                    }

                    bm.StartCoroutine(OvenThenMilk());
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

                int pickIdx = Random.Range(0, normalTypes.Count);
                GemType chosenType = normalTypes[pickIdx];
                List<Vector2Int> spawnPositions = new List<Vector2Int>();
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    for (int x = 0; x < gameBoard.Width; x++)
                    {
                        var bl = gameBoard.GetBlock(x, y);
                        if (bl != null && bl.BlockInstance != null && bl.GemType == chosenType)
                            spawnPositions.Add(new Vector2Int(x, y));
                    }
                }

                var bm = BoardManager.Instance;
                if (bm != null)
                {
                    IEnumerator OvenThenRoller()
                    {
                        yield return StartCoroutine(OvenRoutine(secondPos, gameBoard, bm));
                        for (int i = 0; i < spawnPositions.Count; i++)
                            UseSpecial(spawnPositions[i], lineType, gameBoard, outDamage);
                    }
                    bm.StartCoroutine(OvenThenRoller());
                }
                return;
            }

            // 12) 오븐 + 도넛 
            if ((a == GemType.Oven && b == GemType.DonutBox) ||
                (a == GemType.DonutBox && b == GemType.Oven))
            {
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

                int pickIdx = Random.Range(0, normalTypes.Count);
                GemType chosenType = normalTypes[pickIdx];
                List<Vector2Int> spawnPositions = new List<Vector2Int>();
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    for (int x = 0; x < gameBoard.Width; x++)
                    {
                        var bl = gameBoard.GetBlock(x, y);
                        if (bl != null && bl.BlockInstance != null && bl.GemType == chosenType)
                            spawnPositions.Add(new Vector2Int(x, y));
                    }
                }

                var bm = BoardManager.Instance;
                if (bm != null)
                {
                    IEnumerator OvenThenDonut()
                    {
                        yield return StartCoroutine(OvenRoutine(secondPos, gameBoard, bm));
                        for (int i = 0; i < spawnPositions.Count; i++)
                        {
                            UseSpecial(spawnPositions[i], GemType.DonutBox, gameBoard, outDamage);
                        }
                    }
                    bm.StartCoroutine(OvenThenDonut());
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
        private class StrokeTag : MonoBehaviour
        {
            public Material original;
            public bool hasOriginal;
        }

        private void ApplyStroke(GameBoardData gb, List<Vector2Int> cells)
        {
            if (_ovenStrokeMat == null || gb == null || cells == null) return;

            for (int i = 0; i < cells.Count; i++)
            {
                var p = cells[i];
                var blk = gb.GetBlock(p.x, p.y);
                if (blk == null || blk.BlockInstance == null) continue;

                if (blk.GemType >= GemType.Milk) continue;

                var sr = blk.BlockInstance.GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                var tag = blk.BlockInstance.GetComponent<StrokeTag>();
                if (tag == null) tag = blk.BlockInstance.AddComponent<StrokeTag>();

                if (!tag.hasOriginal)
                {
                    tag.original = sr.material;
                    tag.hasOriginal = true;
                }
                sr.material = _ovenStrokeMat;
            }
        }

        public IEnumerator StrokePulseRoutine(GameBoardData gb, List<Vector2Int> cells, float duration, bool includeSpecial = true)
        {
            if (gb == null || cells == null || cells.Count == 0) yield break;
            if (_ovenStrokeMat == null) yield break;

            for (int i = 0; i < cells.Count; i++)
            {
                var p = cells[i];
                var blk = gb.GetBlock(p.x, p.y);
                if (blk == null || blk.BlockInstance == null) continue;

                if (!includeSpecial && blk.GemType >= GemType.Milk) continue;

                var sr = blk.BlockInstance.GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                var tag = blk.BlockInstance.GetComponent<StrokeTag>();
                if (tag == null) tag = blk.BlockInstance.AddComponent<StrokeTag>();

                if (!tag.hasOriginal)
                {
                    tag.original = sr.material;
                    tag.hasOriginal = true;
                }
                sr.material = _ovenStrokeMat;
            }

            yield return new WaitForSeconds(duration);

            for (int i = 0; i < cells.Count; i++)
            {
                var p = cells[i];
                var blk = gb.GetBlock(p.x, p.y);
                if (blk == null || blk.BlockInstance == null) continue;
                RevertStroke(blk.BlockInstance);
            }
        }

        private void RevertStroke(GameObject go)
        {
            if (go == null) return;
            var sr = go.GetComponent<SpriteRenderer>();
            var tag = go.GetComponent<StrokeTag>();
            if (sr != null && tag != null && tag.hasOriginal)
            {
                sr.material = tag.original;
                Destroy(tag);
            }
        }
    }
}
