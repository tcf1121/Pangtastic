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
            BoardData boardData = boardManager.BoardLoader.LoadBoard(); //JWJ 수정함
            boardManager.Spawner.BlockPlate.BlockPlateArray = boardData.BlockPlateArray;
            boardManager.Spawner.BlockArray = boardData.BlockArray;
            boardManager.Spawner.BlockPlate.DrawTile();
            boardManager.Spawner.InitBlockArray();
            Debug.Log($"블록보드 배열 가로 길이: {boardManager.Spawner.BlockPlate.BlockPlateWidth}, 세로 길이: {boardManager.Spawner.BlockPlate.BlockPlateHeight}");
            Debug.Log($"블록 배열 가로 길이: {boardManager.Spawner.BlockArray.GetLength(1)}, 세로 길이: {boardManager.Spawner.BlockArray.GetLength(0)}");
            Debug.Log($"블록 배열 최상단의 값 : {boardManager.Spawner.BlockArray[boardManager.Spawner.BlockArray.GetLength(0) - 1, 0].BlockType}");
            boardManager.Spawner.DrawBlock();
            boardManager.ChangeState(new ReadyState());
        }
    }
}
