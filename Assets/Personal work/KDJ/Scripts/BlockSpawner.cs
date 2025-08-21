using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KDJ
{
    [System.Serializable]
    public class Block
    {
        public int BlockType { get; set; }
        public GameObject BlockInstance { get; set; } = null;
    }

    public class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _blockPrefabs = new List<GameObject>();
        [SerializeField] private BlockPlate _blockPlate;

        private Block[,] _blockArray;
        private Queue<Block>[] _blockWaitingQueue;

        public int BlankBlockCount = 0;

        #region 초기화
        /// <summary>
        /// 블럭 배열 초기화
        /// </summary>
        public void InitBlockArray()
        {
            // 추가 생성될 블럭을 담아놓을 큐를 생성. 배열은 미리 블럭을 꺼내 로드할 행 하나만 추가
            _blockArray = new Block[_blockPlate.BlockPlateHeight + 1, _blockPlate.BlockPlateWidth];
            _blockWaitingQueue = new Queue<Block>[_blockPlate.BlockPlateWidth];

            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockArray.GetLength(0); y++)
                {
                    if (y < _blockPlate.BlockPlateHeight)
                    {
                        if (_blockPlate.BlockPlateArray[y, x]) _blockArray[y, x] = new Block { BlockType = Random.Range(1, 6) };
                    }
                    else
                    {
                        _blockArray[y, x] = new Block { BlockType = Random.Range(1, 6) }; // 마지막 행은 빈 블럭으로 초기화
                    }
                }

                _blockWaitingQueue[x] = new Queue<Block>();
            }

            // 테스트 코드
            _blockArray[3, 2].BlockType = 6;
            _blockArray[3, 3].BlockType = 6;
            _blockArray[3, 4].BlockType = 6;
        }

        /// <summary>
        /// 초기 블럭 생성
        /// </summary>
        public void DrawBlock()
        {
            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockArray.GetLength(0); y++)
                {
                    if (_blockPlate.BlockPlateWidth % 2 == 0)
                    {
                        if (y < _blockPlate.BlockPlateHeight)
                        {
                            if (_blockPlate.BlockPlateArray[y, x] && _blockArray[y, x].BlockType != 0)
                            {
                                Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                                GameObject blockPrefab = GetBlockTile(_blockArray[y, x].BlockType);
                                _blockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                            }
                        }
                        else
                        {
                            if (_blockArray[y, x].BlockType != 0)
                            {
                                Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                                GameObject blockPrefab = GetBlockTile(_blockArray[y, x].BlockType);
                                _blockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                            }
                        }

                    }
                    else
                    {
                        if (y < _blockPlate.BlockPlateHeight)
                        {
                            if (_blockPlate.BlockPlateArray[y, x] && _blockArray[y, x].BlockType != 0)
                            {
                                Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2, y - _blockPlate.BlockPlateHeight / 2, 0);
                                GameObject blockPrefab = GetBlockTile(_blockArray[y, x].BlockType);
                                _blockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                            }
                        }
                        else
                        {
                            if (_blockArray[y, x].BlockType != 0)
                            {
                                Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2, y - _blockPlate.BlockPlateHeight / 2, 0);
                                GameObject blockPrefab = GetBlockTile(_blockArray[y, x].BlockType);
                                _blockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                            }
                        }

                    }
                }
            }
        }
        #endregion

        #region 블럭 관리
        /// <summary>
        /// 블럭 배열 정렬
        /// </summary>
        public void SortBlockArray()
        {
            Debug.Log("블럭 배열 정렬");
            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
                {
                    if (_blockArray[y, x] != null && _blockArray[y, x].BlockType == 6) continue; // 6번 블럭은 건너뜀

                    int yPos = 0;
                    int xPos = 0;

                    if (_blockArray[y, x] == null && _blockPlate.BlockPlateArray[y, x])
                    {
                        if (_blockArray[y + 1, x] != null)
                        {
                            if (_blockArray[y + 1, x].BlockType == 6)
                            {
                                if (_blockPlate.BlockPlateArray[y + 1, x - 1] && _blockArray[y + 1, x - 1] != null && _blockArray[y + 1, x - 1].BlockType != 6)
                                {
                                    // 오른쪽 위에 블럭이 있는 경우
                                    _blockArray[y, x] = _blockArray[y + 1, x - 1];
                                    _blockArray[y + 1, x - 1] = null;
                                    _blockArray[y, x].BlockInstance.transform.position = new Vector3(
                                        _blockArray[y, x].BlockInstance.transform.position.x + 1,
                                        _blockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                }
                                else if (_blockPlate.BlockPlateArray[y + 1, x + 1] && _blockArray[y + 1, x + 1] != null && _blockArray[y + 1, x + 1].BlockType != 6)
                                {
                                    // 오른쪽 위에 없고 왼쪽 위에 있는 경우
                                    _blockArray[y, x] = _blockArray[y + 1, x + 1];
                                    _blockArray[y + 1, x + 1] = null;
                                    _blockArray[y, x].BlockInstance.transform.position = new Vector3(
                                        _blockArray[y, x].BlockInstance.transform.position.x - 1,
                                        _blockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                }
                                else
                                {
                                    // 둘다 없는 경우 건너뜀
                                    continue;
                                }
                            }
                            else
                            {
                                _blockArray[y, x] = _blockArray[y + 1, x];
                                _blockArray[y + 1, x] = null;
                                _blockArray[y, x].BlockInstance.transform.position = new Vector3(
                                    _blockArray[y, x].BlockInstance.transform.position.x,
                                    _blockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                            }
                        }
                    }
                    else if (!_blockPlate.BlockPlateArray[y, x])
                    {
                        if (_blockArray[y + 1, x] != null)
                        {
                            if (_blockArray[y - 1, x] == null)
                            {
                                _blockArray[y - 1, x] = _blockArray[y + 1, x];
                                _blockArray[y + 1, x] = null;
                                _blockArray[y - 1, x].BlockInstance.transform.position = new Vector3(
                                    _blockArray[y - 1, x].BlockInstance.transform.position.x,
                                    _blockArray[y - 1, x].BlockInstance.transform.position.y - 2, 0);
                            }
                        }
                    }
                    else if (x > 0 && _blockArray[y, x - 1] == null && _blockArray[y + 1, x].BlockType != 6)
                    {
                        _blockArray[y, x - 1] = _blockArray[y + 1, x];
                        _blockArray[y + 1, x] = null;
                        _blockArray[y, x - 1].BlockInstance.transform.position = new Vector3(
                            _blockArray[y, x - 1].BlockInstance.transform.position.x - 1,
                            _blockArray[y, x - 1].BlockInstance.transform.position.y - 1, 0);
                    }


                }

                if (_blockWaitingQueue[x].Count == 0) _blockWaitingQueue[x].Enqueue(new Block { BlockType = Random.Range(1, 6) });

                // y축 생성이 끝나고 대기열 [_blockPlate.BlockPlateHeight, x]에 블럭이 없는 경우 큐에서 꺼내와서 할당
                if (_blockArray[_blockPlate.BlockPlateHeight, x] == null && _blockWaitingQueue[x].Count > 0)
                {
                    Vector3 position;

                    if (_blockPlate.BlockPlateWidth % 2 == 0)
                    {
                        position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, _blockPlate.BlockPlateHeight - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                    }
                    else
                    {
                        position = new Vector3(x - _blockPlate.BlockPlateWidth / 2, _blockPlate.BlockPlateHeight - _blockPlate.BlockPlateHeight / 2, 0);
                    }

                    _blockArray[_blockPlate.BlockPlateHeight, x] = _blockWaitingQueue[x].Dequeue();
                    _blockArray[_blockPlate.BlockPlateHeight, x].BlockInstance = Instantiate(GetBlockTile(_blockArray[_blockPlate.BlockPlateHeight, x].BlockType), position, Quaternion.identity);
                }
            }
            
        }

        /// <summary>
        /// 빈칸이 존재하는 경우 블럭 생성
        /// </summary>
        public void SpawnBlock()
        {
            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
                {
                    if (_blockPlate.BlockPlateArray[y, x] && _blockArray[y, x] == null)
                    {
                        // 빈칸이 있는 경우 해당 열 큐에 블럭을 생성해서 추가
                        _blockWaitingQueue[x].Enqueue(new Block { BlockType = Random.Range(1, 6) });
                    }
                }
            }
        }

        /// <summary>
        /// 블럭 체크. 시각적 오브젝트가 파괴된 경우에도 해당 칸을 빈칸으로 설정
        /// </summary>
        public void CheckBlockArray()
        {
            Debug.Log("블럭 배열 상태 확인 시작");
            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
                {
                    if (_blockPlate.BlockPlateArray[y, x])
                    {
                        if (_blockArray[y, x] == null || _blockArray[y, x].BlockInstance == null)
                        {
                            if (_blockArray[y, x].BlockInstance != null) Destroy(_blockArray[y, x].BlockInstance);
                            _blockArray[y, x] = null;
                        }
                    }
                }
            }
        }
        #endregion

        #region 테스트 코드
        /// <summary>
        /// 테스트 코드. 하단 블럭을 ㅗ자 형태로 파괴
        /// </summary>
        /// <returns></returns>
        public bool TestDeleteBlock()
        {
            bool deleted = false;
            if (_blockArray[0, 1] != null && _blockArray[0, 1].BlockInstance != null)
            {
                Destroy(_blockArray[0, 1].BlockInstance);
                _blockArray[0, 1] = null;
                deleted = true;
            }
            if (_blockArray[0, 2] != null && _blockArray[0, 2].BlockInstance != null)
            {
                Destroy(_blockArray[0, 2].BlockInstance);
                _blockArray[0, 2] = null;
                deleted = true;
            }
            if (_blockArray[0, 3] != null && _blockArray[0, 3].BlockInstance != null)
            {
                Destroy(_blockArray[0, 3].BlockInstance);
                _blockArray[0, 3] = null;
                deleted = true;
            }
            if (_blockArray[1, 2] != null && _blockArray[1, 2].BlockInstance != null)
            {
                Destroy(_blockArray[1, 2].BlockInstance);
                _blockArray[1, 2] = null;
                deleted = true;
            }
            return deleted;
        }
        #endregion

        #region 블럭 데이터 관리
        /// <summary>
        /// 입력받은 블럭 번호에 해당하는 블럭 프리팹을 반환
        /// </summary>
        /// <param name="blockNum"></param>
        /// <returns></returns>
        private GameObject GetBlockTile(int blockNum)
        {
            switch (blockNum)
            {
                case 1: return _blockPrefabs[0];
                case 2: return _blockPrefabs[1];
                case 3: return _blockPrefabs[2];
                case 4: return _blockPrefabs[3];
                case 5: return _blockPrefabs[4];
                case 6: return _blockPrefabs[5];
                default: return null;
            }
        }

        /// <summary>
        /// 블록 데이터 배열에 빈 블록이 있는지 확인
        /// </summary>
        /// <returns></returns>
        public bool HasEmptyBlocks()
        {
            BlankBlockCount = 0; // 빈 블럭 카운트 초기화

            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
                {
                    if (_blockPlate.BlockPlateArray[y, x] && _blockArray[y, x] == null)
                    {
                        BlankBlockCount++;
                    }
                }
            }
            return BlankBlockCount > 0;
        }

        /// <summary>
        /// 주어진 y좌표 위에 있는 빈 공간의 개수를 반환
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetAboveEmptySpaceCount(int x, int y)
        {
            int count = 0;
            for (int i = y; i < _blockPlate.BlockPlateHeight; i++)
            {
                if (_blockArray[i, x] == null && _blockPlate.BlockPlateArray[i, x])
                {
                    count++;
                }
                else
                {
                    break; // 빈 공간이 아니면 중단
                }
            }
            return count;
        }
        #endregion
    }
}