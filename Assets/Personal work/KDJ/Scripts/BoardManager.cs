using KDJ.States;
using LHJ;
using SCR_B;
using TMPro;
using UnityEngine;

namespace KDJ
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text _blockInfo;
        [SerializeField] private TMP_Text _scoreInfo;
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
        #endregion
    }
}