using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCR;

namespace KDJ
{
    [System.Serializable]
    public class Block
    {
        public int BlockType { get; set; }
        public GameObject BlockInstance { get; set; } = null;
        public GemType GemType { get; set; }
    }

    public class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _blockPrefabs = new List<GameObject>();
        [SerializeField] public BlockPlate BlockPlate;

        //TestObject
        [SerializeField] private IngredientSO _lemon;
        [SerializeField] private IngredientSO _strawberry;
        [SerializeField] private IngredientSO _grape;

        private Queue<Block>[] _blockWaitingQueue;
        private OrderStateController _bakingTest;

        public List<GemType> DestroyBlockData { get; private set; } = new List<GemType>();
        public Block[,] BlockArray;
        public int BlankBlockCount = 0;

        #region 초기화
        /// <summary>
        /// 블럭 배열 초기화
        /// </summary>
        public void InitBlockArray()
        {
            // 추가 생성될 블럭을 담아놓을 큐를 생성. 배열은 미리 블럭을 꺼내 로드할 행 하나만 추가
            BlockArray = new Block[BlockPlate.BlockPlateHeight + 1, BlockPlate.BlockPlateWidth];
            _blockWaitingQueue = new Queue<Block>[BlockPlate.BlockPlateWidth];
            _bakingTest = FindObjectOfType<OrderStateController>();

            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockArray.GetLength(0); y++)
                {
                    int num = Random.Range(1, 6);

                    if (y < BlockPlate.BlockPlateHeight)
                    {
                        if (BlockPlate.BlockPlateArray[y, x]) BlockArray[y, x] = new Block { BlockType = num, GemType = (GemType)num - 1 };
                    }
                    else
                    {
                        BlockArray[y, x] = new Block { BlockType = num, GemType = (GemType)num - 1 };
                    }
                }
                _blockWaitingQueue[x] = new Queue<Block>();
            }

            // 테스트 코드
            //BlockArray[3, 2].BlockType = 12;
            //BlockArray[3, 2].GemType = GemType.Box;
            //BlockArray[3, 3].BlockType = 12;
            //BlockArray[3, 3].GemType = GemType.Box;
            //BlockArray[3, 4].BlockType = 12;
            //BlockArray[3, 4].GemType = GemType.Box;
            //BlockArray[3, 5].BlockType = 12;
            //BlockArray[3, 5].GemType = GemType.Box;
            //_blockArray[3, 4].BlockType = 6;
        }

        /// <summary>
        /// 초기 블럭 생성
        /// </summary>
        public void DrawBlock()
        {
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockArray.GetLength(0); y++)
                {
                    if (BlockPlate.BlockPlateWidth % 2 == 0)
                    {
                        if (y < BlockPlate.BlockPlateHeight)
                        {
                            if (BlockPlate.BlockPlateArray[y, x] && BlockArray[y, x].BlockType != 0)
                            {
                                Vector3 position = new Vector3(x - BlockPlate.BlockPlateWidth / 2 + 0.5f, y - BlockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                                GameObject blockPrefab = GetBlockTile(BlockArray[y, x].BlockType);
                                BlockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                            }
                        }
                        else
                        {
                            if (BlockArray[y, x].BlockType != 0)
                            {
                                Vector3 position = new Vector3(x - BlockPlate.BlockPlateWidth / 2 + 0.5f, y - BlockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                                GameObject blockPrefab = GetBlockTile(BlockArray[y, x].BlockType);
                                BlockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                            }
                        }

                    }
                    else
                    {
                        if (y < BlockPlate.BlockPlateHeight)
                        {
                            if (BlockPlate.BlockPlateArray[y, x] && BlockArray[y, x].BlockType != 0)
                            {
                                Vector3 position = new Vector3(x - BlockPlate.BlockPlateWidth / 2, y - BlockPlate.BlockPlateHeight / 2, 0);
                                GameObject blockPrefab = GetBlockTile(BlockArray[y, x].BlockType);
                                BlockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                            }
                        }
                        else
                        {
                            if (BlockArray[y, x].BlockType != 0)
                            {
                                Vector3 position = new Vector3(x - BlockPlate.BlockPlateWidth / 2, y - BlockPlate.BlockPlateHeight / 2, 0);
                                GameObject blockPrefab = GetBlockTile(BlockArray[y, x].BlockType);
                                BlockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
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
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    if (BlockArray[y, x] != null && BlockArray[y, x].GemType == GemType.Box) continue; // 6번 블럭은 건너뜀

                    if (BlockArray[y, x] == null && BlockPlate.BlockPlateArray[y, x])
                    {
                        if (BlockArray[y + 1, x] != null)
                        {
                            if (BlockArray[y + 1, x].GemType == GemType.Box)
                            {
                                if (BlockPlate.BlockPlateArray[y + 1, x - 1] && BlockArray[y + 1, x - 1] != null && BlockArray[y + 1, x - 1].GemType != GemType.Box)
                                {
                                    // 오른쪽 위에 블럭이 있는 경우
                                    BlockArray[y, x] = BlockArray[y + 1, x - 1];
                                    BlockArray[y + 1, x - 1] = null;
                                    BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                        BlockArray[y, x].BlockInstance.transform.position.x + 1,
                                        BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                }
                                else if (BlockPlate.BlockPlateArray[y + 1, x + 1] && BlockArray[y + 1, x + 1] != null && BlockArray[y + 1, x + 1].GemType != GemType.Box)
                                {
                                    // 오른쪽 위에 없고 왼쪽 위에 있는 경우
                                    BlockArray[y, x] = BlockArray[y + 1, x + 1];
                                    BlockArray[y + 1, x + 1] = null;
                                    BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                        BlockArray[y, x].BlockInstance.transform.position.x - 1,
                                        BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                }
                                else
                                {
                                    // 둘다 없는 경우 건너뜀
                                    continue;
                                }
                            }
                            else
                            {
                                BlockArray[y, x] = BlockArray[y + 1, x];
                                BlockArray[y + 1, x] = null;
                                BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                    BlockArray[y, x].BlockInstance.transform.position.x,
                                    BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                            }
                        }
                    }
                    else if (!BlockPlate.BlockPlateArray[y, x])
                    {
                        if (BlockArray[y + 1, x] != null)
                        {
                            if (BlockArray[y - 1, x] == null)
                            {
                                BlockArray[y - 1, x] = BlockArray[y + 1, x];
                                BlockArray[y + 1, x] = null;
                                BlockArray[y - 1, x].BlockInstance.transform.position = new Vector3(
                                    BlockArray[y - 1, x].BlockInstance.transform.position.x,
                                    BlockArray[y - 1, x].BlockInstance.transform.position.y - 2, 0);
                            }
                        }
                    }
                    else if (x > 0 && BlockArray[y, x - 1] == null && BlockArray[y + 1, x].GemType != GemType.Box)
                    {
                        BlockArray[y, x - 1] = BlockArray[y + 1, x];
                        BlockArray[y + 1, x] = null;
                        BlockArray[y, x - 1].BlockInstance.transform.position = new Vector3(
                            BlockArray[y, x - 1].BlockInstance.transform.position.x - 1,
                            BlockArray[y, x - 1].BlockInstance.transform.position.y - 1, 0);
                    }


                }

                if (_blockWaitingQueue[x].Count == 0)
                {
                    int num = Random.Range(1, 6);
                    _blockWaitingQueue[x].Enqueue(new Block { BlockType = num, GemType = (GemType)num - 1 });
                }

                // y축 생성이 끝나고 대기열 [_blockPlate.BlockPlateHeight, x]에 블럭이 없는 경우 큐에서 꺼내와서 할당
                if (BlockArray[BlockPlate.BlockPlateHeight, x] == null && _blockWaitingQueue[x].Count > 0)
                {
                    Vector3 position;

                    if (BlockPlate.BlockPlateWidth % 2 == 0)
                    {
                        position = new Vector3(x - BlockPlate.BlockPlateWidth / 2 + 0.5f, BlockPlate.BlockPlateHeight - BlockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                    }
                    else
                    {
                        position = new Vector3(x - BlockPlate.BlockPlateWidth / 2, BlockPlate.BlockPlateHeight - BlockPlate.BlockPlateHeight / 2, 0);
                    }

                    BlockArray[BlockPlate.BlockPlateHeight, x] = _blockWaitingQueue[x].Dequeue();
                    BlockArray[BlockPlate.BlockPlateHeight, x].BlockInstance = Instantiate(GetBlockTile(BlockArray[BlockPlate.BlockPlateHeight, x].BlockType), position, Quaternion.identity);
                }
            }

        }

        /// <summary>
        /// 빈칸이 존재하는 경우 블럭 생성
        /// </summary>
        public void SpawnBlock()
        {
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    if (BlockPlate.BlockPlateArray[y, x] && BlockArray[y, x] == null)
                    {
                        // 빈칸이 있는 경우 해당 열 큐에 블럭을 생성해서 추가
                        int num = Random.Range(1, 6);
                        _blockWaitingQueue[x].Enqueue(new Block { BlockType = num, GemType = (GemType)num - 1 });
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
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    if (BlockPlate.BlockPlateArray[y, x])
                    {
                        if (BlockArray[y, x] == null || BlockArray[y, x].BlockInstance == null)
                        {
                            if (BlockArray[y, x].BlockInstance != null) Destroy(BlockArray[y, x].BlockInstance);

                            // 이 부분에 파괴된 블럭의 데이터를 내보낼 로직 추가하면 됨
                            //switch (BlockArray[y, x].GemType)
                            //{
                            //    case GemType.Grape:
                            //        _bakingTest?.AddIngredient(_grape);
                            //        break;
                            //    case GemType.Strawberry:
                            //        _bakingTest?.AddIngredient(_strawberry);
                            //        break;
                            //    case GemType.Lemon:
                            //        _bakingTest?.AddIngredient(_lemon);
                            //        break;
                            //
                            //}

                            BlockArray[y, x] = null;
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
            if (BlockArray[0, 1] != null && BlockArray[0, 1].BlockInstance != null)
            {
                Destroy(BlockArray[0, 1].BlockInstance);
                BlockArray[0, 1] = null;
                deleted = true;
            }
            if (BlockArray[0, 2] != null && BlockArray[0, 2].BlockInstance != null)
            {
                Destroy(BlockArray[0, 2].BlockInstance);
                BlockArray[0, 2] = null;
                deleted = true;
            }
            if (BlockArray[0, 3] != null && BlockArray[0, 3].BlockInstance != null)
            {
                Destroy(BlockArray[0, 3].BlockInstance);
                BlockArray[0, 3] = null;
                deleted = true;
            }
            if (BlockArray[1, 2] != null && BlockArray[1, 2].BlockInstance != null)
            {
                Destroy(BlockArray[1, 2].BlockInstance);
                BlockArray[1, 2] = null;
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
                case 12: return _blockPrefabs[5];
                default: return null;
            }
        }

        /// <summary>
        /// 블록 배열에 빈 배열이 있는지 확인
        /// </summary>
        /// <returns></returns>
        public bool HasEmptyBlocks()
        {
            BlankBlockCount = 0; // 빈 블럭 카운트 초기화

            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    if (BlockPlate.BlockPlateArray[y, x] && BlockArray[y, x] == null)
                    {
                        BlankBlockCount++;
                    }
                }
            }
            return BlankBlockCount > 0;
        }

        /// <summary>
        /// 블록 배열의 오브젝트가 비어있는지 확인
        /// </summary>
        /// <returns></returns>
        public bool HasEmptyBlockObjects()
        {
            BlankBlockCount = 0; // 빈 블럭 카운트 초기화

            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    if (BlockPlate.BlockPlateArray[y, x] && BlockArray[y, x].BlockInstance == null)
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
            for (int i = y; i < BlockPlate.BlockPlateHeight; i++)
            {
                if (BlockArray[i, x] == null && BlockPlate.BlockPlateArray[i, x])
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