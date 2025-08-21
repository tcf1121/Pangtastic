using System.Collections;
using UnityEngine;

namespace KDJ
{
    [System.Serializable]
    public class Block
    {
        public int BlockType { get; set; }
        public GameObject BlockInstance { get; set; }
    }

    public class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _block1;
        [SerializeField] private GameObject _block2;
        [SerializeField] private GameObject _block3;
        [SerializeField] private GameObject _block4;
        [SerializeField] private GameObject _block5;
        [SerializeField] private BlockPlate _blockPlate;

        private Block[,] _blockArray;

        public int BlankBlockCount = 0;
        #region 초기화
        /// <summary>
        /// 블럭 배열 초기화
        /// </summary>
        public void InitBlockArray()
        {
            // 블럭 배열은 높이를 2배로 설정
            _blockArray = new Block[_blockPlate.BlockPlateHeight * 2, _blockPlate.BlockPlateWidth];

            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
                {
                    if (_blockPlate.BlockPlateArray[y, x])
                    {
                        _blockArray[y, x] = new Block { BlockType = Random.Range(1, 6) };
                    }
                    else
                    {
                        _blockArray[y, x] = new Block { BlockType = 0 }; // 빈 블록
                    }

                    _blockArray[y, x].BlockInstance = null;
                }
            }
        }

        /// <summary>
        /// 초기 블럭 생성
        /// </summary>
        public void DrawBlock()
        {
            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
                {
                    if (_blockPlate.BlockPlateWidth % 2 == 0)
                    {
                        if (_blockArray[y, x].BlockType != 0)
                        {
                            Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                            GameObject blockPrefab = GetBlockTile(_blockArray[y, x].BlockType);
                            _blockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                        }
                        else
                        {
                            _blockArray[y, x].BlockInstance = null;
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
                        else
                        {
                            _blockArray[y, x].BlockInstance = null;
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
                for (int y = 0; y < _blockArray.GetLength(0) - 1; y++)
                {
                    if (_blockArray[y, x] == null)
                    {
                        if (_blockArray[y + 1, x] != null)
                        {
                            _blockArray[y, x] = _blockArray[y + 1, x];
                            _blockArray[y + 1, x] = null;
                            _blockArray[y, x].BlockInstance.transform.position = new Vector3(
                                _blockArray[y, x].BlockInstance.transform.position.x,
                                _blockArray[y, x].BlockInstance.transform.position.y - 1,
                                0
                                );
                        }
                    }
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
                // x축이 변동되면 초기화
                int spawnCount = 0;

                for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
                {
                    if (_blockPlate.BlockPlateArray[y, x] && _blockArray[y, x] == null)
                    {
                        Vector3 position;
                        Vector3 spawnPosition;

                        // 블럭을 블럭판 위에 생성 시킨 다음 자신의 위치까지 내려오도록 구성
                        if (_blockPlate.BlockPlateWidth % 2 == 0)
                        {
                            // 목표 위치
                            position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                            // 생성 위치 = x축은 동일하고 y축은 현재 위치에서 배열 세로축만큼 더한 값 + 보정
                            spawnPosition = new Vector3(position.x, position.y + _blockPlate.BlockPlateHeight - y + spawnCount, 0);
                        }
                        else
                        {
                            position = new Vector3(x - _blockPlate.BlockPlateWidth / 2, y - _blockPlate.BlockPlateHeight / 2, 0);
                            spawnPosition = new Vector3(position.x, position.y + _blockPlate.BlockPlateHeight - y + spawnCount, 0);
                        }
                        // 생성 후 배열 위에 배치
                        int spawnIndex = _blockPlate.BlockPlateHeight + spawnCount;
                        if (_blockArray[spawnIndex, x] == null)
                        {
                            _blockArray[spawnIndex, x] = new Block();
                        }
                        Debug.Log(spawnIndex);
                        _blockArray[spawnIndex, x].BlockType = Random.Range(1, 6);
                        GameObject gemPrefab = GetBlockTile(_blockArray[spawnIndex, x].BlockType);
                        _blockArray[spawnIndex, x].BlockInstance = Instantiate(gemPrefab, spawnPosition, Quaternion.identity);

                        // 생성했다면 spawnCount 증가
                        spawnCount++;
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
                    if (_blockArray[y, x] == null || _blockArray[y, x].BlockInstance == null)
                    {
                        if (_blockArray[y, x].BlockInstance != null) Destroy(_blockArray[y, x].BlockInstance);
                        _blockArray[y, x] = null;
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
                case 1: return _block1;
                case 2: return _block2;
                case 3: return _block3;
                case 4: return _block4;
                case 5: return _block5;
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