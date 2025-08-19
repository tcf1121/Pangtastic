using UnityEngine;

namespace KDJ
{
    public class GameManager : MonoBehaviour
    {
        public IGameState CurrentState { get; private set; }
        public BlockSpawner Spawner { get; private set; }

        private void Awake()
        {
            Spawner = FindObjectOfType<BlockSpawner>();
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
    }
}
