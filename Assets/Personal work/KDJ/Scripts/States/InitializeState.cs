using System.Collections;
using UnityEngine;

namespace KDJ.States
{
    public class InitializeState : IGameState
    {
        public void OnEnter(BoardManager boardManager)
        {
            // 테스트 코드
            if (boardManager.IsTest)
                boardManager.StartCoroutine(boardManager.StageInit());

            InGameManager.SpawnCustomer();
        }

        public void OnUpdate(BoardManager boardManager) { }

        public void OnExit(BoardManager boardManager)
        {
            Debug.Log("초기화 상태 종료");
        }

        /*
        public IEnumerator StageInit(BoardManager boardManager)
        {
            Debug.Log("초기화 상태 진입");
            // TestCode. 스테이지 세팅
            boardManager.TestStageManager.SetStage(boardManager.CurStage);

            //Manager.Stage.SetStage(boardManager.CurStage);

            yield return new WaitForSeconds(1f);

            // BoardLoader가 레벨 데이터를 로드하면, 그 데이터를 실제 게임 보드에 적용합니다.
            BoardData loadedBoardData = boardManager.BoardLoader.LoadBoard();

            // 씬에 있는 실제 BlockPlate 컴포넌트를 찾습니다.
            BlockPlate blockPlate = Object.FindObjectOfType<BlockPlate>();
            if (blockPlate == null)
            {
                Debug.LogError("BlockPlate를 찾을 수 없습니다!");
                yield break;
            }

            // 로드한 데이터로 BlockPlate를 설정하고 타일을 그립니다.
            blockPlate.BlockPlateArray = loadedBoardData.BlockPlateArray;
            blockPlate.DrawTile();

            Camera.main.orthographicSize = blockPlate.BlockPlateArray.GetLength(1) + 1;

            yield return null; // 타일이 그려질 시간을 줍니다.

            // 새로운 구조에 맞게 Spawner를 초기화합니다.
            boardManager.Spawner.Initialize(boardManager, loadedBoardData, blockPlate, boardManager.BlockMover);

            Debug.Log($"보드 초기화 완료. 가로: {boardManager.Spawner.GameBoardData.Width}, 세로: {boardManager.Spawner.GameBoardData.Height}");


            BoardManager.SetTouch(true);

            boardManager.ChangeState(new ReadyState());
        }
        */
    }
}
