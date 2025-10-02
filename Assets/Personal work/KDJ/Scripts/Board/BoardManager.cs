using KDJ.States;
using LHJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace KDJ
{
    public class BoardManager : MonoBehaviour
    {
        public class MatchAnimationData
        {
            public HashSet<Vector2Int> CoordsToDestroy { get; set; }
            public (Vector2Int pos, int type)? SpecialToCreate { get; set; }
            public Vector2Int? SpecialSpawnPos { get; set; }
            public Vector2Int? SwapPosition { get; set; }
        }

        public bool IsBoardBusy
        {
            get
            {
                return IsWaitingForAnimation ||
                SpecialBlockEffect.effectRunning ||
                IsItemEffectRunning ||
                !(CurrentState is ReadyState) ||
                MatchChecker.AllBlockMatchCheck(this);
            }
        }

        [SerializeField] private TMP_Text _blockInfo;
        [SerializeField] private TMP_Text _scoreInfo;
        public bool IsTest = false;
        [SerializeField] private CameraController _cameraController;
        [Header("애니메이션 설정")]
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private ObjectPool _explosionEffectPool;
        [SerializeField] private AnimationCurve _explosionScaleCurve;
        [SerializeField] private AnimationCurve _explosionAlphaCurve;
        [SerializeField] private AnimationCurve _specialBlockCreateCurve;
        [SerializeField] private RectTransform _specialBlockStartPos;

        [Header("가위 선택 UI")]
        [SerializeField] private GameObject _scissorUseUI;

        [Header("거품기 선택 UI")]
        [SerializeField] private GameObject _whiskUseUI;

        public IGameState CurrentState { get; private set; }
        public BlockSpawner Spawner { get; private set; }
        public BoardMatchChecker MatchChecker { get; private set; }
        public BlockMover BlockMover { get; private set; }
        public MatchCombo MatchCombo { get; set; }
        public BoardLoader BoardLoader { get; set; }
        public HintManager HintManager { get; private set; }
        public TestStageManager TestStageManager { get; set; }
        public Tutorial Tutorial;
        public int Score { get; private set; } = 0;
        public int CurStage;
        public static bool CanTouch { get; private set; }
        public float MatchDelay;
        public static BoardManager Instance { get; private set; }
        public bool IsItemSelected { get; private set; } = false;
        public ItemType SelectedItemType { get; private set; }
        public ObjectPool ScoreUIPool;
        public bool IsWaitingForAnimation { get; set; } = false;
        public bool IsReadyForStart { get; set; } = false;
        public int firstCheck { get; set; } = 0;
        public bool IsUseItem { get; set; } = false;
        public bool IsClearSpecialTime { get; set; } = false;
        public bool IsTutorialPlayed { get; set; } = false;
        public bool IsPlayingTutorial { get; set; } = false;
        public bool IsRewardSkipped { get; set; } = false;
        public bool CanSwapBlocks => !IsWaitingForAnimation && !SpecialBlockEffect.effectRunning && !IsUseItem && !MatchChecker.AllBlockMatchCheck(this);
        public bool IsItemEffectRunning { get; set; }
        public Vector2Int? InitialSwapPosition { get; set; } // 한 턴의 스왑 시작 위치를 기억
        public List<Vector2Int> CurHintPositions = new List<Vector2Int>();
        public ObjectPool CoinTextPool;

        private Coroutine _hintCoroutine;
        private List<Material> _activeHintShaders = new List<Material>();

        private Coroutine _clearRewardCoroutine;
        private bool _isClearRewardAnimationPlaying = false;
        private List<GemType> _currentRewards;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (IsTest)
            {
                Spawner = FindObjectOfType<BlockSpawner>();
                MatchChecker = GetComponent<BoardMatchChecker>();
                BlockMover = GetComponent<BlockMover>();
                MatchCombo = GetComponent<MatchCombo>();
                BoardLoader = GetComponent<BoardLoader>();
                HintManager = GetComponent<HintManager>();
                TestStageManager = FindObjectOfType<TestStageManager>();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }


        /// <summary>
        /// 스테이지 초기화 코루틴
        /// 실제로 사용할 코루틴
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        public IEnumerator StageInit(Image progress)
        {
            Spawner = FindObjectOfType<BlockSpawner>();
            MatchChecker = GetComponent<BoardMatchChecker>();
            BlockMover = GetComponent<BlockMover>();
            MatchCombo = GetComponent<MatchCombo>();
            BoardLoader = GetComponent<BoardLoader>();
            HintManager = GetComponent<HintManager>();
            Tutorial = GetComponent<Tutorial>();
            TestStageManager = FindObjectOfType<TestStageManager>();
            CanTouch = false;
            IsReadyForStart = false;
            IsClearSpecialTime = false;
            progress.fillAmount = 0.25f;

            yield return new WaitForSeconds(0.1f);

            // TestCode. 스테이지 세팅
            //TestStageManager.SetStage(CurStage);
            BoardData loadedBoardData = BoardLoader.LoadBoard();
            //Manager.Stage.SetStage(boardManager.CurStage);
            progress.fillAmount = 0.5f;

            yield return new WaitForSeconds(0.1f);

            // BoardLoader가 레벨 데이터를 로드하면, 그 데이터를 실제 게임 보드에 적용합니다.


            // 씬에 있는 실제 BlockPlate 컴포넌트를 찾습니다.
            BlockPlate blockPlate = Object.FindObjectOfType<BlockPlate>();
            if (blockPlate == null)
            {
                Debug.LogError("BlockPlate를 찾을 수 없습니다!");
                yield break;
            }

            // 로드한 데이터로 BlockPlate를 설정하고 타일을 그립니다.
            blockPlate.BlockPlateArray = loadedBoardData.BlockPlateArray;
            Camera.main.orthographicSize = blockPlate.BlockPlateArray.GetLength(1) + 1;
            progress.fillAmount = 0.75f;

            yield return new WaitForSeconds(0.1f); // 타일이 그려질 시간을 줍니다.

            blockPlate.DrawTile();
            // 새로운 구조에 맞게 Spawner를 초기화합니다.
            Spawner.Initialize(this, loadedBoardData, blockPlate, BlockMover);
            Debug.Log($"보드 초기화 완료. 가로: {Spawner.GameBoardData.Width}, 세로: {Spawner.GameBoardData.Height}");
            _cameraController.SetStarted(true);
            //_tutorial.SetMaskImage(Spawner.GameBoardData.Width, Spawner.GameBoardData.Height);
            CanTouch = true;
            progress.fillAmount = 1f;
            IsReadyForStart = true;
            ChangeState(new ReadyState());
        }

        /// <summary>
        /// 스테이지 초기화 코루틴
        /// 테스트용 인자가 없는 오버로드 버전
        /// </summary>
        /// <returns></returns>
        public IEnumerator StageInit()
        {
            yield return new WaitForSeconds(0.1f);

            // TestCode. 스테이지 세팅
            TestStageManager.SetStage(CurStage - 1);
            BoardData loadedBoardData = BoardLoader.LoadBoard();
            //Manager.Stage.SetStage(boardManager.CurStage);

            yield return new WaitForSeconds(0.1f);

            // BoardLoader가 레벨 데이터를 로드하면, 그 데이터를 실제 게임 보드에 적용합니다.


            // 씬에 있는 실제 BlockPlate 컴포넌트를 찾습니다.
            BlockPlate blockPlate = Object.FindObjectOfType<BlockPlate>();
            if (blockPlate == null)
            {
                Debug.LogError("BlockPlate를 찾을 수 없습니다!");
                yield break;
            }

            // 로드한 데이터로 BlockPlate를 설정하고 타일을 그립니다.
            blockPlate.BlockPlateArray = loadedBoardData.BlockPlateArray;
            Camera.main.orthographicSize = blockPlate.BlockPlateArray.GetLength(1) + 1;

            yield return new WaitForSeconds(0.1f); // 타일이 그려질 시간을 줍니다.

            blockPlate.DrawTile();
            // 새로운 구조에 맞게 Spawner를 초기화합니다.
            Spawner.Initialize(this, loadedBoardData, blockPlate, BlockMover);
            Debug.Log($"보드 초기화 완료. 가로: {Spawner.GameBoardData.Width}, 세로: {Spawner.GameBoardData.Height}");
            CanTouch = true;
            ChangeState(new ReadyState());
        }

        private void Start()
        {
            Debug.Log("보드 매니저 시작");
            ChangeState(new InitializeState());
            // UpdateUI(Score);
        }

        private void Update()
        {
            if (_isClearRewardAnimationPlaying && Input.GetMouseButtonDown(0) && InGameManager.GetStageClear())
            {
                SkipClearReward();
            }

            if (CurrentState != null)
            {
                CurrentState.OnUpdate(this);
                //Debug.Log($"Current State: {CurrentState.GetType().Name}");
            }

        }



        public void ChangeState(IGameState newState)
        {
            if (CurrentState != null)
            {
                CurrentState.OnExit(this);
            }
            CurrentState = newState;
            CurrentState.OnEnter(this);
        }

        // 아이템 선택
        public void SelectItem(ItemType type)
        {
            SelectedItemType = type;
            IsItemSelected = true;
            if (type == ItemType.Scissors && _scissorUseUI != null)
                _scissorUseUI.SetActive(true);
            if (type == ItemType.Whisk && _whiskUseUI != null)
                _whiskUseUI.SetActive(true);
        }

        // 아이템 선택 해제
        public void ClearItemSelection()
        {
            IsItemSelected = false;
            if (_scissorUseUI != null)
                _scissorUseUI.SetActive(false);
            if (_whiskUseUI != null)
                _whiskUseUI.SetActive(false);
        }

        public static void SetTouch(bool canTouch)
        {
            CanTouch = canTouch;
        }


        #region 테스트 코드
        // public void UpdateUI(Block block, int x, int y)
        // {
        //     _blockInfo.text = $"Gem Type: {block.GemType}\nPosition: ({y}, {x})\nIsObstacle: {block.IsObstacle}\nIsNormal: {block.IsNormal}\nCanMove: {block.CanMove}\nObstacleBlock: {block is ObstacleBlock}";
        // }
        // 
        // public void UpdateUI(int score)
        // {
        //     //InGameManager.AddScore(score);
        //     Score += score;
        //     _scoreInfo.text = $"Score\n{Score}";
        // }
        // 
        // public void ResetUI()
        // {
        //     _blockInfo.text = string.Empty;
        // }
        // 
        // public void TestCode()
        // {
        //     Debug.Log("TestCode 실행");
        // }
        #endregion

        #region 효과
        public IEnumerator AnimateAndDestroyMatches(HashSet<Vector2Int> coordsToDestroy, List<(Vector2Int pos, int type, List<Vector2Int> matchCoords)> specialsToCreate, List<Vector2Int> swapPositions = null)
        {
            List<HashSet<Vector2Int>> matchGroups = new List<HashSet<Vector2Int>>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            foreach (Vector2Int coord in coordsToDestroy)
            {
                if (visited.Contains(coord)) continue;

                HashSet<Vector2Int> newGroup = new HashSet<Vector2Int>();
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                queue.Enqueue(coord);
                newGroup.Add(coord);
                visited.Add(coord);

                while (queue.Count > 0)
                {
                    Vector2Int curCoord = queue.Dequeue();
                    Vector2Int[] neighbors = new Vector2Int[]
                    {
                        new Vector2Int(curCoord.x, curCoord.y + 1), // 상
                        new Vector2Int(curCoord.x, curCoord.y - 1), // 하
                        new Vector2Int(curCoord.x - 1, curCoord.y), // 좌
                        new Vector2Int(curCoord.x + 1, curCoord.y)  // 우
                    };

                    foreach (Vector2Int neighbor in neighbors)
                    {
                        if (coordsToDestroy.Contains(neighbor) && !visited.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                            newGroup.Add(neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }

                matchGroups.Add(newGroup);
            }

            ShowScoreForMatches(matchGroups);

            // 애니메이션 총 시간
            float shrinkTime = _duration * 0.35f;
            float popTime = _duration * 0.65f;

            Vector3 originalScale = Vector3.one;
            Vector3 shrinkScale = Vector3.one * 0.2f;

            List<Block> blocksToAnimate = new List<Block>();
            List<Vector2Int> exceptions = new List<Vector2Int>();
            foreach (var coord in coordsToDestroy)
            {
                if (Spawner.GameBoardData.GetOverlayBlock(coord.x, coord.y) is Ice ice)
                {
                    exceptions.Add(coord);
                    ice.TakeDamage();
                    continue;
                }
                Block block = Spawner.GameBoardData.GetBlock(coord.x, coord.y);
                if (block != null && block.BlockInstance != null)
                {
                    blocksToAnimate.Add(block);
                }
            }

            IsWaitingForAnimation = true;
            // 1단계: 축소
            float timer = 0;
            while (timer < shrinkTime)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / shrinkTime);
                foreach (var block in blocksToAnimate)
                {
                    block.BlockInstance.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
                }
                yield return null;
            }

            // 2단계: 폭발 이펙트 생성
            SpriteRenderer[] renderers = new SpriteRenderer[blocksToAnimate.Count];

            if (specialsToCreate.Count > 0)
                Manager.Audio.PlaySFX("Block_MakeSpecial");
            else
                Manager.Audio.PlaySFX("Block_Match");

            foreach (var block in blocksToAnimate)
            {
                Vector3 blockPos = block.BlockInstance.transform.position;
                block.BlockInstance.GetComponent<PooledObject>().ReturnToPool();
                block.BlockInstance = _explosionEffectPool.GetObject().gameObject;
                block.BlockInstance.transform.position = blockPos;
                block.BlockInstance.transform.localScale = shrinkScale;
                renderers[blocksToAnimate.IndexOf(block)] = block.BlockInstance.GetComponent<SpriteRenderer>();
            }

            timer = 0;
            while (timer < popTime)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / popTime);
                foreach (var block in blocksToAnimate)
                {
                    float scaleValue = _explosionScaleCurve.Evaluate(progress);
                    float alphaValue = _explosionAlphaCurve.Evaluate(progress);
                    Color color = renderers[blocksToAnimate.IndexOf(block)].color;
                    color.a = alphaValue;
                    renderers[blocksToAnimate.IndexOf(block)].color = color;
                    block.BlockInstance.transform.localScale = Vector3.one * scaleValue;
                }
                yield return null;
            }

            IsWaitingForAnimation = false;

            // 3단계: 파괴 및 데이터 정리
            foreach (var coord in coordsToDestroy)
            {
                Block block = Spawner.GameBoardData.GetBlock(coord.x, coord.y);
                if (block != null)
                {
                    if (exceptions.Contains(coord))
                        continue;

                    if (block.BlockInstance != null)
                    {
                        if (block.BlockInstance.TryGetComponent<PooledObject>(out var pooledObj))
                        {
                            int score = MatchChecker.CalculateScore(block.Score);
                            InGameManager.AddScore(score);
                            InGameManager.AddIngredientSta(block.GemType);
                            pooledObj.ReturnToPool();
                        }
                        else
                        {
                            InGameManager.AddIngredientSta(block.GemType);
                            Destroy(block.BlockInstance);
                        }
                    }
                    Spawner.GameBoardData.BlockArray[coord.y, coord.x] = null; // 좌표를 사용하여 데이터 정리
                }
            }

            // 4단계: 특수 블록 생성
            if (specialsToCreate.Count > 0)
            {
                foreach (var special in specialsToCreate)
                {
                    Vector2Int spawnPos = CalculateSpecialBlockSpawnPosition(special.matchCoords, swapPositions);
                    Vector3 worldPos = BlockMover.GridToWorld(spawnPos, Spawner.GameBoardData.Width, Spawner.GameBoardData.Height);
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.1f);

                    foreach (var collider in colliders)
                    {
                        if (collider.TryGetComponent<PooledObject>(out var pooledObj))
                        {
                            pooledObj.ReturnToPool();
                        }
                        else
                        {
                            Destroy(collider.gameObject);
                        }
                    }

                    Spawner.SpawnBlock(spawnPos.x, spawnPos.y, (GemType)special.type, BlockMover);
                }
            }

            // 5단계: 다음 상태로 전환
            ChangeState(new RefillState());
        }

        private Vector2Int CalculateSpecialBlockSpawnPosition(List<Vector2Int> matchCoords, List<Vector2Int> swapPositions)
        {
            if (swapPositions != null)
            {
                foreach (var swapPos in swapPositions)
                {
                    if (matchCoords.Contains(swapPos))
                    {
                        return swapPos;
                    }
                }
            }

            Vector2Int bottomLeft = new Vector2Int(int.MaxValue, int.MaxValue);
            foreach (var coord in matchCoords)
            {
                if (Spawner.GameBoardData.GetOverlayBlock(coord.x, coord.y) is Ice)
                {
                    continue;
                }
                if (coord.y < bottomLeft.y)
                {
                    bottomLeft = coord;
                }
                else if (coord.y == bottomLeft.y)
                {
                    if (coord.x < bottomLeft.x)
                    {
                        bottomLeft.x = coord.x;
                    }
                }
            }
            return bottomLeft;
        }

        public void ShowHint(List<Vector2Int> hintPositions)
        {
            // 이미 실행 중인 힌트가 있다면, 새 힌트를 시작하기 전에 중지시킵니다.
            if (_hintCoroutine != null)
            {
                StopCoroutine(_hintCoroutine);
            }
            _hintCoroutine = StartCoroutine(HintCoroutine(hintPositions));
        }

        private IEnumerator HintCoroutine(List<Vector2Int> hintPositions)
        {
            _activeHintShaders.Clear();
            // 힌트 셰이더 활성화
            foreach (var pos in hintPositions)
            {
                Block block = Spawner.GameBoardData.GetBlock(pos.x, pos.y);
                if (block != null && block.BlockInstance != null)
                {
                    Material shader = block.BlockInstance.GetComponent<SpriteRenderer>().material;
                    if (shader != null)
                    {
                        _activeHintShaders.Add(shader);
                        shader.EnableKeyword("SHINE_ON");
                        shader.EnableKeyword("SHAKEUV_ON");
                        shader.SetFloat("_ShakeSpeed", 3.5f);
                        shader.SetFloat("_XMultiplier", 2f);
                        shader.SetFloat("_YMultiplier", 2f);
                    }
                }
            }

            // 힌트 애니메이션
            float timer = 0f;
            float alpha = 1f;
            while (timer < 3f) // 3초 지속
            {
                timer += Time.deltaTime;
                if (alpha <= 0) alpha = 1f;
                alpha -= Time.deltaTime;
                foreach (var shader in _activeHintShaders)
                {
                    shader.SetFloat("_ShineLocation", alpha);
                }
                yield return null;
            }

            HideHint();
            _hintCoroutine = null;
        }

        public void HideHint()
        {
            if (_hintCoroutine != null)
            {
                StopCoroutine(_hintCoroutine);
            }
            CleanupHintEffect();
        }

        public void CleanupHintEffect()
        {
            foreach (var shader in _activeHintShaders)
            {
                if (shader != null)
                {
                    shader.SetFloat("_ShineLocation", 1);
                    shader.DisableKeyword("SHINE_ON");
                    shader.DisableKeyword("SHAKEUV_ON");
                }
            }
        }

        private void ShowScoreForMatches(List<HashSet<Vector2Int>> matchGroups)
        {
            foreach (var group in matchGroups)
            {
                Vector2 centerPos = Vector2.zero;
                int matchCount = group.Count;
                int totalScore = 0;
                foreach (var coord in group)
                {
                    Block block = Spawner.GameBoardData.GetBlock(coord.x, coord.y);
                    if (block != null)
                    {
                        totalScore += MatchChecker.CalculateScore(block.Score);
                        centerPos += new Vector2(coord.x, coord.y);
                    }
                }
                centerPos /= matchCount;
                Vector3 worldPos = BlockMover.GridToWorld(new Vector2Int(Mathf.RoundToInt(centerPos.x), Mathf.RoundToInt(centerPos.y)), Spawner.GameBoardData.Width, Spawner.GameBoardData.Height);
                TMP_Text scorePopup = ScoreUIPool.GetObject().GetComponent<TMP_Text>();
                scorePopup.transform.position = worldPos;
                scorePopup.text = $"{totalScore}";
                scorePopup.GetComponentInParent<PooledObject>().ReturnToPool(1.0f);
            }
        }
        public void PlayMatchExplosion(Vector3 position)
        {
            StartCoroutine(PlayMatchExplosionRoutine(position));
        }

        private IEnumerator PlayMatchExplosionRoutine(Vector3 position)
        {
            if (_explosionEffectPool == null) yield break;

            var fxObj = _explosionEffectPool.GetObject().gameObject;
            var sr = fxObj.GetComponent<SpriteRenderer>();

            fxObj.transform.position = position;
            fxObj.transform.localScale = Vector3.one * 0.2f;

            float popTime = _duration * 0.65f;
            float timer = 0f;

            while (timer < popTime)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / popTime);

                float scale = _explosionScaleCurve != null ? _explosionScaleCurve.Evaluate(t) : Mathf.Lerp(0.2f, 1f, t);
                float alpha = _explosionAlphaCurve != null ? _explosionAlphaCurve.Evaluate(t) : (1f - t);

                if (sr != null)
                {
                    var c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
                fxObj.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            if (fxObj.TryGetComponent<PooledObject>(out var pooled))
                pooled.ReturnToPool();
            else
                Destroy(fxObj);
        }
        #endregion

        #region 보상 및 특수 블록
        private bool _isRewardRoutineDone = false;

        public IEnumerator RewardRoutine(List<GemType> rewards)
        {
            _isRewardRoutineDone = false;
            _currentRewards = new List<GemType>(rewards);
            _clearRewardCoroutine = StartCoroutine(ClearRewardAnimation(rewards));
            yield return new WaitUntil(() => _isRewardRoutineDone);
        }

        public IEnumerator ClearRewardAnimation(List<GemType> rewards)
        {
            _isClearRewardAnimationPlaying = true; // 스킵 감지를 위해 플래그설정

            float timer1 = 0f;
            float timer2 = 0f;
            float duration = 0.25f;
            List<Vector2Int> specialPositions = new List<Vector2Int>();
            Instance.HintManager.StopHintTimer();
            IsClearSpecialTime = true;
            CanTouch = false;
            Vector3 screenPos = _specialBlockStartPos.position;
            screenPos.z = 20f;
            Vector3 flyStartPos = Camera.main.ScreenToWorldPoint(screenPos);

            if (!IsRewardSkipped)
            {
                yield return new WaitForSeconds(0.25f);
            }

            // 기존 매치 및 특수 블록 처리
            while (IsBoardBusy)
            {
                if (IsRewardSkipped) break;

                yield return new WaitUntil(() => CurrentState is ReadyState && CanTouch && !SpecialBlockEffect.effectRunning);
                yield return new WaitForSeconds(0.1f);
            }

            if (!IsRewardSkipped)
            {
                yield return new WaitForSeconds(0.25f);
            }

            /*
            // 보상 블록 생성
            for (int i = 0; i < rewards.Count; i++)
            {
                if (IsRewardSkipped) break;

                Vector2Int randPos = new Vector2Int(Random.Range(0, Spawner.GameBoardData.Width), Random.Range(0, Spawner.GameBoardData.Height));
                timer1 = 0f;
                timer2 = 0f;

                while (true)
                {
                    if (IsRewardSkipped) break;

                    if (Spawner.GameBoardData.BlockPlate.BlockPlateArray[randPos.y, randPos.x] && Spawner.GameBoardData.GetBlock(randPos.x, randPos.y).IsNormal && !(Spawner.GameBoardData.GetOverlayBlock(randPos.x, randPos.y) is Ice))
                    {
                        break;
                    }
                    randPos = new Vector2Int(Random.Range(0, Spawner.GameBoardData.Width), Random.Range(0, Spawner.GameBoardData.Height));
                    yield return null;
                }

                Block oldBlock = Spawner.GameBoardData.GetBlock(randPos.x, randPos.y);

                // 기존 블록 축소
                while (timer1 < 0.15f)
                {
                    if (IsRewardSkipped) break;

                    timer1 += Time.deltaTime;
                    float scale = Mathf.Lerp(1f, 0f, timer1 / 0.15f);
                    if (oldBlock.BlockInstance != null)
                    {
                        oldBlock.BlockInstance.transform.localScale = Vector3.one * scale;
                    }
                    yield return null;
                }
                if (oldBlock.BlockInstance != null)
                    oldBlock.BlockInstance.GetComponent<PooledObject>().ReturnToPool();
                Spawner.GameBoardData.SetBlock(randPos.x, randPos.y, null);

                // 보상 블록 생성
                Debug.Log($"보상 블록 생성: {rewards[i]} at ({randPos.x}, {randPos.y})");
                Spawner.SpawnBlock(randPos.x, randPos.y, rewards[i], BlockMover);
                Block newBlock = Spawner.GameBoardData.GetBlock(randPos.x, randPos.y);
                if (newBlock != null && newBlock.BlockInstance != null)
                {
                    newBlock.BlockInstance.transform.localScale = Vector3.zero;
                }

                _currentRewards.Remove(rewards[i]);

                while (timer2 < duration)
                {
                    if (IsRewardSkipped) break;

                    timer2 += Time.deltaTime;
                    float progress = Mathf.Clamp01(timer2 / duration);
                    float scale = _specialBlockCreateCurve.Evaluate(progress);
                    if (newBlock != null && newBlock.BlockInstance != null)
                    {
                        newBlock.BlockInstance.transform.localScale = Vector3.one * scale;
                    }
                    yield return null;
                }

                specialPositions.Add(randPos);
            }
            */

            var mySequence = DOTween.Sequence();
            Vector2Int randPos = Vector2Int.zero;
            HashSet<Vector2Int> visitedPos = new HashSet<Vector2Int>();
            List<Vector2Int> randPositions = new List<Vector2Int>();
            float startTime = 0f;

            for (int i = 0; i < rewards.Count; i++)
            {
                int c = 0;
                while (true)
                {
                    if (IsRewardSkipped) break;

                    if (c > 100)
                    {
                        Debug.LogWarning("보상 블록 위치 선정 시도 100회 초과, 중단합니다.");
                        break;
                    }

                    randPos = new Vector2Int(Random.Range(0, Spawner.GameBoardData.Width), Random.Range(0, Spawner.GameBoardData.Height));

                    if (Spawner.GameBoardData.BlockPlate.BlockPlateArray[randPos.y, randPos.x] && Spawner.GameBoardData.GetBlock(randPos.x, randPos.y).IsNormal && !(Spawner.GameBoardData.GetOverlayBlock(randPos.x, randPos.y) is Ice) && !visitedPos.Contains(randPos))
                    {
                        visitedPos.Add(randPos);
                        randPositions.Add(randPos);
                        break;
                    }

                    c++;
                }
            }

            foreach (var reward in rewards)
            {
                if (IsRewardSkipped) break;

                // 2. 시작 좌표에서 보상 블록 오브젝트만 생성(데이터는 생성 X)
                var tempBlock = Spawner.SpawnBlockObject(reward);
                tempBlock.transform.position = flyStartPos;
                tempBlock.transform.localScale = Vector3.zero;

                GameObject capturedTempBlock = tempBlock;
                Vector2Int capturedRandPos = randPositions[rewards.IndexOf(reward)];
                GemType capturedReward = reward;
                // 3. 지정 위치로 날아가는 DOTween 애니메이션 실행
                Tween move = tempBlock.transform.DOMove(BlockMover.GridToWorld(capturedRandPos, Spawner.GameBoardData.Width, Spawner.GameBoardData.Height), 1f).SetEase(Ease.InOutQuad);
                Tween scale = tempBlock.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    // 4. 도착 시점에 데이터 생성 및 오브젝트 교체
                    // 기존 블록 제거
                    Block oldBlock = Spawner.GameBoardData.GetBlock(capturedRandPos.x, capturedRandPos.y);
                    if (oldBlock != null && oldBlock.BlockInstance != null)
                    {
                        oldBlock.BlockInstance.GetComponent<PooledObject>().ReturnToPool();
                    }
                    Spawner.GameBoardData.SetBlock(capturedRandPos.x, capturedRandPos.y, null);

                    Spawner.SpawnBlock(capturedRandPos.x, capturedRandPos.y, capturedReward, BlockMover);
                    // 블록 생성 후 임시 블록 제거(깔끔하게 보이게끔)
                    Destroy(capturedTempBlock);
                });

                mySequence.Insert(startTime, move);
                mySequence.Insert(startTime, scale);

                float delay = Random.Range(0.1f, 0.25f);
                startTime += delay;
            }

            yield return new WaitUntil(() => !mySequence.IsPlaying() || IsRewardSkipped);

            Debug.Log("스테이지 클리어 여부" + InGameManager.GetStageClear());

            if (!InGameManager.GetStageClear())
            {
                Debug.Log("아직 클리어 아님");
                // 클리어가 아니라면 특수 블록 사용은 건너뜀
                IsClearSpecialTime = false;
                CanTouch = true;

                _isClearRewardAnimationPlaying = false; // 코루틴 종료 전 플래그 해제
                _clearRewardCoroutine = null;
                yield break;
            }

            if (!IsRewardSkipped)
                yield return new WaitForSeconds(1f);

            // 특수 블록 처리
            while (IsAnySpecialBlockOnBoard() || IsBoardBusy)
            {
                if (IsRewardSkipped) break;

                if (IsAnySpecialBlockOnBoard())
                {
                    UseSpecialBlock(GetComponent<SpecialBlockEffect>());
                }
                yield return new WaitUntil(() => CurrentState is ReadyState &&
                CanTouch && !SpecialBlockEffect.effectRunning);
                yield return new WaitForSeconds(0.1f);
            }

            if (!IsRewardSkipped)
            {
                yield return new WaitForSeconds(0.5f);
            }

            IsClearSpecialTime = false;
            CanTouch = true;
            Debug.Log("특수 시간 종료");

            _isRewardRoutineDone = true;
            _isClearRewardAnimationPlaying = false; // 코루틴 정상 종료 시 플래그 해제
            _clearRewardCoroutine = null;
        }

        private void SkipClearReward()
        {
            if (!_isClearRewardAnimationPlaying) return;

            Debug.Log("보상 애니메이션 스킵 실행!");

            // 안전하게 플래그로 관리함
            // 1. 애니메이션 코루틴 즉시 중단
            // if (_clearRewardCoroutine != null)
            // {
            //     StopCoroutine(_clearRewardCoroutine);
            //     _clearRewardCoroutine = null;
            // }

            IsRewardSkipped = true;
            _isClearRewardAnimationPlaying = false;

            var sBE = GetComponent<SpecialBlockEffect>();

            if (sBE != null)
            {
                sBE.StopAllSpecialEffects();
            }



            // 2. SkipLogic으로 데이터 시뮬레이션 실행
            var skipLogic = new SkipLogic(Spawner.GameBoardData, Spawner.GetMaxDonutSpawnRange());
            SkipResult result = skipLogic.Simulate(_currentRewards);

            // 3. 시뮬레이션 결과 실제 게임에 반영
            InGameManager.AddScore(result.GainedScore);
            foreach (var ingredient in result.GainedIngredients)

                // 4. 게임 상태를 최종 상태로 정리
                IsClearSpecialTime = false;
            CanTouch = true;
            Debug.Log("스킵 완료. 최종 상태로 즉시 전환.");
            _isRewardRoutineDone = true;

            // 5. 스킵 후에는 보드가 엉망인 상태로 보이므로, 화면을 가리거나 다음 씬으로 바로 넘어가는 것이 좋습니다.
            // 여기서는 임시로 모든 블록 오브젝트를 비활성화합니다.
            foreach (var block in Spawner.GameBoardData.BlockArray)
            {
                if (block?.BlockInstance != null)
                {
                    block.BlockInstance.SetActive(false);
                }
            }
            foreach (var block in Spawner.GameBoardData.OverlayArray)
            {
                if (block?.BlockInstance != null)
                {
                    block.BlockInstance.SetActive(false);
                }
            }
        }

        private bool IsAnySpecialBlockOnBoard()
        {
            foreach (var block in Spawner.GameBoardData.BlockArray)
            {
                if (block != null && block.BlockInstance != null && block.GemType > GemType.Sugar && block.GemType < GemType.Dust)
                {
                    return true;
                }
            }
            return false;
        }

        public void UseSpecialBlock(SpecialBlockEffect effect)
        {
            Debug.Log("특수 블록 사용 진입");

            List<Vector2Int> positions = new List<Vector2Int>();
            foreach (var b in Spawner.GameBoardData.BlockArray)
            {
                if (b != null && b.BlockInstance != null && b.GemType > GemType.Sugar && b.GemType < GemType.Dust)
                {
                    Vector3 pos = BlockMover.WorldToArrayPosition(b.BlockInstance.transform.position, Spawner.GameBoardData.Width, Spawner.GameBoardData.Height);
                    positions.Add(new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)));
                }
            }

            foreach (var p in positions)
            {
                Block block = Spawner.GameBoardData.GetBlock(p.x, p.y);
                if (block != null)
                {
                    Debug.Log($"위치 {p.x}, {p.y}의 특수블록 {block.GemType} 사용");
                    effect.UseSpecial(p, block.GemType, Spawner.GameBoardData, new List<Vector2Int>());
                }
            }

            ChangeState(new RefillState());
        }


        #endregion

    }
}
