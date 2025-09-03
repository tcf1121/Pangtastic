using UnityEngine;
using System.Collections;

namespace KDJ.States
{
    public class RefillState : IGameState
    {
        private Coroutine _refillProcessCoroutine;

        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("블록 재충전 상태");
            
            // 상태에 진입하면 전체 리필 프로세스를 한 번만 시작합니다.
            _refillProcessCoroutine = boardManager.Spawner.StartCoroutine(RefillAndChangeState(boardManager));
        }

        public void OnUpdate(BoardManager boardManager)
        {
            // 모든 로직은 OnEnter에서 시작된 코루틴이 처리하므로 OnUpdate는 비워둡니다.
        }

        public void OnExit(BoardManager boardManager)
        {
            Debug.Log("블록 재충전 상태 종료");
            if (_refillProcessCoroutine != null)
            {
                boardManager.Spawner.StopCoroutine(_refillProcessCoroutine);
                _refillProcessCoroutine = null;
            }
            boardManager.Spawner.DestroyedBlocks.Clear(); // 파괴된 블럭 데이터 초기화
            boardManager.BlockMover.StartPos = Vector2.zero; // 초기 시작 위치 설정
            boardManager.BlockMover.EndPos = Vector2.zero; // 초기 종료 위치 설정
        }

        private IEnumerator RefillAndChangeState(BoardManager boardManager)
        {
            // 배열에서 파괴된 블록 인스턴스를 정리
            boardManager.Spawner.CheckAndClearDestroyedBlocks();
            
            // 빈 셀의 개수만큼 대기열에 블록을 채움
            // boardManager.Spawner.SpawnBlock(); // 이 로직은 RefillBoardCoroutine으로 통합되었습니다.

            // Spawner에서 새로운 리필 코루틴을 실행하고 끝날 때까지 대기
            yield return boardManager.Spawner.StartCoroutine(boardManager.Spawner.RefillBoardCoroutine(boardManager.BlockMover));

            // 보드가 안정되고 리필된 후, 잠시 기다렸다가 상태를 변경
            yield return new WaitForSeconds(0.1f);
            boardManager.ChangeState(new ReadyState());
            _refillProcessCoroutine = null;
        }
    }
}
