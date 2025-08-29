using System.Collections.Generic;
using UnityEngine;
using SCR;
using System.Collections;
using System.Xml.Serialization;


namespace SCR_B
{
    public class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private PrefabList blockList;
        [SerializeField] private GameObject spawnRoot;

        private Queue<Block>[] _blockWaitingQueue;

        [Header("생성될 블럭 타입 범위 설정")]
        [Range(1, 6)]
        [SerializeField] private int _spawnRangeMin;
        [Range(1, 6)]
        [SerializeField] private int _spawnRangeMax;
        public int SpawnRangeMin
        {
            get { return _spawnRangeMin; }
            set { _spawnRangeMin = value; }
        }
        public int SpawnRangeMax
        {
            get { return _spawnRangeMax; }
            set { _spawnRangeMax = value; }
        }

        public List<GemType> DestroyBlockData { get; private set; } = new List<GemType>();
        public BoardData _boardData;
        public int BlankBlockCount = 0;

        private List<Vector2Int> _emptyBlock = new();
        private List<Vector2Int> _canMoveBlock = new();

        #region 초기화

        public void SetBoardData(BoardData boardData)
        {
            _boardData = boardData;
        }

        /// <summary>
        /// 초기 블럭 생성
        /// </summary>
        public void DrawFirstPuzzle()
        {
            for (int x = 0; x < _boardData.GetWidth(); x++)
            {
                for (int y = 0; y < _boardData.GetHeight(); y++)
                {
                    SpawnBlock(x, y);
                }
            }
        }
        #endregion

        #region 블럭 관리
        private Vector3 GetWorldPos(int x, int y)
        {

            return new Vector3(x + _boardData.ZeroPos.x, y + +_boardData.ZeroPos.y, 0);
        }

        /// <summary>
        /// 위에서 아래로 순회하며 블록을 이동
        /// </summary>
        public IEnumerator MoveDownAll()
        {
            CheckBlocks();
            Vector2Int pos;

            while (true)
            {
                for (int x = 0; x < _boardData.GetWidth(); x++)
                {
                    for (int y = _boardData.GetHeight() - 1; y > 0; y--)
                    {
                        pos = new Vector2Int(x, y);
                        if (_emptyBlock.Contains(pos) && _boardData.RespawnPos.Contains(pos + Vector2Int.up))
                        {
                            SpawnDonut(x, y);
                        }
                        else if (_canMoveBlock.Contains(pos) && _emptyBlock.Contains(pos + Vector2Int.down))
                            MoveDown(x, y);
                    }
                }
                yield return new WaitForSeconds(0.1f);

                CheckBlocks();
                if (_emptyBlock.Count == 0) break;
            }

            yield return null;
        }

        public void MoveDown(int x, int y)
        {
            if (_boardData.BlockArray[y, x].GemType <= GemType.Sugar ||
                _boardData.BlockArray[y, x].GemType == GemType.Egg ||
                _boardData.BlockArray[y, x].GemType == GemType.Coin)
            {
                _boardData.BlockArray[y - 1, x] = _boardData.BlockArray[y, x].Clone();
                _boardData.BlockArray[y - 1, x].BlockInstance.transform.position = GetWorldPos(x, y - 1);
                _boardData.BlockArray[y - 1, x].Pos = new Vector2Int(x, y - 1);
                _boardData.BlockArray[y, x] = null;
            }
        }

        /// <summary>
        /// 블럭 체크. 시각적 오브젝트가 파괴된 경우에도 해당 칸을 빈칸으로 설정
        /// </summary>
        public void CheckBlockInArray()
        {
            for (int x = 0; x < _boardData.GetWidth(); x++)
            {
                for (int y = 0; y < _boardData.GetHeight(); y++)
                {
                    CheckBlock(x, y);
                }
            }
        }

        public void CheckBlock(int x, int y)
        {
            if (_boardData.BlockArray[y, x] != null && _boardData.BlockArray[y, x].BlockInstance == null)
            {
                _boardData.BlockArray[y, x] = null;
            }
        }

        /// <summary>
        /// 지정 위치에 입력받은 블록을 생성
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SpawnBlock(int x, int y)
        {
            GemType gemType = _boardData.BlockArray[y, x].GemType;
            GameObject blockPrefab = blockList.GetPrefab(gemType);
            if (blockPrefab != null)
            {
                GameObject blockInstance = Instantiate(blockPrefab, spawnRoot.transform);

                blockInstance.transform.position = GetWorldPos(x, y);

                if (_boardData.BlockArray[y, x].GemType == GemType.Ice)
                    _boardData.BlockArray[y, x].BlockInstance.GetComponent<SpriteRenderer>().sprite = (_boardData.BlockArray[y, x] as Ice).GetIceImage();
                if (gemType == GemType.Dust || gemType == GemType.Syrup)
                {
                    _boardData.OverlayArray[y, x].BlockInstance = blockInstance;
                }
                else
                {
                    _boardData.BlockArray[y, x].BlockInstance = blockInstance;
                }
            }
        }

