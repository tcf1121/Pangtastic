using KDJ.States;
using LHJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        [Header("애니메이션 설정")]
        [SerializeField] private float _duration = 0.3f;

        public IGameState CurrentState { get; private set; }
        public BlockSpawner Spawner { get; private set; }
        public BoardMatchChecker MatchChecker { get; private set; }
        public BlockMover BlockMover { get; private set; }
        public MatchCombo MatchCombo { get; set; }
        public BoardLoader BoardLoader { get; set; }
        public int Score { get; private set; } = 0;
        public int CurStage;
        public static bool CanTouch { get;  private set; }
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

            Debug.Log("보드 매니저 초기화");
            Spawner = FindObjectOfType<BlockSpawner>();
            MatchChecker = GetComponent<BoardMatchChecker>();
            BlockMover = GetComponent<BlockMover>();
            MatchCombo = GetComponent<MatchCombo>();
            BoardLoader = GetComponent<BoardLoader>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
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
            InGameManager.AddScore(score);
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
                        Destroy(block.BlockInstance);
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