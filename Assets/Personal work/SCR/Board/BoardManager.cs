using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace SCR_B
{
    public class BoardManager : MonoBehaviour
    {
        public BlockSpawner Spawner { get; private set; }
        public BoardMatchChecker MatchChecker { get; private set; }
        public BlockMover BlockMover { get; private set; }
        public MatchCombo MatchCombo { get; set; }
        public BoardLoader BoardLoader { get; set; }
        public BoardData BoardData { get { return _boardData; } }
        private BoardData _boardData;
        public int Width { get { return _width; } }
        private int _width;
        public int Height { get { return _height; } }
        private int _height;
        public bool CanTouch;
        private static BoardManager instace;

        private void Awake()
        {
            instace = this;
            Spawner = GetComponent<BlockSpawner>();
            MatchChecker = GetComponent<BoardMatchChecker>();
            BlockMover = GetComponent<BlockMover>();
            MatchCombo = GetComponent<MatchCombo>();
            BoardLoader = GetComponent<BoardLoader>();
            CanTouch = false;
        }

        private void Start()
        {
            StartCoroutine(StageInit());
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                instace.StartCoroutine(instace.Spawner.MoveDownAll());
            }
        }

        public IEnumerator StageInit()
        {
            Debug.Log("초기화 상태");
            _boardData = BoardLoader.LoadBoard();
            yield return new WaitForSeconds(1f);
            SetBoardData();
            Spawner._boardData = _boardData;
            Spawner.DrawFirstPuzzle();
            _width = _boardData.GetWidth();
            _height = _boardData.GetHeight();
            Debug.Log($"블록보드 배열 가로 길이: {_boardData.GetWidth()}, 세로 길이: {_boardData.GetHeight()}");
            Debug.Log($"블록 배열 가로 길이: {_boardData.BlockArray.GetLength(1)}, 세로 길이: {_boardData.BlockArray.GetLength(0)}");
            CanTouch = true;
        }

        public void SetBoardData()
        {
            Spawner.SetBoardData(_boardData);
            MatchChecker.SetBoardData(_boardData);
            BlockMover.SetBoardData(_boardData);
        }

        public static void Move()
        {
            if (!instace.CanTouch) return;
            instace.BlockMover.Move(PuzzleBoard.GetStartPos(), PuzzleBoard.GetEndPos());
        }

        public static void UseItem()
        {
            if (!instace.CanTouch) return;
            instace.BlockMover.Move(PuzzleBoard.GetStartPos(), PuzzleBoard.GetEndPos());
        }

        public static IEnumerator HandleTurn()
        {
            while (true)
            {
                yield return instace.StartCoroutine(instace.MatchChecker.CheckAll());
                if (instace.MatchChecker.MatchDatas.Count != 0)
                {
                    yield return instace.StartCoroutine(instace.MatchChecker.DamageAll());
                    yield return instace.StartCoroutine(instace.Spawner.MoveDownAll());
                }
                else
                {
                    break;
                }
            }
        }

        public static Vector3 GetWorldPos(int x, int y)
        {

            return new Vector3(x + instace._boardData.ZeroPos.x, y + +instace._boardData.ZeroPos.y, 0);
        }

        public static BoardManager GetBoard()
        {
            return instace;
        }
    }
}