using DG.Tweening;
using KDJ;
using KDJ.States;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    public class ItemCheck : MonoBehaviour
    {
        [SerializeField] private BoardManager _board;

        [Header("가위")]
        [SerializeField] private GameObject _scissorHFx;
        [SerializeField] private GameObject _scissorVFx;
        [SerializeField] private float _scissorFxMs;

        [Header("거품기")]
        [SerializeField] private GameObject _whiskCenterFx;
        [SerializeField] private GameObject _whiskAreaFx;
        [SerializeField] private float _whiskRotateMs;
        [SerializeField] private float _whiskAreaFadeMs;

        [Header("도넛판")]
        [SerializeField] private Transform _boardRoot;
        [SerializeField] private Transform _poolsRoot;
        [SerializeField] private float _donutMoveDist;

        [Header("커피")]
        [SerializeField] private RectTransform _coffeeIconRt;
        [SerializeField] private GameObject _coffeeFx; 
        [SerializeField] private float _coffeeDurationMs;
        [SerializeField] private float _coffeeZigzagHeight;
        [SerializeField] private float _coffeeZigzagAmp;
        private void Update()
        {
            if (_board == null || !_board.IsItemSelected || !(BoardManager.Instance.CurrentState is ReadyState) || _board.IsItemEffectRunning || BoardManager.Instance.IsClearSpecialTime || InGameManager.GetStageClear())
                return;

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouse = Input.mousePosition;
                mouse.z = -Camera.main.transform.position.z;
                Vector3 world = Camera.main.ScreenToWorldPoint(mouse);

                int w = _board.Spawner.GameBoardData.BlockPlate.BlockPlateWidth;
                int h = _board.Spawner.GameBoardData.BlockPlate.BlockPlateHeight;
                Vector2 target = new Vector2(world.x, world.y);
                Vector2Int grid = _board.BlockMover.WorldToGrid(target, w, h);

                if (!InBounds(grid))
                {
                    _board.ClearItemSelection();
                    return;
                }
                var sp = _board.Spawner;
                var blk = sp.GameBoardData.BlockArray[grid.y, grid.x];
                if (blk == null || blk.BlockInstance == null)
                {
                    _board.ClearItemSelection();
                    return;
                }

                // 선택된 아이템 발동
                BoardManager.SetTouch(false);
                UseCell(grid, _board.SelectedItemType);

                // 발동 후 해제
                _board.ClearItemSelection();
            }
        }
        private bool IsSpecialType(GemType t)
        {
            return t == GemType.Roller_h
                || t == GemType.Roller_v
                || t == GemType.Milk
                || t == GemType.DonutBox
                || t == GemType.Oven;
        }

        // 커피 아이템
        public bool UseCoffee(float amount = 30f)
        {
            if (BoardManager.Instance.IsClearSpecialTime || InGameManager.GetStageClear())
                return false;
            var order = FindObjectOfType<OrderStateController>();

            order.AddPatience(amount);
            Manager.Audio.PlaySFX("Coffe_Use");
            PlayCoffeeFxFromButton();
            return true;
        }

        // 좌표 확인
        private bool InBounds(Vector2Int p)
        {
            var sp = _board.Spawner;
            int w = sp.GameBoardData.BlockPlate.BlockPlateWidth;
            int h = sp.GameBoardData.BlockPlate.BlockPlateHeight;
            if (p.x < 0 || p.x >= w || p.y < 0 || p.y >= h)
                return false;

            return sp.GameBoardData.BlockPlate.BlockPlateArray[p.y, p.x];
        }


        // 아이템 별 해당 아이템 효과 실행
        private void UseCell(Vector2Int pos, ItemType type)
        {
            int destroyed = 0;
            BoardManager.Instance.HintManager.StopHintTimer();
            switch (type)
            {
                case ItemType.Scissors:
                    StartCoroutine(ApplyScissor(pos));
                    Manager.User.UseItem(type);
                    break;
                case ItemType.Whisk:
                    StartCoroutine(ApplyWhisk(pos));
                    Manager.User.UseItem(type);
                    break;
            }
        }

        // 가위: 선택 지점의 가로+세로 제거
        private IEnumerator ApplyScissor(Vector2Int pos)
        {
            _board.IsItemEffectRunning = true;
            Manager.Audio.PlaySFX("Sciccors_Use");

            var sp = _board.Spawner;
            int w = sp.GameBoardData.BlockPlate.BlockPlateWidth;
            int h = sp.GameBoardData.BlockPlate.BlockPlateHeight;

            float stepWait = Mathf.Max(0.01f, (_scissorFxMs * 0.001f) / Mathf.Max(w, h));
            Vector3 GridToWorld(int gx, int gy)
            {
                return _board.BlockMover.GridToWorld(new Vector2Int(gx, gy), w, h);
            }

            // 연출 FX
            GameObject fxH = null, fxV = null;
            float hDuration = w * stepWait;
            float vDuration = h * stepWait;

            if (_scissorHFx != null)
            {
                Vector3 hStart = GridToWorld(0, pos.y);
                Vector3 hEnd = GridToWorld(w - 1, pos.y);

                fxH = Instantiate(_scissorHFx, hStart, Quaternion.identity, _board.transform);
                fxH.transform.DOMove(hEnd, hDuration).SetEase(Ease.Linear).OnComplete(() =>
                {
                    if (fxH != null) Destroy(fxH);
                });
            }

            if (_scissorVFx != null)
            {
                Vector3 vStart = GridToWorld(pos.x, h - 1);
                Vector3 vEnd = GridToWorld(pos.x, 0);

                fxV = Instantiate(_scissorVFx, vStart, Quaternion.Euler(0, 0, 270), _board.transform);
                fxV.transform.DOMove(vEnd, vDuration).SetEase(Ease.Linear).OnComplete(() =>
                {
                    if (fxV != null) Destroy(fxV);
                });
            }

            int destroyedCount = 0;
            HashSet<Vector2Int> targetCoords = new HashSet<Vector2Int>();
            HashSet<Vector2Int> triggeredSpecial = new HashSet<Vector2Int>();
            HashSet<Vector2Int> pendingClear = new HashSet<Vector2Int>();

            int max = Mathf.Max(w, h);
            for (int i = 0; i < max; i++)
            {
                if (i < w)
                {
                    int x = i;
                    var blk = sp.GameBoardData.BlockArray[pos.y, x];

                    if (blk != null)
                    {
                        if (blk.GemType == GemType.Flour_s || blk.GemType == GemType.FlourBag)
                        {
                            if (blk is FlourBag flourBag && targetCoords.Add(new Vector2Int(x, pos.y)))
                            {
                                flourBag.TakeDamage();
                            }
                            else if (blk is FlourBag_s flourBag_s && targetCoords.Add(flourBag_s.OwnerPos()))
                            {
                                flourBag_s.TakeDamage();
                            }
                        }
                        else
                        {
                            if (IsSpecialType(blk.GemType))
                            {
                                Vector2Int p = new Vector2Int(x, pos.y);
                                if (triggeredSpecial.Add(p))
                                    TriggerSpecial(p, blk.GemType);
                            }
                            else
                            {
                                // 오버레이 우선
                                if (sp.GameBoardData.OverlayArray[pos.y, x] is ObstacleBlock overlayBlock)
                                {
                                    overlayBlock.TakeDamage();
                                }
                                else
                                {
                                    if (blk is ObstacleBlock obstacle)
                                    {
                                        if (blk.GemType == GemType.Syrup || blk.GemType == GemType.Egg)
                                            InGameManager.AddIngredientSta(blk.GemType);
                                        obstacle.TakeDamage();
                                    }
                                    else
                                    {
                                        if (blk.BlockInstance != null && blk.BlockInstance.TryGetComponent<PooledObject>(out var pooled))
                                        {
                                            BoardManager.Instance.PlayMatchExplosion(blk.BlockInstance.transform.position);
                                            Manager.Audio.PlaySFX("Block_Match");
                                            InGameManager.AddIngredientSta(blk.GemType);
                                            pooled.ReturnToPool();
                                        }
                                        else if (blk.BlockInstance != null)
                                        {
                                            BoardManager.Instance.PlayMatchExplosion(blk.BlockInstance.transform.position);
                                            Manager.Audio.PlaySFX("Block_Match");
                                            InGameManager.AddIngredientSta(blk.GemType);
                                            Destroy(blk.BlockInstance);
                                        }
                                        blk.BlockInstance = null;
                                        pendingClear.Add(new Vector2Int(x, pos.y));
                                        destroyedCount++;
                                    }
                                }
                            }
                        }
                    }
                }

                if (i < h)
                {
                    int y = (h - 1) - i;
                    var blk = sp.GameBoardData.BlockArray[y, pos.x];

                    if (blk != null)
                    {
                        if (blk.GemType == GemType.Flour_s || blk.GemType == GemType.FlourBag)
                        {
                            if (blk is FlourBag flourBag && targetCoords.Add(new Vector2Int(pos.x, y)))
                            {
                                flourBag.TakeDamage();
                            }
                            else if (blk is FlourBag_s flourBag_s && targetCoords.Add(flourBag_s.OwnerPos()))
                            {
                                flourBag_s.TakeDamage();
                            }
                        }
                        else
                        {
                            if (IsSpecialType(blk.GemType))
                            {
                                Vector2Int p = new Vector2Int(pos.x, y);
                                if (triggeredSpecial.Add(p))
                                    TriggerSpecial(p, blk.GemType);
                            }
                            else
                            {
                                if (sp.GameBoardData.OverlayArray[y, pos.x] is ObstacleBlock overlayBlock)
                                {
                                    overlayBlock.TakeDamage();
                                }
                                else
                                {
                                    if (blk is ObstacleBlock obstacle)
                                    {
                                        if (blk.GemType == GemType.Syrup || blk.GemType == GemType.Egg)
                                            InGameManager.AddIngredientSta(blk.GemType);
                                        obstacle.TakeDamage();
                                    }
                                    else
                                    {
                                        if (blk.BlockInstance != null && blk.BlockInstance.TryGetComponent<PooledObject>(out var pooled))
                                        {
                                            BoardManager.Instance.PlayMatchExplosion(blk.BlockInstance.transform.position);
                                            InGameManager.AddIngredientSta(blk.GemType);
                                            pooled.ReturnToPool();
                                        }
                                        else if (blk.BlockInstance != null)
                                        {
                                            BoardManager.Instance.PlayMatchExplosion(blk.BlockInstance.transform.position);
                                            InGameManager.AddIngredientSta(blk.GemType);
                                            Destroy(blk.BlockInstance);
                                        }
                                        blk.BlockInstance = null;
                                        pendingClear.Add(new Vector2Int(pos.x, y));
                                        destroyedCount++;
                                    }
                                }
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(stepWait);
            }

            if (destroyedCount > 0)
                InGameManager.AddScore(destroyedCount * 10);
            yield return new WaitUntil(() => SpecialBlockEffect.effectRunning == false);
            foreach (var p in pendingClear)
            {
                if (p.y >= 0 && p.y < sp.GameBoardData.BlockPlate.BlockPlateHeight &&
                    p.x >= 0 && p.x < sp.GameBoardData.BlockPlate.BlockPlateWidth)
                {
                    sp.GameBoardData.BlockArray[p.y, p.x] = null;
                }
            }

            _board.IsItemEffectRunning = false;
            BoardManager.SetTouch(true);
            _board.ChangeState(new RefillState());
        }

        private IEnumerator ApplyWhisk(Vector2Int pos)
        {
            _board.IsItemEffectRunning = true;
            Manager.Audio.PlaySFX("Whisk_Use");
            var sp = _board.Spawner;
            int w = sp.GameBoardData.BlockPlate.BlockPlateWidth;
            int h = sp.GameBoardData.BlockPlate.BlockPlateHeight;

            int destroyedCount = 0;
            HashSet<Vector2Int> targetCoords = new HashSet<Vector2Int>();
            HashSet<Vector2Int> triggeredSpecial = new HashSet<Vector2Int>();
            List<Vector2Int> area = new List<Vector2Int>(9);

            int r = 1;
            for (int y = pos.y - r; y <= pos.y + r; y++)
            {
                if (y < 0 || y >= h) continue;
                for (int x = pos.x - r; x <= pos.x + r; x++)
                {
                    if (x < 0 || x >= w) continue;
                    area.Add(new Vector2Int(x, y));
                }
            }

            Vector3 GridToWorld(int gx, int gy) => _board.BlockMover.GridToWorld(new Vector2Int(gx, gy), w, h);

            float rotateSec = Mathf.Max(0.01f, _whiskRotateMs * 0.001f);
            if (_whiskCenterFx != null)
            {
                Vector3 centerWorld = GridToWorld(pos.x, pos.y);
                float cell = Vector3.Distance(GridToWorld(pos.x + 1, pos.y), centerWorld);
                float radius = cell * 1.0f;

                const int points = 24;
                var path = new Vector3[points + 1];
                for (int i = 0; i <= points; i++)
                {
                    float t = (i / (float)points) * Mathf.PI * 2f;
                    path[i] = new Vector3(centerWorld.x + Mathf.Cos(t) * radius,
                                          centerWorld.y + Mathf.Sin(t) * radius,
                                          centerWorld.z);
                }

                var whiskFx = Instantiate(_whiskCenterFx, path[0], Quaternion.identity, _board.transform);
                whiskFx.transform.DOPath(path, rotateSec, PathType.CatmullRom, PathMode.TopDown2D, 10, Color.white)
                       .SetEase(Ease.Linear)
                       .OnComplete(() => { if (whiskFx != null) Destroy(whiskFx); });
                yield return new WaitForSeconds(rotateSec);
            }

            bool areaFaded = true;
            float fadeSec = Mathf.Max(0.01f, _whiskAreaFadeMs * 0.001f);
            if (_whiskAreaFx != null)
            {
                areaFaded = false;
                Vector3 centerWorld = GridToWorld(pos.x, pos.y);
                var areaFx = Instantiate(_whiskAreaFx, centerWorld, Quaternion.identity, _board.transform);

                if (areaFx.TryGetComponent<SpriteRenderer>(out var areaSr) && areaSr.sprite != null)
                {
                    float cellW = Vector3.Distance(GridToWorld(pos.x + 1, pos.y), centerWorld);
                    float cellH = Vector3.Distance(GridToWorld(pos.x, pos.y + 1), centerWorld);

                    Vector2 spriteSize = areaSr.sprite.bounds.size;
                    float sx = (3f * cellW) / Mathf.Max(0.0001f, spriteSize.x);
                    float sy = (3f * cellH) / Mathf.Max(0.0001f, spriteSize.y);
                    areaFx.transform.localScale = new Vector3(sx, sy, 1f);

                    areaSr.DOFade(0f, fadeSec).OnComplete(() =>
                    {
                        if (areaFx != null) Destroy(areaFx);
                        areaFaded = true;
                    });
                }
                else
                {
                    yield return new WaitForSeconds(fadeSec);
                    if (areaFx != null) Destroy(areaFx);
                    areaFaded = true;
                }
            }

            if (!areaFaded)
                yield return new WaitUntil(() => areaFaded);
            bool playedMatchSfx = false;

            // 블록 파괴 처리
            for (int i = 0; i < area.Count; i++)
            {
                int x = area[i].x;
                int y = area[i].y;

                var blk = sp.GameBoardData.BlockArray[y, x];
                if (blk == null) continue;
                if (blk.GemType != GemType.Flour_s && blk.BlockInstance == null)
                    continue;

                if (blk.GemType == GemType.Flour_s || blk.GemType == GemType.FlourBag)
                {
                    if (blk is FlourBag bag && targetCoords.Add(new Vector2Int(x, y))) { bag.TakeDamage(); continue; }
                    else if (blk is FlourBag_s bagS && targetCoords.Add(bagS.OwnerPos())) { bagS.TakeDamage(); continue; }
                    else if (blk is FlourBag_s) continue;
                }

                if (sp.GameBoardData.OverlayArray[y, x] is ObstacleBlock overlayOb) { overlayOb.TakeDamage(); continue; }

                if (IsSpecialType(blk.GemType))
                {
                    Vector2Int p = new Vector2Int(x, y);
                    if (triggeredSpecial.Add(p))
                        TriggerSpecial(p, blk.GemType);
                    continue;
                }

                if (blk is ObstacleBlock obstacle)
                {
                    if (blk.GemType == GemType.Syrup || blk.GemType == GemType.Egg)
                        InGameManager.AddIngredientSta(blk.GemType);
                    obstacle.TakeDamage();
                    continue;
                }

                if (blk.BlockInstance != null && blk.BlockInstance.TryGetComponent<PooledObject>(out var pooled))
                {
                    BoardManager.Instance.PlayMatchExplosion(blk.BlockInstance.transform.position);

                    if (!playedMatchSfx)
                    {
                        Manager.Audio.PlaySFX("Block_Match");
                        playedMatchSfx = true;
                    }

                    InGameManager.AddIngredientSta(blk.GemType);
                    pooled.ReturnToPool();
                }
                else if (blk.BlockInstance != null)
                {
                    BoardManager.Instance.PlayMatchExplosion(blk.BlockInstance.transform.position);

                    if (!playedMatchSfx)
                    {
                        Manager.Audio.PlaySFX("Block_Match");
                        playedMatchSfx = true;
                    }

                    InGameManager.AddIngredientSta(blk.GemType);
                    Destroy(blk.BlockInstance);
                }

                sp.GameBoardData.BlockArray[y, x].BlockInstance = null;
                destroyedCount++;
            }

            if (destroyedCount > 0) InGameManager.AddScore(destroyedCount * 10);
            yield return new WaitUntil(() => SpecialBlockEffect.effectRunning == false);
            _board.IsItemEffectRunning = false;
            BoardManager.SetTouch(true);
            _board.ChangeState(new RefillState());
        }

        // 도넛판 아이템 실행
        public bool UseDonutPan()
        {
            if (_board == null) return false;
            if (!(BoardManager.Instance.CurrentState is ReadyState)) return false;
            if (_board.IsItemEffectRunning) return false;
            if (BoardManager.Instance.IsClearSpecialTime || InGameManager.GetStageClear()) return false;

            _board.ClearItemSelection();
            BoardManager.Instance.HintManager.StopHintTimer();

            StartCoroutine(RegenSpecialBlockRoutine());
            return true;
        }

        // 보드를 초기화 후 특수 블록 하나 생성하는 루틴
        private IEnumerator RegenSpecialBlockRoutine()
        {
            _board.IsItemEffectRunning = true;
            Manager.Audio.PlaySFX("DonutPan_Use");
            float moveDuration = 2.0f;

            Vector3 boardStart = _boardRoot.localPosition;
            Vector3 poolStart = _poolsRoot.position;

            var down = DOTween.Sequence()
                .Join(_boardRoot.DOLocalMoveY(boardStart.y - _donutMoveDist, moveDuration))
                .Join(_poolsRoot.DOLocalMoveY(poolStart.y - _donutMoveDist, moveDuration));
            yield return down.WaitForCompletion();

            _board.Spawner.Shuffle(_board);
            yield return null;
            _board.ChangeState(new RefillState());
            InjectRandomSpecial();

            Manager.Audio.PlaySFX("DonutPan_Use");
            var up = DOTween.Sequence()
                .Join(_boardRoot.DOLocalMove(boardStart, moveDuration))
                .Join(_poolsRoot.DOLocalMove(poolStart, moveDuration));
            yield return up.WaitForCompletion();
            BoardManager.Instance.HintManager.StopHintTimer();
            _board.IsItemEffectRunning = false;
            BoardManager.Instance.ChangeState(new ReadyState());
        }

        // 랜덤 위치에 특수 블록 생성
        private void InjectRandomSpecial()
        {
            var sp = _board.Spawner;
            int w = sp.GameBoardData.BlockPlate.BlockPlateWidth;
            int h = sp.GameBoardData.BlockPlate.BlockPlateHeight;

            List<Vector2Int> candidates = new List<Vector2Int>();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!sp.GameBoardData.BlockPlate.BlockPlateArray[y, x]) continue;
                    var blk = sp.GameBoardData.BlockArray[y, x];
                    var ovl = sp.GameBoardData.OverlayArray[y, x];
                    if (blk == null || blk.IsObstacle) continue;
                    if (ovl != null && ovl.GemType == GemType.Ice) continue;

                    bool isSpecial = (blk.GemType > GemType.Sugar && blk.GemType < GemType.Dust);
                    if (!isSpecial) candidates.Add(new Vector2Int(x, y));
                }
            }
            if (candidates.Count == 0) return;

            Vector2Int pick = candidates[Random.Range(0, candidates.Count)];

            var cur = sp.GameBoardData.BlockArray[pick.y, pick.x];
            if (cur?.BlockInstance != null)
            {
                Destroy(cur.BlockInstance);
                cur.BlockInstance = null;
            }

            int min = (int)GemType.Sugar + 1;
            int max = (int)GemType.Dust - 1;
            GemType special = (GemType)Random.Range(min, max + 1);


            sp.SpawnBlock(pick.x, pick.y, special, _board.BlockMover);
            var cell = sp.GameBoardData.BlockArray[pick.y, pick.x];
            if (cell != null && cell.BlockInstance != null)
            {
                Transform t = cell.BlockInstance.transform;

                if (_poolsRoot != null && !t.IsChildOf(_poolsRoot) && !_boardRoot.IsChildOf(t))
                {
                    t.SetParent(_poolsRoot, true);
                }
                t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y - _donutMoveDist, t.localPosition.z);
            }
        }
        private void TriggerSpecial(Vector2Int p, GemType t)
        {
            var sbe = FindObjectOfType<SpecialBlockEffect>();
            if (sbe == null) return;

            var gb = _board.Spawner.GameBoardData;
            var hits = new List<Vector2Int>();
            sbe.UseSpecial(p, t, gb, hits);                
            if (hits.Count > 0) sbe.ApplyDamageAndScore(_board, hits);  
        }
        private void PlayCoffeeFxFromButton()
        {
            if (_coffeeFx == null || _coffeeIconRt == null) return;
            var parent = _coffeeIconRt.parent as RectTransform;
            var go = Instantiate(_coffeeFx, parent);

            var fxRt = go.GetComponent<RectTransform>();
            fxRt.anchoredPosition3D = _coffeeIconRt.anchoredPosition3D; 
            fxRt.localScale = Vector3.one;

            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            float dur = Mathf.Max(0.01f, _coffeeDurationMs * 0.001f);

            Vector2 p0 = fxRt.anchoredPosition;
            Vector2 p1 = p0 + new Vector2(+_coffeeZigzagAmp, _coffeeZigzagHeight * 0.33f);
            Vector2 p2 = p0 + new Vector2(-_coffeeZigzagAmp, _coffeeZigzagHeight * 0.66f);
            Vector2 p3 = p0 + new Vector2(+_coffeeZigzagAmp, _coffeeZigzagHeight);

            var seq = DOTween.Sequence();
            seq.Append(fxRt.DOAnchorPos(p1, dur / 3f).SetEase(Ease.Linear));
            seq.Append(fxRt.DOAnchorPos(p2, dur / 3f).SetEase(Ease.Linear));
            seq.Append(fxRt.DOAnchorPos(p3, dur / 3f).SetEase(Ease.Linear));

            cg.DOFade(0f, dur);

            seq.OnComplete(() =>
            {
                if (go != null) Destroy(go);
            });
        }
    }
}
