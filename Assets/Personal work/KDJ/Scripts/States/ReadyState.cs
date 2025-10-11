using LHJ;
using System.Collections;
using UnityEngine;

namespace KDJ.States
{
    public class ReadyState : IGameState
    {
        private Coroutine _matchDelayCoroutine;
        private bool _isSwapping = false;
        private int count = 0;

        public void OnEnter(BoardManager boardManager)
        {
            if (boardManager.IsRewardSkipped) return;

            Debug.Log("입력 준비 상태");
            boardManager.InitialSwapPosition = null;
            boardManager.BlockMover.ResetCoordMoved();
            _isSwapping = false;

            if (Manager.User.GetStage() < 5 && !boardManager.IsTutorialPlayed)
            {
                // 튜토리얼 시작
                boardManager.Tutorial.PlayTutorial(SetStartPos());
                return; // 튜토리얼을 실행한 순간에는 힌트와 매치를 시작하지 않음
            }

            boardManager.HintManager.StartHintTimer();

            // 매칭되는 블럭이 있을 경우 매칭 상태로 전환
            if (boardManager.MatchChecker.AllBlockMatchCheck(boardManager))
            {
                if (_matchDelayCoroutine != null)
                {
                    boardManager.StopCoroutine(_matchDelayCoroutine);
                }
                _matchDelayCoroutine = boardManager.StartCoroutine(MatchDelayCoroutine(boardManager));
            }
            else
            {
                // 없으면 터치 가능
                BoardManager.SetTouch(true);
            }
        }

        public void OnUpdate(BoardManager boardManager)
        {
            // 사용자 입력이나 특정 조건 발생 시 힌트를 중단하고, 아닐 경우 힌트를 시작합니다.
            if (SpecialBlockEffect.effectRunning || boardManager.IsUseItem || (boardManager.IsItemSelected && !BoardManager.CanTouch) || boardManager.IsClearSpecialTime)
            {
                if (boardManager.IsUseItem) boardManager.IsUseItem = false;
                boardManager.HintManager.StopHintTimer();
            }
            else
            {
                boardManager.HintManager.StartHintTimer();
            }

            // 입력 중단 처리 부분
            if (!BoardManager.CanTouch || boardManager.IsClearSpecialTime) return;

            if (_isSwapping) return;

            BlockCheck(boardManager);
#if UNITY_EDITOR
            TestUserInput(boardManager);
#else
            UserInput(boardManager);
#endif
        }

        public void OnExit(BoardManager boardManager)
        {
            Debug.Log("입력 준비 상태 종료");


            if (_matchDelayCoroutine != null)
            {
                boardManager.StopCoroutine(_matchDelayCoroutine);
                _matchDelayCoroutine = null;
            }
        }

        private void UserInput(BoardManager boardManager)
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    Debug.Log("터치 시작");

