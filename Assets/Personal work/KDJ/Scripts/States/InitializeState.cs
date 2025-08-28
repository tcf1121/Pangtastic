using UnityEngine;

namespace KDJ.States
{
    public class InitializeState : IGameState
    {
        public void OnEnter(BoardManager boardManager)
        {
            StageInit(boardManager);
        }

        public void OnUpdate(BoardManager boardManager) { }

        public void OnExit(BoardManager boardManager)
        {
            Debug.Log("초기화 상태 종료");
        }

        public void StageInit(BoardManager boardManager)
        {
            Debug.Log("초기화 상태");
            BoardData boardData = boardManager.BoardLoader.ReadCSV(boardManager.CurStage);
            boardManager.Spawner.BlockPlate.BlockPlateArray = boardData.BlockPlateArray;
            boardManager.Spawner.BlockArray = boardData.BlockArray;

            boardManager.Spawner.InitBlockArray();
            boardManager.Spawner.DrawBlock();
            boardManager.ChangeState(new ReadyState());
        }
    }
}
