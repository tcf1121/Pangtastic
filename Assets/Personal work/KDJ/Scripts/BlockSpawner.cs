using System.Collections;
using UnityEngine;

namespace KDJ
{
    public class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _block1;
        [SerializeField] private GameObject _block2;
        [SerializeField] private GameObject _block3;
        [SerializeField] private GameObject _block4;
        [SerializeField] private GameObject _block5;
        [SerializeField] private BlockPlate _blockPlate;

        private GameObject[,] _blockArray;
        private int[,] _blockDataArray;

        public int BlankBlockCount = 0;

        /// <summary>
        /// 블럭 배열 초기화
        /// </summary>
        public void InitBlockArray()
        {
            _blockDataArray = new int[_blockPlate.BlockPlateWidth, _blockPlate.BlockPlateHeight];
            _blockArray = new GameObject[_blockPlate.BlockPlateWidth, _blockPlate.BlockPlateHeight];

            for (int x = 0; x < _blockDataArray.GetLength(0); x++)
            {
                for (int y = 0; y < _blockDataArray.GetLength(1); y++)
                {
                    if (_blockPlate.BlockPlateArray[y, x])
                    {
                        _blockDataArray[y, x] = Random.Range(1, 6);
                    }
                    else
                    {
                        _blockDataArray[y, x] = 0; // 빈 블록
                    }

                    _blockArray[y, x] = null;
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
                        if (_blockDataArray[y, x] != 0)
                        {
                            Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                            GameObject blockPrefab = GetBlockTile(_blockDataArray[y, x]);
                            _blockArray[y, x] = Instantiate(blockPrefab, position, Quaternion.identity);
                        }
                        else
                        {
                            _blockArray[y, x] = null;
                        }
                    }
                    else
                    {
                        if (_blockDataArray[y, x] != 0)
                        {
                            Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2, y - _blockPlate.BlockPlateHeight / 2, 0);
                            GameObject blockPrefab = GetBlockTile(_blockDataArray[y, x]);
                            _blockArray[y, x] = Instantiate(blockPrefab, position, Quaternion.identity);
                        }
                        else
                        {
                            _blockArray[y, x] = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 블럭 데이터 배열 정렬
        /// </summary>
        public void SortBlockDataArray()
        {
            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
                {
                    if (_blockDataArray[y, x] == 0 && _blockPlate.BlockPlateArray[y, x])
                    {
                        for (int k = y + 1; k < _blockPlate.BlockPlateHeight; k++)
                        {
                            if (_blockDataArray[k, x] != 0)
                            {
                                _blockDataArray[y, x] = _blockDataArray[k, x];
                                _blockDataArray[k, x] = 0;
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 블럭 배열에 데이터는 존재하는데 뷰에는 블럭이 없는 경우
        /// </summary>
        public void SpawnBlock()
        {
            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
                {
                    if (_blockDataArray[y, x] != 0 && _blockPlate.BlockPlateArray[y, x] && _blockArray[y, x] == null)
                    {
                        Vector3 position;
                        Vector3 spawnPosition;
                        // 블럭을 블럭판 위에 생성 시킨 다음 자신의 위치까지 내려오도록 구성
                        if (_blockPlate.BlockPlateWidth % 2 == 0)
                        {
                            // 목표 위치
                            position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                            // 생성 위치 = x축은 동일하고 y축은 현재 위치에서 나를 포함한 내 위의 빈칸 수만큼 위로 생성
                            spawnPosition = new Vector3(position.x, position.y + GetAboveEmptySpaceCount(y), 0);
                        }
                        else
                        {
                            position = new Vector3(x - _blockPlate.BlockPlateWidth / 2, y - _blockPlate.BlockPlateHeight / 2, 0);
                            spawnPosition = new Vector3(position.x, position.y + GetAboveEmptySpaceCount(y), 0);
                        }
                        // 생성 후 목표위치로 이동
                        GameObject gemPrefab = GetBlockTile(_blockDataArray[y, x]);
                        _blockArray[y, x] = Instantiate(gemPrefab, spawnPosition, Quaternion.identity);
                        StartCoroutine(MoveBlockCoroutine(_blockArray[y, x], position, 0.5f));
                    }
                }
            }
        }

        /// <summary>
        /// 블럭 배열의 빈 공간에 데이터 생성
        /// </summary>
        public void SpawnBlockData()
        {
            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
                {
                    if (_blockDataArray[y, x] == 0 && _blockPlate.BlockPlateArray[y, x])
                    {
                        _blockDataArray[y, x] = Random.Range(1, 6);
                    }
                }
            }
        }

        /// <summary>
        /// 블럭 리프레쉬, 블럭 파괴 후 다시 생성함
        /// </summary>
        public void RefreshBlock()
        {
            for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
                {
                    // 데이터에는 빈 블록인데 뷰에는 블록이 있는 경우
                    if (_blockDataArray[y, x] == 0 && _blockArray[y, x] != null)
                    {
                        Destroy(_blockArray[y, x]);
                        _blockArray[y, x] = null;
                    }
                    // 데이터에는 블록이 있는데 뷰에는 블록이 없는 경우
                    else if (_blockPlate.BlockPlateArray[y, x] && _blockDataArray[y, x] != 0)
                    {
                        if (_blockArray[y, x] != null) Destroy(_blockArray[y, x]);

                        Vector3 position;
                        if (_blockPlate.BlockPlateWidth % 2 == 0)
                        {
                            position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                        }
                        else
                        {
                            position = new Vector3(x - _blockPlate.BlockPlateWidth / 2, y - _blockPlate.BlockPlateHeight / 2, 0);
                        }
                        GameObject gemPrefab = GetBlockTile(_blockDataArray[y, x]);
                        _blockArray[y, x] = Instantiate(gemPrefab, position, Quaternion.identity);
                    }
                }
            }
        }

        /// <summary>
        /// 테스트 코드. 하단 블럭을 ㅗ자 형태로 파괴
        /// </summary>
        /// <returns></returns>
        public bool TestDeleteBlock()
        {
            bool deleted = false;
            if (_blockDataArray[0, 1] != 0)
            {
                _blockDataArray[0, 1] = 0;
                Destroy(_blockArray[0, 1]);
                _blockArray[0, 1] = null;
                deleted = true;
            }
            if (_blockDataArray[0, 2] != 0)
            {
                _blockDataArray[0, 2] = 0;
                Destroy(_blockArray[0, 2]);
                _blockArray[0, 2] = null;
                deleted = true;
            }
            if (_blockDataArray[0, 3] != 0)
            {
                _blockDataArray[0, 3] = 0;
                Destroy(_blockArray[0, 3]);
                _blockArray[0, 3] = null;
                deleted = true;
            }
            if (_blockDataArray[1, 2] != 0)
            {
                _blockDataArray[1, 2] = 0;
                Destroy(_blockArray[1, 2]);
                _blockArray[1, 2] = null;
                deleted = true;
            }
            return deleted;
        }

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
                    if (_blockPlate.BlockPlateArray[y, x] && _blockDataArray[y, x] == 0)
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
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetAboveEmptySpaceCount(int y)
        {
            int count = 0;
            for (int i = y; i < _blockPlate.BlockPlateHeight; i++)
            {
                if (_blockArray[i, y] == null && _blockPlate.BlockPlateArray[i, y])
                {
                    count++;
                }
                else
                {
                    break; // 빈 공간이 아니면 중단
                }
            }
            return count + 1;
        }

        /// <summary>
        /// 블럭 이동을 위한 코루틴. 이후 DOTween으로 교체될수도 있음
        /// </summary>
        /// <param name="block"></param>
        /// <param name="targetPosition"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public IEnumerator MoveBlockCoroutine(GameObject block, Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = block.transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                block.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            block.transform.position = targetPosition;
            // 블럭 이동이 완료되면 빈 블럭 카운트 감소

            BlankBlockCount--;
        }

        public void StopCoroutine()
        {
            StopAllCoroutines();
        }
    }
}