                    Vector3 mousePosition = Input.mousePosition;
                    mousePosition.z = -Camera.main.transform.position.z;
                    boardManager.BlockMover.SetStartPos(Camera.main.ScreenToWorldPoint(mousePosition));
                    //TestBlockInfo(boardManager);
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Debug.Log("터치 이동");
                    Vector3 mousePosition = Input.mousePosition;
                    mousePosition.z = -Camera.main.transform.position.z;
                    boardManager.BlockMover.UpdateCoord(Camera.main.ScreenToWorldPoint(mousePosition));
                    if (boardManager.BlockMover.IsCoordMoved && !_isSwapping)
                    {
                        boardManager.StartCoroutine(SwapAndChangeState(boardManager));
                        _isSwapping = true;
                    }
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    Debug.Log("터치 종료");
                    // boardManager.ResetUI();
                    if (!boardManager.BlockMover.IsCoordMoved)
                    {
                        // 뗏을때 아무것도 안 움직였다면 좌표 초기화
                        boardManager.BlockMover.ResetPos();
                    }
                }
            }

        }

        private void TestUserInput(BoardManager boardManager)
        {
            if (Input.GetMouseButtonDown(0) && !_isSwapping)
            {
                Debug.Log("마우스 클릭 시작");

                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = -Camera.main.transform.position.z;
                boardManager.BlockMover.SetStartPos(Camera.main.ScreenToWorldPoint(mousePosition));
                //TestBlockInfo(boardManager);
            }
            else if (Input.GetMouseButton(0) && !_isSwapping)
            {
                Debug.Log("마우스 클릭 이동");
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = -Camera.main.transform.position.z;
                boardManager.BlockMover.UpdateCoord(Camera.main.ScreenToWorldPoint(mousePosition));
                if (boardManager.BlockMover.IsCoordMoved && !_isSwapping)
                {
                    boardManager.StartCoroutine(SwapAndChangeState(boardManager));
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("마우스 클릭 종료");
                // boardManager.ResetUI();
                if (!boardManager.BlockMover.IsCoordMoved)
                {
                    // 뗏을때 아무것도 안 움직였다면 좌표 초기화
                    boardManager.BlockMover.ResetPos();
                }
            }
        }

        private Vector3 SetStartPos()
        {
            switch (Manager.User.GetStage())
            {
                case 0:
                    return new Vector3(3f, 0.5f, 0f);
                case 1:
                    return new Vector3(2f, -0.5f, 0f);
                case 2:
                    return new Vector3(2f, -0.5f, 0f);
                case 3:
                    return new Vector3(3f, 0.5f, 0f);
                case 4:
                    return new Vector3(0.5f, 1.5f, 0f);
                default:
                    return Vector3.zero;
            }
        }

        private IEnumerator SwapAndChangeState(BoardManager boardManager)
        {
            Debug.Log("스왑 시도 및 상태 전환");
            _isSwapping = true;
            bool isHintPosSwap = false;

            if (boardManager.CurHintPositions.Count > 0)
            {
                foreach (var pos in boardManager.CurHintPositions)
                {
                    if (boardManager.BlockMover.ValidateAndSetPositions(boardManager) && (pos == boardManager.BlockMover.StartBlockPos || pos == boardManager.BlockMover.EndBlockPos))
                    {
                        isHintPosSwap = true;
                        break;
                    }
                }
            }

            if (isHintPosSwap)
            {
                boardManager.HintManager.StopHintTimer();
                boardManager.CurHintPositions.Clear();
            }

            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = -Camera.main.transform.position.z;
            boardManager.BlockMover.EndPos = Camera.main.ScreenToWorldPoint(mousePosition);

            bool swapSuccess = false;
            yield return boardManager.StartCoroutine(
                boardManager.BlockMover.TrySwap(boardManager, result => swapSuccess = result)
            );

            if (swapSuccess)
            {
                boardManager.InitialSwapPosition = boardManager.BlockMover.EndBlockPos;
                boardManager.ChangeState(new MatchingState());
            }
            else
            {
                _isSwapping = false;
            }
        }

        private void BlockCheck(BoardManager boardManager)
        {
            if (boardManager.Spawner.HasEmptyBlocks())
            {
                if (boardManager.Spawner.CanBlockMoveInArray())
                {
                    boardManager.ChangeState(new RefillState());
                }
            }
            else
            {
                if (!boardManager.MatchChecker.AllBlockMatchPossibilityCheck(boardManager, out int possibleCount) && boardManager.firstCheck < 1)
                {
                    // 최초 실행시에만
                    boardManager.Spawner.Shuffle(boardManager, true);
                    boardManager.firstCheck++;
                    boardManager.ChangeState(new ReadyState());
                }
                else if (!boardManager.MatchChecker.AllBlockMatchPossibilityCheck(boardManager, out int asd))
                {
                    boardManager.Spawner.Shuffle(boardManager, false);
                    boardManager.ChangeState(new ReadyState());
                }
            }
        }

        // private void TestBlockInfo(BoardManager boardManager)
        // {
        //     Vector3 mousePosition = Input.mousePosition;
        //     mousePosition.z = -Camera.main.transform.position.z;
        //     Vector2 targetPos = Camera.main.ScreenToWorldPoint(mousePosition);
        // 
        //     Vector2Int gridPos = boardManager.BlockMover.WorldToGrid(targetPos, boardManager.Spawner.GameBoardData.Width, boardManager.Spawner.GameBoardData.Height);
        // 
        //     if (gridPos.x < 0 || gridPos.y < 0 || gridPos.x >= boardManager.Spawner.GameBoardData.Width || gridPos.y >= boardManager.Spawner.GameBoardData.Height)
        //     {
        //         return;
        //     }
        // 
        //     if (!boardManager.Spawner.GameBoardData.BlockPlate.BlockPlateArray[gridPos.y, gridPos.x]) return;
        // 
        //     Block block = boardManager.Spawner.GameBoardData.GetBlock(gridPos.x, gridPos.y);
        //     if (block != null)
        //     {
        //         boardManager.UpdateUI(block, gridPos.x, gridPos.y);
        //     }
        // }

        private IEnumerator MatchDelayCoroutine(BoardManager boardManager)
        {
            yield return new WaitForSeconds(0.1f);
            boardManager.ChangeState(new MatchingState());
            _matchDelayCoroutine = null;
        }
    }
}