        /// <summary>
        /// 지정 위치에 입력받은 블록을 생성 잼타입을 받을 수 있음
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="gemType"></param>
        public void SpawnBlock(int x, int y, GemType gemType)
        {
            _boardData.SetArray(x, y, gemType);
            GameObject blockPrefab = blockList.GetPrefab(gemType);
            if (blockPrefab != null)
            {
                GameObject blockInstance = Instantiate(blockPrefab, spawnRoot.transform);

                blockInstance.transform.position = GetWorldPos(x, y);

                if (_boardData.BlockArray[y, x].GemType == GemType.Ice)
                    _boardData.BlockArray[y, x].BlockInstance.GetComponent<SpriteRenderer>().sprite = (_boardData.BlockArray[y, x] as Ice).GetIceImage();
                if (gemType == GemType.Dust || gemType == GemType.Syrup)
                {
                    _boardData.OverlayArray[y, x].BlockInstance = blockInstance;
                }
                else
                {
                    _boardData.BlockArray[y, x].BlockInstance = blockInstance;
                }
            }
        }

        /// <summary>
        /// 랜덤 도넛 생성(정해진 도넛 중)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SpawnDonut(int x, int y)
        {
            SpawnBlock(x, y, _boardData.RespawnDount());
        }
        /// <summary>
        /// 지정 위치 블록 삭제
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DestoryBlock(int x, int y)
        {
            if (_boardData.BlockArray[y, x].BlockInstance != null)
            {
                Destroy(_boardData.BlockArray[y, x].BlockInstance);
                _boardData.DelArray(x, y);
            }

        }

        /// <summary>
        /// 랜덤 위치 특수 블럭 생성
        /// </summary>
        /// <param name="gemType"></param>
        public void RandomPosSpawnSpecialBlock(GemType gemType)
        {
            int randX;
            int randY;
            do
            {
                randX = Random.Range(0, _boardData.GetWidth());
                randY = Random.Range(0, _boardData.GetHeight());
            } while (!_boardData.BlockPlateArray[randY, randX] &&
                _boardData.BlockArray[randY, randX].GemType > GemType.Sugar);

            DestoryBlock(randX, randY);

            SpawnBlock(randX, randY, gemType);
        }

        /// <summary>
        /// 보드 판 섞음
        /// </summary>
        public void ShuffleBlockArray()
        {
            // 블록 배열을 섞는 로직 구현
            // 각 배열 인덱스에 접근하여 일반 블럭이라면 기존 데이터를 제거하고 랜덤 일반 블럭을 생성
            for (int x = 0; x < _boardData.GetWidth(); x++)
            {
                for (int y = 0; y < _boardData.GetHeight(); y++)
                {
                    if (_boardData.BlockPlateArray[y, x])
                    {
                        if (_boardData.BlockArray[y, x] != null &&
                        _boardData.BlockArray[y, x].BlockInstance != null && !_boardData.BlockArray[y, x].IsObstacle)
                        {
                            DestoryBlock(x, y);
                            SpawnDonut(x, y);
                        }
                    }
                }
            }
        }

        #endregion

        #region 블럭 데이터 관리

        /// <summary>
        /// 블록 배열을 확인해서 빈 블록과 움직일 수 있는 블록을 확인
        /// </summary>
        public void CheckBlocks()
        {
            _emptyBlock.Clear();
            _canMoveBlock.Clear();
            CheckAllEmptyBlocks();
            CheckAllCanMove();
        }

        /// <summary>
        /// 블록 배열에 빈 배열이 있는지 확인 후 리스트에 추가
        /// </summary>
        /// <returns></returns>
        public void CheckAllEmptyBlocks()
        {
            for (int x = 0; x < _boardData.GetWidth(); x++)
            {
                for (int y = 0; y < _boardData.GetHeight(); y++)
                {
                    if (CheckEmptyBlock(x, y))
                    {
                        _emptyBlock.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        public bool CheckEmptyBlock(int x, int y)
        {
            if (_boardData.BlockPlateArray[y, x] &&
                    _boardData.BlockArray[y, x] == null)
                return true;
            else return false;
        }

        /// <summary>
        /// 블록 배열에 움직일 수 있는지 확인 후 리스트에 추가
        /// </summary>
        public void CheckAllCanMove()
        {
            for (int x = 0; x < _boardData.GetWidth(); x++)
            {
                for (int y = _boardData.GetHeight() - 1; y > 0; y--)
                {
                    if (CheckCanMove(x, y))
                    {
                        if (!_canMoveBlock.Contains(new Vector2Int(x, y)))
                            _canMoveBlock.Add(new Vector2Int(x, y));
                    }

                }
            }
        }

        public bool CheckCanMove(int x, int y)
        {
            if (_boardData.BlockPlateArray[y, x] &&
                    _boardData.BlockArray[y, x] != null &&
                     _boardData.BlockArray[y, x].CanMove)
                return true;
            else return false;
        }

        #endregion
    }
}