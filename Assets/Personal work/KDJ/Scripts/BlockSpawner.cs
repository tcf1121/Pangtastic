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
        public int Score { get; private set; } = 10;
        public GameObject BlockInstance { get; set; } = null;
        public GemType GemType { get; set; }
        public bool IsObstacle { get; set; } = false;
    }

    public class Cloche : Block
    {
        public int CurrentHP { get; set; } = 1;
        public int X { get; set; }
        public int Y { get; set; }

        public void TakeDamage(BoardManager boardManager)
        {
            CurrentHP--;

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                Broken(boardManager, X, Y);
            }
        }

        public void Broken(BoardManager boardManager, int x, int y)
        {
            Object.Destroy(boardManager.Spawner.BlockArray[y, x].BlockInstance);
            boardManager.Spawner.SpawnRandomBlock(x, y);
        }
    }

    public class ObstacleBlock : Block
    {
        public BoardCell Cell { get; set; }
    }

    public class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _blockPrefabs = new List<GameObject>();
        [SerializeField] public BlockPlate BlockPlate;

        //TestObject
        [SerializeField] private IngredientSO _lemon;
        [SerializeField] private IngredientSO _strawberry;
        [SerializeField] private IngredientSO _grape;
        [SerializeField] private IngredientSO _apple;
        [SerializeField] private IngredientSO _carrot;

        private Queue<Block>[] _blockWaitingQueue;
        private OrderStateController _bakingTest;
        private CustomerFlowController _test;

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
            // _bakingTest = FindObjectOfType<OrderStateController>();
            // _test = FindObjectOfType<CustomerFlowController>();
            // _test.Spawn();

            // BlockArray[3, 3] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 2, X = 3, Y = 3 };

            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockArray.GetLength(0); y++)
                {
                    int num = Random.Range(1, 7);

                    if (y < BlockPlate.BlockPlateHeight)
                    {
                        if (BlockPlate.BlockPlateArray[y, x] && BlockArray[y, x] == null) BlockArray[y, x] = new Block { BlockType = num, GemType = (GemType)num - 1 };
                    }
                    else
                    {
                        BlockArray[y, x] = new Block { BlockType = num, GemType = (GemType)num - 1 };
                    }
                }
                _blockWaitingQueue[x] = new Queue<Block>();
            }

            // BlockArray[0, 0] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 0, Y = 0 };
            // BlockArray[0, 1] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 1, Y = 0 };
            // BlockArray[0, 2] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 2, Y = 0 };
            // BlockArray[0, 3] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 3, Y = 0 };
            // BlockArray[0, 4] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 4, Y = 0 };
            // BlockArray[0, 5] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 5, Y = 0 };
            // BlockArray[3, 0] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 0, Y = 1 };
            // BlockArray[3, 1] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 1, Y = 1 };
            // BlockArray[3, 2] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 2, Y = 1 };
            BlockArray[2, 3] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 3, Y = 2 };
            BlockArray[2, 4] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 4, Y = 2 };
            BlockArray[2, 5] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 5, Y = 2 };

            // 테스트 코드

            //BlockArray[3, 4] = new ObstacleBlock { Cell = new BoardCell(new Vector3Int(3, 4, 0), GemType.Cloche), IsObstacle = true, BlockType = (int)GemType.Cloche + 1, GemType = GemType.Cloche };
            //BlockArray[3, 2].BlockType = 10;
            //BlockArray[3, 2].GemType = (GemType)9;
            //BlockArray[3, 3].BlockType = 7;
            //BlockArray[3, 3].GemType = (GemType)7;
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
        /// 블럭 배열 정렬 x축 0번부터 위로 한줄씩 진행
        /// </summary>
        public void SortBlockArray()
        {
            Debug.Log("블럭 배열 정렬");
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                while (true)
                {
                    bool canMove = false;
                    for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                    {
                        if (BlockArray[y, x] != null && BlockArray[y, x].IsObstacle) continue;

                        if (BlockArray[y, x] == null && BlockPlate.BlockPlateArray[y, x])
                        {
                            // 내가 비어있다면 윗칸의 블럭을 내 위치로 내림

                            if (BlockArray[y + 1, x] != null)
                            {
                                if (!BlockArray[y + 1, x].IsObstacle)
                                {
                                    // 내 위가 방해 블럭이 아니라면 바로 내림
                                    BlockArray[y, x] = BlockArray[y + 1, x];
                                    BlockArray[y + 1, x] = null;
                                    BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                        BlockArray[y, x].BlockInstance.transform.position.x,
                                        BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                    canMove = true;
                                }
                                else
                                {
                                    // 방해 블럭이고
                                    if (x - 1 >= 0 && BlockPlate.BlockPlateArray[y + 1, x - 1] && BlockArray[y + 1, x - 1] != null && !BlockArray[y + 1, x - 1].IsObstacle)
                                    {
                                        // 왼쪽 위가 일반 블럭이라면
                                        Debug.Log("왼쪽 위에 있는 블록 가져옴");
                                        BlockArray[y, x] = BlockArray[y + 1, x - 1];
                                        BlockArray[y + 1, x - 1] = null;
                                        BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                            BlockArray[y, x].BlockInstance.transform.position.x + 1,
                                            BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                        canMove = true;
                                    }
                                    else if (x + 1 < BlockPlate.BlockPlateWidth && BlockPlate.BlockPlateArray[y + 1, x + 1] && BlockArray[y + 1, x + 1] != null && !BlockArray[y + 1, x + 1].IsObstacle)
                                    {
                                        // 왼쪽 위가 방해 블럭이고 오른쪽 위가 일반 블럭이라면
                                        Debug.Log("오른쪽 위에 있는 블록 가져옴");
                                        BlockArray[y, x] = BlockArray[y + 1, x + 1];
                                        BlockArray[y + 1, x + 1] = null;
                                        BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                            BlockArray[y, x].BlockInstance.transform.position.x - 1,
                                            BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                        canMove = true;
                                    }
                                    else
                                    {
                                        // 둘 다 방해 블럭이라면 패스
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                // 내 위가 비어있다면 내 윗칸에 방해 블럭이 있는지 체크하고 내 아래가 비어있는지도 체크
                                if (GetAboveObstacleBlock(x, y))
                                {
                                    // 있다면 내 위치에서 왼쪽 위가 일반 블럭일 경우 가져오기
                                    if (x - 1 >= 0 && BlockPlate.BlockPlateArray[y + 1, x - 1] && BlockArray[y + 1, x - 1] != null && !BlockArray[y + 1, x - 1].IsObstacle)
                                    {
                                        Debug.Log("왼쪽 위에 있는 블록 가져옴");
                                        BlockArray[y, x] = BlockArray[y + 1, x - 1];
                                        BlockArray[y + 1, x - 1] = null;
                                        BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                            BlockArray[y, x].BlockInstance.transform.position.x + 1,
                                            BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                        canMove = true;
                                    }
                                    else if (x + 1 < BlockPlate.BlockPlateWidth && BlockPlate.BlockPlateArray[y + 1, x + 1] && BlockArray[y + 1, x + 1] != null && !BlockArray[y + 1, x + 1].IsObstacle)
                                    {
                                        // 왼쪽 위가 비어있다면 오른쪽 위를 체크
                                        Debug.Log("오른쪽 위에 있는 블록 가져옴");
                                        BlockArray[y, x] = BlockArray[y + 1, x + 1];
                                        BlockArray[y + 1, x + 1] = null;
                                        BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                            BlockArray[y, x].BlockInstance.transform.position.x - 1,
                                            BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                        canMove = true;
                                    }
                                    else
                                    {
                                        // 둘 다 없다면 패스
                                        continue;
                                    }
                                }
                            }
                        }

                        if (_blockWaitingQueue[x].Count == 0)
                        {
                            int num = Random.Range(1, 7);
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

                    if (canMove == false) break;
                }
            }
        }


        /// <summary>
        /// 위에서 아래로 순회하며 블록을 이동
        /// </summary>
        public void SortBlockArray2()
        {
            // 이번엔 위에서 아래로 내려오면서 순환하는 로직
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                while (true)
                {
                    bool canMove = false;

                    for (int y = BlockPlate.BlockPlateHeight - 1; y >= 0; y--)
                    {
                        if (BlockArray[y, x] != null && BlockArray[y, x].IsObstacle) continue;

                        if (BlockArray[y, x] == null && BlockPlate.BlockPlateArray[y, x])
                        {
                            // 내가 비어있다면 윗칸의 블럭을 내 위치로 내림

                            if (BlockArray[y + 1, x] != null)
                            {
                                if (!BlockArray[y + 1, x].IsObstacle)
                                {
                                    // 내 위가 방해 블럭이 아니라면 바로 내림
                                    BlockArray[y, x] = BlockArray[y + 1, x];
                                    BlockArray[y + 1, x] = null;
                                    BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                        BlockArray[y, x].BlockInstance.transform.position.x,
                                        BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                    canMove = true;
                                }
                                else
                                {
                                    // 방해 블럭이고
                                    if (x - 1 >= 0 && BlockPlate.BlockPlateArray[y + 1, x - 1] && BlockArray[y + 1, x - 1] != null && !BlockArray[y + 1, x - 1].IsObstacle)
                                    {
                                        // 왼쪽 위가 일반 블럭이라면
                                        Debug.Log("왼쪽 위에 있는 블록 가져옴");
                                        BlockArray[y, x] = BlockArray[y + 1, x - 1];
                                        BlockArray[y + 1, x - 1] = null;
                                        BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                            BlockArray[y, x].BlockInstance.transform.position.x + 1,
                                            BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                        canMove = true;
                                    }
                                    else if (x + 1 < BlockPlate.BlockPlateWidth && BlockPlate.BlockPlateArray[y + 1, x + 1] && BlockArray[y + 1, x + 1] != null && !BlockArray[y + 1, x + 1].IsObstacle)
                                    {
                                        // 왼쪽 위가 방해 블럭이고 오른쪽 위가 일반 블럭이라면
                                        Debug.Log("오른쪽 위에 있는 블록 가져옴");
                                        BlockArray[y, x] = BlockArray[y + 1, x + 1];
                                        BlockArray[y + 1, x + 1] = null;
                                        BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                            BlockArray[y, x].BlockInstance.transform.position.x - 1,
                                            BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                        canMove = true;
                                    }
                                    else
                                    {
                                        // 둘 다 방해 블럭이라면 패스
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                // 내 위가 비어있다면 내 윗칸에 방해 블럭이 있는지 체크하고 내 아래가 비어있는지도 체크
                                if (GetAboveObstacleBlock(x, y))
                                {
                                    // 있다면 내 위치에서 왼쪽 위가 일반 블럭일 경우 가져오기
                                    if (x - 1 >= 0 && BlockPlate.BlockPlateArray[y + 1, x - 1] && BlockArray[y + 1, x - 1] != null && !BlockArray[y + 1, x - 1].IsObstacle)
                                    {
                                        Debug.Log("왼쪽 위에 있는 블록 가져옴");
                                        BlockArray[y, x] = BlockArray[y + 1, x - 1];
                                        BlockArray[y + 1, x - 1] = null;
                                        BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                            BlockArray[y, x].BlockInstance.transform.position.x + 1,
                                            BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                        canMove = true;
                                    }
                                    else if (x + 1 < BlockPlate.BlockPlateWidth && BlockPlate.BlockPlateArray[y + 1, x + 1] && BlockArray[y + 1, x + 1] != null && !BlockArray[y + 1, x + 1].IsObstacle)
                                    {
                                        // 왼쪽 위가 비어있다면 오른쪽 위를 체크
                                        Debug.Log("오른쪽 위에 있는 블록 가져옴");
                                        BlockArray[y, x] = BlockArray[y + 1, x + 1];
                                        BlockArray[y + 1, x + 1] = null;
                                        BlockArray[y, x].BlockInstance.transform.position = new Vector3(
                                            BlockArray[y, x].BlockInstance.transform.position.x - 1,
                                            BlockArray[y, x].BlockInstance.transform.position.y - 1, 0);
                                        canMove = true;
                                    }
                                    else
                                    {
                                        // 둘 다 없다면 패스
                                        continue;
                                    }
                                }
                            }
                        }
                    }

                    if (_blockWaitingQueue[x].Count == 0)
                    {
                        int num = Random.Range(1, 7);
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

                    if (!canMove) break;
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
                        int num = Random.Range(1, 7);
                        _blockWaitingQueue[x].Enqueue(new Block { BlockType = num, GemType = (GemType)num - 1 });
                    }
                }
            }
        }

        public void CheckBlockInArray()
        {
            int count = 0;
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    if (BlockArray[y, x] != null)
                    {
                        count++;
                    }
                }
            }
        }

        /// <summary>
        /// 블럭 체크. 시각적 오브젝트가 파괴된 경우에도 해당 칸을 빈칸으로 설정
        /// </summary>
        public void CheckBlockArray(BoardManager boardManager)
        {
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    if (BlockPlate.BlockPlateArray[y, x])
                    {
                        if (BlockArray[y, x] != null && BlockArray[y, x].BlockInstance == null)
                        {
                            // if (BlockArray[y, x].BlockInstance != null) Destroy(BlockArray[y, x].BlockInstance);

                            // 이 부분에 파괴된 블럭의 데이터를 내보낼 로직 추가하면 됨
                            //switch (BlockArray[y, x].BlockType)
                            //{
                            //    case 1:
                            //        _bakingTest?.AddIngredient(_carrot);
                            //        break;
                            //    case 2:
                            //        _bakingTest?.AddIngredient(_lemon);
                            //        break;
                            //    case 3:
                            //        _bakingTest?.AddIngredient(_grape);
                            //        break;
                            //    case 4:
                            //        _bakingTest?.AddIngredient(_strawberry);
                            //        break;
                            //    case 5:
                            //        _bakingTest?.AddIngredient(_apple);
                            //        break;
                            //    default:
                            //        break;
                            //
                            //}
                            if (BlockArray[y, x] is Cloche)
                            {
                                SpawnRandomBlock(x, y);
                                continue;
                            }

                            BlockArray[y, x] = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 지정 위치에 입력받은 블록을 생성
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        // 추후에 3번째 매개변수 Gemtype을 받도록 변경해야합니다.
        public void SpawnBlock(int x, int y, int blockNum)
        {
            GameObject blockPrefab = GetBlockTile(blockNum);
            if (blockPrefab != null)
            {
                GameObject blockInstance = Instantiate(blockPrefab);

                if (BlockPlate.BlockPlateWidth % 2 == 0)
                    blockInstance.transform.position = new Vector3(x - BlockPlate.BlockPlateWidth / 2 + 0.5f, y - BlockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                else
                    blockInstance.transform.position = new Vector3(x - BlockPlate.BlockPlateWidth / 2, y - BlockPlate.BlockPlateHeight / 2, 0);

                BlockArray[y, x] = new Block { BlockInstance = blockInstance, BlockType = blockNum, GemType = (GemType)blockNum - 1 };
            }
        }

        public void RandomPosSpawnSpecialBlock(int blockNum)
        {
            int x = Random.Range(0, BlockPlate.BlockPlateWidth);
            int y = Random.Range(0, BlockPlate.BlockPlateHeight);

            while (true)
            {
                if (BlockPlate.BlockPlateArray[y, x] && BlockArray[y, x].GemType > GemType.Sugar && BlockArray[y, x].GemType < GemType.Dough)
                {
                    x = Random.Range(0, BlockPlate.BlockPlateWidth);
                    y = Random.Range(0, BlockPlate.BlockPlateHeight);
                }
                else
                {
                    break;
                }
            }

            GameObject blockPrefab = GetBlockTile(blockNum);

            Destroy(BlockArray[y, x].BlockInstance);

            if (blockPrefab != null)
            {
                GameObject blockInstance = Instantiate(blockPrefab);

                if (BlockPlate.BlockPlateWidth % 2 == 0)
                    blockInstance.transform.position = new Vector3(x - BlockPlate.BlockPlateWidth / 2 + 0.5f, y - BlockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                else
                    blockInstance.transform.position = new Vector3(x - BlockPlate.BlockPlateWidth / 2, y - BlockPlate.BlockPlateHeight / 2, 0);

                BlockArray[y, x] = new Block { BlockInstance = blockInstance, BlockType = blockNum, GemType = (GemType)blockNum - 1 };
            }
        }

        /// <summary>
        /// 지정 위치에 랜덤 일반 블록을 생성
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SpawnRandomBlock(int x, int y)
        {
            int randomBlockType = Random.Range(1, 7); // 1부터 6까지의 랜덤 블럭 타입
            SpawnBlock(x, y, randomBlockType);
        }

        public void ShuffleBlockArray()
        {
            // 블록 배열을 섞는 로직 구현
            // 각 배열 인덱스에 접근하여 기존 데이터를 제거하고 랜덤 일반 블럭을 생성
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    if (BlockPlate.BlockPlateArray[y, x])
                    {
                        if (BlockArray[y, x] != null && BlockArray[y, x].BlockInstance != null)
                        {
                            Destroy(BlockArray[y, x].BlockInstance);
                        }
                        int randomBlockType = Random.Range(1, 7); // 1부터 6까지의 랜덤 블럭 타입
                        SpawnBlock(x, y, randomBlockType);
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
                case 6: return _blockPrefabs[5];
                case 7: return _blockPrefabs[6];
                case 8: return _blockPrefabs[7];
                case 9: return _blockPrefabs[8];
                case 10: return _blockPrefabs[9];
                case 11: return _blockPrefabs[10];
                case 12: return _blockPrefabs[11];
                case 13: return _blockPrefabs[12];
                case 14: return _blockPrefabs[13];
                case 15: return _blockPrefabs[14];
                case 16: return _blockPrefabs[15];
                case 17: return _blockPrefabs[16];
                case 18: return _blockPrefabs[17];
                case 19: return _blockPrefabs[18];
                case 20: return _blockPrefabs[19];
                default: return null;
            }
        }

        /// <summary>
        /// 블록 배열에 빈 배열이 있는지 확인
        /// </summary>
        /// <returns></returns>
        public bool HasEmptyBlocks()
        {
            int blankBlockCount = 0; // 빈 블럭 카운트 초기화

            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    if (BlockPlate.BlockPlateArray[y, x] && BlockArray[y, x] == null)
                    {
                        blankBlockCount++;
                    }
                }
            }
            Debug.Log($"빈칸 개수 : {blankBlockCount}");
            return blankBlockCount > 0;
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
                    if (BlockPlate.BlockPlateArray[y, x] && (BlockArray[y, x] == null || BlockArray[y, x].BlockInstance == null))
                    {
                        BlankBlockCount++;
                    }
                }
            }
            return BlankBlockCount > 0;
        }

        /// <summary>
        /// 배열을 순회하며 블록이 빈칸으로 움직일 수 있는지 확인
        /// </summary>
        /// <returns></returns>
        public bool CanBlockMoveInArray()
        {
            bool result = false;

            Debug.Log("블럭 배열 이동 체크");
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    if (BlockArray[y, x] != null && BlockArray[y, x].IsObstacle) continue;

                    if (BlockArray[y, x] == null && BlockPlate.BlockPlateArray[y, x])
                    {
                        if (BlockArray[y + 1, x] != null)
                        {
                            if (!BlockArray[y + 1, x].IsObstacle)
                            {
                                result = true;
                            }
                            else
                            {
                                if (x - 1 >= 0 && BlockPlate.BlockPlateArray[y + 1, x - 1] && BlockArray[y + 1, x - 1] != null && !BlockArray[y + 1, x - 1].IsObstacle)
                                {
                                    result = true;
                                }
                                else if (x + 1 < BlockPlate.BlockPlateWidth && BlockPlate.BlockPlateArray[y + 1, x + 1] && BlockArray[y + 1, x + 1] != null && !BlockArray[y + 1, x + 1].IsObstacle)
                                {
                                    result = true;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (GetAboveObstacleBlock(x, y))
                            {
                                if (x - 1 >= 0 && BlockPlate.BlockPlateArray[y + 1, x - 1] && BlockArray[y + 1, x - 1] != null && !BlockArray[y + 1, x - 1].IsObstacle)
                                {
                                    result = true;
                                }
                                else if (x + 1 < BlockPlate.BlockPlateWidth && BlockPlate.BlockPlateArray[y + 1, x + 1] && BlockArray[y + 1, x + 1] != null && !BlockArray[y + 1, x + 1].IsObstacle)
                                {
                                    result = true;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 위에서 아래로 순회하며 이동이 가능한지 체크
        /// </summary>
        /// <returns></returns>
        public bool CanBlockMoveInArray2()
        {
            bool result = false;
            // 이번엔 위에서 아래로 내려오면서 순환하는 로직
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = BlockPlate.BlockPlateHeight - 1; y >= 0; y--)
                {
                    if (BlockArray[y, x] != null && BlockArray[y, x].IsObstacle) continue;

                    if (BlockArray[y, x] == null && BlockPlate.BlockPlateArray[y, x])
                    {
                        if (BlockArray[y + 1, x] != null)
                        {
                            if (!BlockArray[y + 1, x].IsObstacle)
                            {
                                result = true;
                            }
                            else
                            {
                                if (x - 1 >= 0 && BlockPlate.BlockPlateArray[y + 1, x - 1] && BlockArray[y + 1, x - 1] != null && !BlockArray[y + 1, x - 1].IsObstacle)
                                {
                                    result = true;
                                }
                                else if (x + 1 < BlockPlate.BlockPlateWidth && BlockPlate.BlockPlateArray[y + 1, x + 1] && BlockArray[y + 1, x + 1] != null && !BlockArray[y + 1, x + 1].IsObstacle)
                                {
                                    result = true;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (GetAboveObstacleBlock(x, y))
                            {
                                if (x - 1 >= 0 && BlockPlate.BlockPlateArray[y + 1, x - 1] && BlockArray[y + 1, x - 1] != null && !BlockArray[y + 1, x - 1].IsObstacle)
                                {
                                    result = true;
                                }
                                else if (x + 1 < BlockPlate.BlockPlateWidth && BlockPlate.BlockPlateArray[y + 1, x + 1] && BlockArray[y + 1, x + 1] != null && !BlockArray[y + 1, x + 1].IsObstacle)
                                {
                                    result = true;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            return result;
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

        /// <summary>
        /// 윗칸을 순회하며 방해 블럭이 있는지 확인
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool GetAboveObstacleBlock(int x, int y)
        {
            for (int i = y; i < BlockPlate.BlockPlateHeight; i++)
            {
                if (BlockArray[i, x] is ObstacleBlock || BlockArray[i, x] is Cloche)
                {
                    return true; // 장애물이 있으면 true 반환
                }
            }
            return false; // 장애물이 없으면 false 반환
        }
        #endregion
    }
}