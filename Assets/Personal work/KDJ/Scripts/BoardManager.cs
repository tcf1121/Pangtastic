using TMPro;
using UnityEngine;

namespace KDJ
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text _blockInfo;
        public IGameState CurrentState { get; private set; }
        public BlockSpawner Spawner { get; private set; }
        public BoardMatchChecker MatchChecker { get; private set; }
        public BlockMover BlockMover { get; private set; }
        

        private void Awake()
        {
            Spawner = FindObjectOfType<BlockSpawner>();
            MatchChecker = GetComponent<BoardMatchChecker>();
            BlockMover = GetComponent<BlockMover>();
        }

        private void Start()
        {
            ChangeState(new States.InitializeState());
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

        public void ResetUI()
        {
            _blockInfo.text = string.Empty;
        }
        #endregion
    }
}