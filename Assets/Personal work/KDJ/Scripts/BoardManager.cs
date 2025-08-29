using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V12;
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

        private void Awake()
        {
            Spawner = FindObjectOfType<BlockSpawner>();
            MatchChecker = GetComponent<BoardMatchChecker>();
            BlockMover = GetComponent<BlockMover>();
            MatchCombo = GetComponent<MatchCombo>();
            BoardLoader = GetComponent<BoardLoader>();
        }

        private void Start()
        {
            ChangeState(new States.InitializeState() as IGameState);
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
            _blockInfo.text = $"Block Type: {block.BlockType}\nGem Type: {block.GemType}\nPosition: ({y}, {x})";
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