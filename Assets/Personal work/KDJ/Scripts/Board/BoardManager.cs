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
        [SerializeField] private bool isTest = false;
        [Header("애니메이션 설정")]
        [SerializeField] private float _duration = 0.3f;

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


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (isTest)
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
            progress.fillAmount = 0.25f;

            yield return new WaitForSeconds(0.1f);

            // TestCode. 스테이지 세팅
            TestStageManager.SetStage(CurStage);
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
            CanTouch = true;
            progress.fillAmount = 1f;
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
            TestStageManager.SetStage(CurStage);
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
            // 애니메이션 총 시간
            float shrinkTime = _duration * 0.7f;
            float popTime = _duration * 0.3f;

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

            // 1단계: 축소
            float timer = 0;
            while (timer < shrinkTime)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / shrinkTime);
                foreach (var block in blocksToAnimate)
                {
                    block.BlockInstance.transform.localScale = Vector3.Lerp(originalScale, shrinkScale, progress);
                }
                yield return null;
            }

            // 2단계: 원래 크기로 복귀
            timer = 0;
            while (timer < popTime)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / popTime);
                foreach (var block in blocksToAnimate)
                {
                    block.BlockInstance.transform.localScale = Vector3.Lerp(shrinkScale, originalScale, progress);
                }
                yield return null;
            }

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
                var creation = specialToCreate.Value;
                Spawner.SpawnBlock(specialSpawnPos.Value.x, specialSpawnPos.Value.y, (GemType)creation.type, BlockMover);
            }

            // 5단계: 다음 상태로 전환
            ChangeState(new RefillState());
        }
        #endregion
    }
}