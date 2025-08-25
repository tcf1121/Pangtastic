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
        public int Score { get; private set; } = 0;

        private void Awake()
        {
            Spawner = FindObjectOfType<BlockSpawner>();
            MatchChecker = GetComponent<BoardMatchChecker>();
            BlockMover = GetComponent<BlockMover>();
            MatchCombo = GetComponent<MatchCombo>();
        }

        private void Start()
        {
            ChangeState(new States.InitializeState());
            UpdateUI(Score);
        }

        private void Update()
        {
            if (CurrentState != null)
            {
                CurrentState.OnUpdate(this);
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
        public void UpdateUI(Block block)
        {
            _blockInfo.text = $"Block Type: {block.BlockType}\nGem Type: {block.GemType}";
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