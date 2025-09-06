using SCR;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
        private static BoardManager instance;
        private Vector2Int? firstPos;
        private Vector2Int? secondPos;
        [SerializeField] int _leftTurn;
        [SerializeField] bool isTest;

        private void Awake()
        {
            if (isTest)
            {
                instance = this;
                Spawner = GetComponent<BlockSpawner>();
                MatchChecker = GetComponent<BoardMatchChecker>();
                BlockMover = GetComponent<BlockMover>();
                MatchCombo = GetComponent<MatchCombo>();
                BoardLoader = GetComponent<BoardLoader>();
                CanTouch = false;
            }

        }

        private void Start()
        {
            if (isTest)
                StartCoroutine(StageInit());
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                instance.StartCoroutine(instance.Spawner.MoveDownAll());
            }
        }

        public IEnumerator StageInit()
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("초기화 상태");
            _boardData = BoardLoader.LoadBoard();
            yield return new WaitForSeconds(0.1f);

            SetBoardData();
            yield return new WaitForSeconds(0.1f);
            Spawner._boardData = _boardData;
            _width = _boardData.GetWidth();
            _height = _boardData.GetHeight();
            Debug.Log($"블록보드 배열 가로 길이: {_boardData.GetWidth()}, 세로 길이: {_boardData.GetHeight()}");
            Debug.Log($"블록 배열 가로 길이: {_boardData.BlockArray.GetLength(1)}, 세로 길이: {_boardData.BlockArray.GetLength(0)}");
            CanTouch = true;
            Spawner.DrawFirstPuzzle();
        }

        public IEnumerator StageInit(Image progress)
        {
            instance = this;
            Spawner = GetComponent<BlockSpawner>();
            MatchChecker = GetComponent<BoardMatchChecker>();
            BlockMover = GetComponent<BlockMover>();
            MatchCombo = GetComponent<MatchCombo>();
            BoardLoader = GetComponent<BoardLoader>();
            CanTouch = false;
            progress.fillAmount = 0.25f;
            yield return new WaitForSeconds(0.1f);
            Debug.Log("초기화 상태");
            _boardData = BoardLoader.LoadBoard();
            progress.fillAmount = 0.5f;
            yield return new WaitForSeconds(0.1f);
            SetBoardData();
            progress.fillAmount = 0.75f;
            yield return new WaitForSeconds(0.1f);
            Spawner._boardData = _boardData;
            _width = _boardData.GetWidth();
            _height = _boardData.GetHeight();
            Debug.Log($"블록보드 배열 가로 길이: {_boardData.GetWidth()}, 세로 길이: {_boardData.GetHeight()}");
            Debug.Log($"블록 배열 가로 길이: {_boardData.BlockArray.GetLength(1)}, 세로 길이: {_boardData.BlockArray.GetLength(0)}");
            CanTouch = true;
            Spawner.DrawFirstPuzzle();
            progress.fillAmount = 1f;
        }

        public void SetBoardData()
        {
            Spawner.SetBoardData(_boardData);
            MatchChecker.SetBoardData(_boardData);
            BlockMover.SetBoardData(_boardData);
        }

        public static void SetTouch(bool value)
        {
            instance.CanTouch = value;
        }

        public static void Move()
        {
            if (!instance.CanTouch) return;
            Debug.Log($"{PuzzleBoard.GetStartPos()}, {PuzzleBoard.GetEndPos()}");
            SetFirstPos(PuzzleBoard.GetStartPos());
            SetSecondPos(PuzzleBoard.GetEndPos());
            instance.StartCoroutine(instance.BlockMover.Move((Vector2Int)instance.firstPos, (Vector2Int)instance.secondPos));
        }

        public static void SetCanTouch(bool value)
        {
            instance.CanTouch = value;
        }

        public static void Select()
        {
            if (!instance.CanTouch) return;
            if (GetFirstPos() == null)
            {
                SetFirstPos(PuzzleBoard.GetStartPos());
                instance.BlockMover.Select((Vector2Int)instance.firstPos, true);
            }
            else SetSecondPos(PuzzleBoard.GetStartPos());

            if (GetFirstPos() == GetSecondPos())
            {
                instance.BlockMover.Select((Vector2Int)instance.firstPos, false);
                instance.BlockMover.DobleSelect((Vector2Int)instance.firstPos);
            }
            else
            {
                instance.BlockMover.Select((Vector2Int)instance.firstPos, false);
                SetFirstPos(PuzzleBoard.GetStartPos());
                instance.BlockMover.Select((Vector2Int)instance.firstPos, true);
                instance.secondPos = null;
            }

        }

        public static void UseItem()
        {
            if (!instance.CanTouch) return;
            SetFirstPos(PuzzleBoard.GetStartPos());
            SetSecondPos(PuzzleBoard.GetEndPos());
            //instance.BlockMover.Move((Vector2Int)instance.firstPos, (Vector2Int)instance.secondPos);
        }

        public static IEnumerator HandleTurn()
        {
            instance.CanTouch = false;
            instance._leftTurn--;
            while (true)
            {
                yield return instance.StartCoroutine(instance.MatchChecker.CheckAll());
                if (instance.MatchChecker.MatchDatas.Count != 0 ||
                instance.MatchChecker.SpecialDamage.Count != 0)
                {
                    yield return instance.StartCoroutine(instance.MatchChecker.DamageAll());
                    yield return instance.StartCoroutine(instance.Spawner.MoveDownAll());
                }
                else
                {
                    break;
                }
            }
            instance.Spawner.CheckPossibleMatch();
            instance.firstPos = null;
            instance.secondPos = null;
            instance.CanTouch = true;
        }

        public static Vector3 GetWorldPos(int x, int y)
        {

            return new Vector3(x + instance._boardData.ZeroPos.x, y + +instance._boardData.ZeroPos.y, 0);
        }

        public static BoardManager GetBoard()
        {
            return instance;
        }

        public static void MatchSpecial(Vector2Int pos, MatchType specialType)
        {
            instance.Spawner.MatchSpecial(pos, specialType);
        }

        public static void UseSpecial(Vector2Int pos, GemType specialType)
        {
            instance.MatchChecker.UseSpecial(pos, specialType);
        }

        public static IEnumerator UseTwoSpecial(Vector2Int firstPos, Vector2Int secondPos, GemType specialType, GemType specialType2)
        {
            yield return instance.StartCoroutine(instance.MatchChecker.UseTwoSpecial(firstPos, secondPos, specialType, specialType2));
        }

        public static bool FistIsMatch()
        {
            return instance.MatchChecker.IsMatch();
        }

        public static bool HasPossibleMatch()
        {
            return instance.MatchChecker.HasPossibleMatch();
        }

        public static void SetFirstPos(Vector2Int pos)
        {
            instance.firstPos = pos - instance._boardData.ZeroPos;
        }

        public static void SetSecondPos(Vector2Int pos)
        {
            instance.secondPos = pos - instance._boardData.ZeroPos;
        }

        public static Vector2Int? GetFirstPos()
        {
            return instance.firstPos;
        }

        public static Vector2Int? GetSecondPos()
        {
            return instance.secondPos;
        }

    }
}