using KDJ.States;
using LHJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField] private TMP_Text _blockInfo;
        [SerializeField] private TMP_Text _scoreInfo;
        public bool IsTest = false;
        [SerializeField] private CameraController _cameraController;
        [Header("애니메이션 설정")]
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private ObjectPool _explosionEffectPool;
        [SerializeField] private AnimationCurve _explosionScaleCurve;
        [SerializeField] private AnimationCurve _explosionAlphaCurve;

        public IGameState CurrentState { get; private set; }
        public BlockSpawner Spawner { get; private set; }
        public BoardMatchChecker MatchChecker { get; private set; }
        public BlockMover BlockMover { get; private set; }
        public MatchCombo MatchCombo { get; set; }
        public BoardLoader BoardLoader { get; set; }
        public TestStageManager TestStageManager { get; set; }
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
            TestStageManager = FindObjectOfType<TestStageManager>();
            CanTouch = false;
            IsReadyForStart = false;
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
            UpdateUI(Score);
        }

        private void Update()
        {
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
        }

        // 아이템 선택 해제
        public void ClearItemSelection()
        {
            IsItemSelected = false;
        }

        public static void SetTouch(bool canTouch)
        {
            CanTouch = canTouch;
        }


        #region 테스트 코드
        public void UpdateUI(Block block, int x, int y)
        {
            _blockInfo.text = $"Gem Type: {block.GemType}\nPosition: ({y}, {x})\nIsObstacle: {block.IsObstacle}\nIsNormal: {block.IsNormal}\nCanMove: {block.CanMove}\nObstacleBlock: {block is ObstacleBlock}";
        }

        public void UpdateUI(int score)
        {
            //InGameManager.AddScore(score);
            Score += score;
            _scoreInfo.text = $"Score\n{Score}";
        }

        public void ResetUI()
        {
            _blockInfo.text = string.Empty;
        }

        public void TestCode()
        {
            Debug.Log("TestCode 실행");
        }

        public IEnumerator AnimateAndDestroyMatches(HashSet<Vector2Int> coordsToDestroy, (Vector2Int pos, int type)? specialToCreate, Vector2Int? specialSpawnPos, Vector2Int? swapPosition = null)
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
            foreach (var coord in coordsToDestroy)
            {
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
                    if (block.BlockInstance != null)
                    {
                        // Destroy(block.BlockInstance);
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
            if (specialToCreate.HasValue && specialSpawnPos.HasValue)
            {
                // 특수 블록이 생성될 위치에 리턴하지 못한 블록이 있다면 리턴
                Vector3 worldPos = BlockMover.GridToWorld(specialSpawnPos.Value, Spawner.GameBoardData.Width, Spawner.GameBoardData.Height);
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

                var creation = specialToCreate.Value;
                Spawner.SpawnBlock(specialSpawnPos.Value.x, specialSpawnPos.Value.y, (GemType)creation.type, BlockMover);
            }

            // 5단계: 다음 상태로 전환
            ChangeState(new RefillState());
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
        #endregion
    }
}