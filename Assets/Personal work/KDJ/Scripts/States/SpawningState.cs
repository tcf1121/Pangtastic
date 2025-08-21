using UnityEngine;
using System.Collections;

namespace KDJ.States
{
    public class SpawningState : IGameState
    {
        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("블록 생성 상태");
            //boardManager.Spawner.SpawnBlock();
            boardManager.ChangeState(new RefillState());
        }

        public void OnUpdate(BoardManager boardManager)
        {
            // 로직 변경. 낙하 후 생성이 아닌 미리 생성 시켜놓고 함께 자연스럽게 낙하하도록
        }

        public void OnExit(BoardManager boardManager)
        {
            Debug.Log("블록 생성 상태 종료");
        }
    }
}
