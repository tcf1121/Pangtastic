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

        [Header("우유")]
        [SerializeField] private GameObject _milkDropFx;   
        [SerializeField] private GameObject _milkSplashFx;
        [SerializeField] private float _milkFlyTime;

        [Header("오븐")]
        [SerializeField] private GameObject _ovenFx;          // 오븐 본체 이미지

        [Header("도넛상자")]
        [SerializeField] private GameObject _donutBurstFx;
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
                var bm = BoardManager.Instance;
                if (bm != null)
                    bm.StartCoroutine(MilkRoutine(pos, gameBoard, bm));
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
        // 밀대(가로,세로)
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

        private IEnumerator MilkRoutine(Vector2Int origin, GameBoardData gameBoard, BoardManager board, System.Action<List<Vector2Int>> onCompleted = null)
        {
            if (gameBoard == null || board == null) yield break;
            BeginEffect();

            // 0) 타겟 선정: 필요 재료 우선, 없으면 랜덤 노말 3개
            List<Vector2Int> targets = GetMilkTargets(gameBoard, 3);
            if (targets.Count == 0) { EndEffect(); yield break; }

            var selfHit = new List<Vector2Int>(1);
            AddCell(gameBoard, origin.x, origin.y, selfHit);
            if (selfHit.Count > 0) ApplyDamageAndScore(board, selfHit);

            Vector3 originWorld = GridToWorld(board, gameBoard, origin.x, origin.y);

            int finished = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                Vector2Int tg = targets[i];
                Vector3 targetWorld = GridToWorld(board, gameBoard, tg.x, tg.y);
                Vector3 mid = (originWorld + targetWorld) * 0.5f;
                mid += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.3f, 0.6f), 0f);

                GameObject drop = Instantiate(_milkDropFx, originWorld, Quaternion.identity, board.transform);
                if (drop.TryGetComponent<SpriteRenderer>(out var sr)) sr.sortingOrder = 9999;

                Vector3[] path = new Vector3[] { originWorld, mid, targetWorld };
                drop.transform.DOMove(targetWorld, _milkFlyTime).SetEase(Ease.InOutSine).OnUpdate(() =>

                {
                    // 흔들림: sin 파동으로 좌우 이동
                    float wobble = Mathf.Sin(Time.time * 20f) * 0.2f;
                    drop.transform.position += new Vector3(wobble, 0f, 0f);
                })

                    .OnComplete(() =>
                    {
                        // 스플래시 FX
                        if (_milkSplashFx != null)
                        {
                            var splash = Instantiate(_milkSplashFx, targetWorld, Quaternion.identity, board.transform);
                            Destroy(splash, 0.5f); // 0.5초 뒤 자동 제거 → 이미지 안 남음
                        }
                        // 타겟 블록 제거
                        var one = new List<Vector2Int>(1) { tg };
                        if (one.Count > 0) ApplyDamageAndScore(board, one);

                        Destroy(drop);
                        finished++;
                    });
            }

            // 모든 드롭이 끝날 때까지 대기
            float elapsed = 0f;
            float timeout = _milkFlyTime + 0.25f; // 여유 버퍼
            while (finished < targets.Count && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (onCompleted != null)
                onCompleted(targets);
            EndEffect();
        }
        private List<Vector2Int> GetMilkTargets(GameBoardData gb, int count)
        {
            // 필요 재료 우선
            // List<GemType> needed = InGameManager.GetTagetGem();
            List<GemType> needed = new List<GemType>(); 

            List<Vector2Int> result = new List<Vector2Int>();

            if (needed != null && needed.Count > 0)
            {
                List<Vector2Int> candidates = GetTargetPos(gb, needed);  
                Shuffle(candidates);                                     
                int take = Mathf.Min(count, candidates.Count);
                for (int i = 0; i < take; i++) result.Add(candidates[i]);
            }

            // 노말 젬에서 랜덤 보충
            if (result.Count < count)
            {
                List<Vector2Int> normals = new List<Vector2Int>();
                for (int y = 0; y < gb.Height; y++)
                {
                    for (int x = 0; x < gb.Width; x++)
                    {
                        var b = gb.GetBlock(x, y);
                        if (b != null && b.BlockInstance != null && b.GemType < GemType.Milk)
                            normals.Add(new Vector2Int(x, y));
                    }
                }
                Shuffle(normals);
                for (int i = 0; i < normals.Count && result.Count < count; i++)
                {
                    Vector2Int v = normals[i];
                    bool exists = false;
                    for (int j = 0; j < result.Count; j++) if (result[j] == v) { exists = true; break; }
                    if (!exists) result.Add(v);
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
        private IEnumerator OvenRoutine(Vector2Int origin, GameBoardData gameBoard, BoardManager board)
        {
            if (gameBoard == null || board == null) yield break;
            BeginEffect();
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

            if (oven.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr.sortingOrder = 9999;

                float spriteSize = Mathf.Max(0.0001f, sr.bounds.size.x);
                baseScale = cellSize / spriteSize;
                oven.transform.localScale = Vector3.one * baseScale;
                oven.transform.DOScale(Vector3.one * baseScale * 1.5f, 0.20f).SetEase(Ease.OutBack);

                float shakeDur = 1.0f;
                oven.transform.DORotate(new Vector3(0, 0, 15f), 0.083f)
                    .SetLoops(Mathf.RoundToInt(shakeDur / 0.083f), LoopType.Yoyo);
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

            float lineWidth = cellSize * 0.12f;
            List<GameObject> lines = new List<GameObject>(targets.Count);
            for (int i = 0; i < targets.Count; i++)
            {
                var p = targets[i];
                if (p.x == origin.x && p.y == origin.y) continue;

                Vector3 to = GridToWorld(board, gameBoard, p.x, p.y);

                GameObject go = new GameObject("OvenTraceLine");
                go.transform.SetParent(board.transform, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.positionCount = 2;
                lr.numCapVertices = 4;
                lr.numCornerVertices = 4;
                lr.startWidth = lr.endWidth = lineWidth;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = lr.endColor = Color.white;
                var rend = lr.GetComponent<Renderer>();
                if (rend != null) { rend.sortingLayerName = "Effects"; rend.sortingOrder = 200; }

                lr.SetPosition(0, originWorld);
                lr.SetPosition(1, originWorld);
                DOTween.To(() => 0f,v => lr.SetPosition(1, Vector3.Lerp(originWorld, to, v)), 1f, 1.0f).SetEase(Ease.OutSine);

                lines.Add(go);
            }

            yield return new WaitForSeconds(1.0f);

            for (int i = 0; i < targets.Count; i++)
            {
                var b = gameBoard.GetBlock(targets[i].x, targets[i].y);
                if (b != null && b.BlockInstance != null)
                {
                    Transform t = b.BlockInstance.transform;
                    Vector3 s0 = t.localScale;
                    t.DOScale(s0 * 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
                }
            }
            yield return new WaitForSeconds(0.8f);

            for (int i = 0; i < lines.Count; i++) Destroy(lines[i]);
            if (oven != null)
            {
                oven.transform.DOScale(Vector3.one * baseScale, 0.15f);
                Destroy(oven, 0.2f);
            }

            // 파괴
            var hits = new List<Vector2Int>(targets.Count);
            for (int i = 0; i < targets.Count; i++)
                AddCell(gameBoard, targets[i].x, targets[i].y, hits);
            if (hits.Count > 0) ApplyDamageAndScore(board, hits);

            EndEffect();
        }

        private IEnumerator DonutRoutine(Vector2Int origin, GameBoardData gameBoard, BoardManager board)
        {
            if (gameBoard == null || board == null) yield break;
            BeginEffect();

            // 원점/셀 크기
            Vector3 originWorld = GridToWorld(board, gameBoard, origin.x, origin.y);
            Vector3 c0 = GridToWorld(board, gameBoard, 0, 0);
            Vector3 c1 = GridToWorld(board, gameBoard, 1, 0);
            float cellSize = Mathf.Abs(c1.x - c0.x);

            int left = Mathf.Min(2, origin.x);
            int right = Mathf.Min(2, gameBoard.Width - 1 - origin.x);
            int down = Mathf.Min(2, origin.y);
            int up = Mathf.Min(2, gameBoard.Height - 1 - origin.y);

            int cellsX = left + right + 1;  
            int cellsY = down + up + 1;

            float areaX = cellSize * cellsX;
            float areaY = cellSize * cellsY;

            GameObject fx = null;
            if (_donutBurstFx != null)
            {
                fx = Object.Instantiate(_donutBurstFx, originWorld, Quaternion.identity, board.transform);
                if (fx.TryGetComponent<SpriteRenderer>(out var sr))
                {
                    sr.sortingOrder = 9999;

                    Vector2 sprLocal = sr.sprite.bounds.size;
                    Vector3 parentLossy = (fx.transform.parent != null) ? fx.transform.parent.lossyScale : Vector3.one;

                    float scaleX = (areaX / Mathf.Max(0.0001f, sprLocal.x)) / Mathf.Max(0.0001f, parentLossy.x);
                    float scaleY = (areaY / Mathf.Max(0.0001f, sprLocal.y)) / Mathf.Max(0.0001f, parentLossy.y);

                    fx.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }
            }

            yield return new WaitForSeconds(0.5f);

            var hits = new List<Vector2Int>();
            for (int y = origin.y - 2; y <= origin.y + 2; y++)
            {
                for (int x = origin.x - 2; x <= origin.x + 2; x++)
                {
                    if (x < 0 || x >= gameBoard.Width || y < 0 || y >= gameBoard.Height) continue;
                    AddCell(gameBoard, x, y, hits);
                }
            }
            if (hits.Count > 0) ApplyDamageAndScore(board, hits);

            if (fx != null) Object.Destroy(fx);
            EndEffect();
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
                    DOTween.Kill(b.BlockInstance.transform, complete: true);
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
                var bm = BoardManager.Instance;
                if (bm != null)
                {
                    Vector2Int milkPos = (a == GemType.Milk) ? firstPos : secondPos;

                    bm.StartCoroutine(MilkRoutine(milkPos, gameBoard, bm, (landed) =>
                    {
                        // 우유 3발 모두 끝난 뒤 → 그 자리에서 세로 라인 발동
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

                    bm.StartCoroutine(MilkRoutine(milkPos, gameBoard, bm, (landed) =>
                    {
                        // 우유 3발 모두 끝난 뒤 → 그 자리에서 가로 라인 발동
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
