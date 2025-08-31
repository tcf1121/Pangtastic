using System.Collections.Generic;
using UnityEngine;
using SCR;
using System.Collections;


namespace SCR_B
{
    public class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject spawnRoot;

        public List<GemType> DestroyBlockData { get; private set; } = new List<GemType>();
        public BoardData _boardData;
        public int BlankBlockCount = 0;

        [SerializeField] private List<Vector2Int> _emptyBlock = new();
        [SerializeField] private Dictionary<Vector2Int, Vector2Int> _canMoveBlock = new();
        [SerializeField] private Dictionary<Vector2Int, MatchType> _specialBlock = new();

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
                for (int y = 0; y < _boardData.GetHeight() + 1; y++)
                {
                    if (y < _boardData.GetHeight()) SpawnBlock(x, y);
                    else SpawnDonut(x, y);
                }
            }
            while (BoardManager.FistIsMatch())
                ShuffleBlockArray();
            InGameManager.SpawnCustomer();
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
            Vector2Int pos;
            CheckBlocks();
            AddSpecial();

            bool cycle = true;

            while (cycle)
            {

                for (int x = 0; x < _boardData.GetWidth(); x++)
                {
                    for (int y = _boardData.GetHeight(); y > 0; y--)
                    {
                        pos = new Vector2Int(x, y);
                        if (_emptyBlock.Contains(pos))
                        {
                            if (_boardData.RespawnPos.Contains(pos)) SpawnDonut(x, y);
                        }
                        else if (_canMoveBlock.ContainsKey(pos))
                        {
                            MoveDown(pos, _canMoveBlock[pos]);
                        }

                    }
                }
                yield return new WaitForSeconds(0.1f);
                cycle = CheckThreeTypes(0);
                if (!cycle)
                {
                    cycle = CheckThreeTypes(1);
                }
                if (!cycle)
                {
                    cycle = CheckThreeTypes(2);
                }
            }

            yield return null;
        }

        public void MatchSpecial(Vector2Int pos, MatchType specialType)
        {
            if (!_specialBlock.ContainsKey(pos))
                _specialBlock.Add(pos, specialType);
        }

        private void AddSpecial()
        {
            Vector2Int pos;
            for (int x = 0; x < _boardData.GetWidth(); x++)
            {
                for (int y = _boardData.GetHeight(); y > 0; y--)
                {
                    pos = new Vector2Int(x, y);
                    if (_emptyBlock.Contains(pos))
                    {
                        if (_specialBlock.ContainsKey(pos))
                        {
                            //Debug.Log(_specialBlock[pos]);
                            SpawnBlock(x, y, MatchToGem(_specialBlock[pos]));
                        }
                    }
                }
            }
            _specialBlock.Clear();
            CheckBlocks();
        }

        public void MoveDown(Vector2Int curPos, Vector2Int targetPos)
        {
            if (_boardData.BlockArray[curPos.y, curPos.x].GemType <= GemType.Oven ||
                _boardData.BlockArray[curPos.y, curPos.x].GemType == GemType.Egg ||
                _boardData.BlockArray[curPos.y, curPos.x].GemType == GemType.Coin)
            {
                _boardData.BlockArray[targetPos.y, targetPos.x] = _boardData.BlockArray[curPos.y, curPos.x].Clone();
                StartCoroutine(MovePosCor(_boardData.BlockArray[targetPos.y, targetPos.x].BlockInstance.transform,
                 GetWorldPos(targetPos.x, targetPos.y), 0.3f));
                _boardData.BlockArray[targetPos.y, targetPos.x].Pos = targetPos;
                _boardData.BlockArray[curPos.y, curPos.x] = null;
            }
        }

        private IEnumerator MovePosCor(Transform gameObject, Vector3 targetPos, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                if (gameObject != null)
                    gameObject.position = Vector3.Lerp(gameObject.position, targetPos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            if (gameObject != null)
                gameObject.position = targetPos;
        }

        private bool CheckThreeTypes(int type)
        {
            bool cycle = true;
            CheckBlocks(type);
            if (_canMoveBlock.Count == 0) cycle = false;
            foreach (var Rpos in _boardData.RespawnPos)
            {
                if (_emptyBlock.Contains(Rpos)) cycle = true;
            }
            return cycle;
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
            GameObject blockPrefab = InGameManager.GetPrefab(gemType);
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
            GameObject blockPrefab = InGameManager.GetPrefab(gemType);
            if (blockPrefab != null)
            {
                GameObject blockInstance = Instantiate(blockPrefab, spawnRoot.transform);

                blockInstance.transform.position = GetWorldPos(x, y);

                if (_boardData.BlockArray[y, x].GemType == GemType.Ice)
                    _boardData.BlockArray[y, x].BlockInstance.GetComponent<GemPrefab>().SetSprite((_boardData.BlockArray[y, x] as Ice).GetIceImage());
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
        public void CheckBlocks(int num = 0)
        {
            _emptyBlock.Clear();
            _canMoveBlock.Clear();
            CheckAllEmptyBlocks();
            CheckAllCanMove(num);
        }

        /// <summary>
        /// 블록 배열에 빈 배열이 있는지 확인 후 리스트에 추가
        /// </summary>
        /// <returns></returns>
        public void CheckAllEmptyBlocks()
        {
            for (int x = 0; x < _boardData.GetWidth(); x++)
            {
                for (int y = 0; y < _boardData.GetHeight() + 1; y++)
                {
                    if (_boardData.RespawnPos.Contains(new Vector2Int(x, y)))
                    {
                        if (_boardData.BlockArray[y, x] == null)
                            _emptyBlock.Add(new Vector2Int(x, y));
                    }
                    else if (CheckEmptyBlock(x, y))
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
        public void CheckAllCanMove(int num = 0)
        {
            Vector2Int pos;
            for (int x = 0; x < _boardData.GetWidth(); x++)
            {
                for (int y = _boardData.GetHeight(); y > 0; y--)
                {
                    pos = new Vector2Int(x, y);
                    if (!_emptyBlock.Contains(pos) && _boardData.BlockArray[y, x].CanMove)
                    {
                        if (num == 0)
                        {
                            if (CheckMoveDownPos(pos) != pos)
                            {
                                if (!_canMoveBlock.ContainsKey(pos))
                                {
                                    _canMoveBlock.Add(pos, CheckMoveDownPos(pos));
                                }
                            }
                        }
                        else if (num == 1)
                        {
                            if (CheckMoveLeftDownPos(pos) != pos)
                            {
                                if (!_canMoveBlock.ContainsKey(pos))
                                {
                                    _canMoveBlock.Add(pos, CheckMoveLeftDownPos(pos));
                                }
                            }
                        }
                        else if (num == 2)
                        {
                            if (CheckMoveRightDownPos(pos) != pos)
                            {
                                if (!_canMoveBlock.ContainsKey(pos))
                                {
                                    _canMoveBlock.Add(pos, CheckMoveRightDownPos(pos));
                                }
                            }
                        }
                    }

                }
            }
        }

        // 수직으로 밑을 확인 해서 그 위치를 가져옴
        public Vector2Int CheckMoveDownPos(Vector2Int pos)
        {
            int x = pos.x;
            int y = pos.y;
            Vector2Int downPos = pos;
            Vector2Int returnPos = pos;
            while (y > 0)
            {
                y--;
                downPos += Vector2Int.down;
                if (_boardData.BlockPlateArray[y, x])
                {
                    if (_emptyBlock.Contains(downPos))
                        returnPos = downPos;
                    else break;
                }
            }
            return returnPos;
        }

        // 왼쪽 밑을 확인 해서 그 위치를 가져옴
        public Vector2Int CheckMoveLeftDownPos(Vector2Int pos)
        {
            int x = pos.x;
            int y = pos.y;
            Vector2Int targetPos = pos + Vector2Int.left + Vector2Int.down;
            Vector2Int returnPos = pos;
            if (y - 1 > 0 && x - 1 > 0)
                if (_boardData.BlockPlateArray[y - 1, x - 1])
                {
                    if (_emptyBlock.Contains(targetPos))
                        returnPos = targetPos;
                }
            return returnPos;
        }

        // 오른쪽 밑을 확인 해서 그 위치를 가져옴
        public Vector2Int CheckMoveRightDownPos(Vector2Int pos)
        {
            int x = pos.x;
            int y = pos.y;
            Vector2Int targetPos = pos + Vector2Int.right + Vector2Int.down;
            Vector2Int returnPos = pos;
            if (y - 1 > 0 && x + 1 < _boardData.GetWidth())
                if (_boardData.BlockPlateArray[y - 1, x + 1])
                {
                    if (_emptyBlock.Contains(targetPos))
                        returnPos = targetPos;
                }
            return returnPos;
        }

        private GemType MatchToGem(MatchType matchType)
        {
            if (matchType == MatchType.Roller_h) return GemType.Roller_h;
            else if (matchType == MatchType.Roller_v) return GemType.Roller_v;
            else if (matchType == MatchType.Milk) return GemType.Milk;
            else if (matchType == MatchType.DonutBox) return GemType.DonutBox;
            else if (matchType == MatchType.Oven) return GemType.Oven;
            else return GemType.Empty;
        }

        #endregion
    }
}