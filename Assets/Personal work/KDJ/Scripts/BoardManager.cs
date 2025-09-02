using KDJ.States;
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
        public float MatchDelay;

        private void Awake()
        {
            Debug.Log("보드 매니저 초기화");
            Spawner = FindObjectOfType<BlockSpawner>();
            MatchChecker = GetComponent<BoardMatchChecker>();
            BlockMover = GetComponent<BlockMover>();
            MatchCombo = GetComponent<MatchCombo>();
            BoardLoader = GetComponent<BoardLoader>();
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

        #region 테스트 코드
        public void UpdateUI(Block block, int x, int y)
        {
            _blockInfo.text = $"Gem Type: {block.GemType}\nPosition: ({y}, {x})";
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
        #endregion
    }
}