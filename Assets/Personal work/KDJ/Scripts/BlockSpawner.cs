using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCR;
using SCR_O;

namespace KDJ
{
    [System.Serializable]
    public class Block
    {
        public int BlockType { get; set; }
        public int Score { get; protected set; } = 10;
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

        [Header("애니메이션 설정")]
        [SerializeField] private float _stepDuration = 0.08f; // 한 스텝(한 칸 낙하)에 걸리는 시간. 템포를 조절합니다.

        private bool _isRefilling = false;

        private Queue<Block>[] _blockWaitingQueue;
        private OrderStateController _bakingTest;
        private CustomerFlowController _test;

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
        public Vector2Int ZeroPos;
        public Block[,] BlockArray;
        public Block[,] OverlayArray;
        public int BlankBlockCount = 0;

        #region 초기화
        /// <summary>
        /// 블럭 배열 초기화
        /// </summary>
        public void InitBlockArray()
        {
            // 배열과 큐를 생성. 배열은 없는 경우에만 생성
            if (BlockArray == null)
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
                    int num = Random.Range(SpawnRangeMin, SpawnRangeMax + 1);

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
            BlockArray[4, 2] = new DonutBag(2, 4);
            BlockArray[4, 3] = new DonutBag(3, 4);
            BlockArray[4, 4] = new DonutBag(4, 4);
            BlockArray[4, 5] = new DonutBag(5, 4);
            BlockArray[1, 5] = new Block { BlockType = 11, GemType = GemType.DonutBox };
            // BlockArray[2, 3] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 3, Y = 2 };
            // BlockArray[2, 4] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 4, Y = 2 };
            // BlockArray[2, 5] = new Cloche { BlockType = 15, GemType = GemType.Cloche, IsObstacle = true, CurrentHP = 1, X = 5, Y = 2 };

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
                                if (BlockArray[y, x].GemType == GemType.Ice)
                                    BlockArray[y, x].BlockInstance.GetComponent<SpriteRenderer>().sprite = (BlockArray[y, x] as SCR_O.Ice).GetIceImage();

                            }
                        }
                        else
                        {
                            if (BlockArray[y, x].BlockType != 0)
                            {
                                Vector3 position = new Vector3(x - BlockPlate.BlockPlateWidth / 2 + 0.5f, y - BlockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                                GameObject blockPrefab = GetBlockTile(BlockArray[y, x].BlockType);
                                BlockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                                if (BlockArray[y, x].GemType == GemType.Ice)
                                    BlockArray[y, x].BlockInstance.GetComponent<SpriteRenderer>().sprite = (BlockArray[y, x] as SCR_O.Ice).GetIceImage();
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
                                if (BlockArray[y, x].GemType == GemType.Ice)
                                    BlockArray[y, x].BlockInstance.GetComponent<SpriteRenderer>().sprite = (BlockArray[y, x] as SCR_O.Ice).GetIceImage();
                            }
                        }
                        else
                        {
                            if (BlockArray[y, x].BlockType != 0)
                            {
                                Vector3 position = new Vector3(x - BlockPlate.BlockPlateWidth / 2, y - BlockPlate.BlockPlateHeight / 2, 0);
                                GameObject blockPrefab = GetBlockTile(BlockArray[y, x].BlockType);
                                BlockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                                if (BlockArray[y, x].GemType == GemType.Ice)
                                    BlockArray[y, x].BlockInstance.GetComponent<SpriteRenderer>().sprite = (BlockArray[y, x] as SCR_O.Ice).GetIceImage();
                            }
                        }

                    }
                }
            }
        }

        public void DrawBlock2()
        {
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockArray.GetLength(0); y++)
                {
                    Vector3 pos = new Vector3(x + 0.5f + ZeroPos.x, y + 0.5f + ZeroPos.x, 0);
                    BlockArray[y, x].BlockInstance = Instantiate(_blockPrefabs[BlockArray[y, x].BlockType - 1]);
                    BlockArray[y, x].BlockInstance.transform.position = pos;
                    if (BlockArray[y, x].GemType == GemType.Ice)
                        BlockArray[y, x].BlockInstance.GetComponent<SpriteRenderer>().sprite = (BlockArray[y, x] as SCR_O.Ice).GetIceImage();
                    if (OverlayArray[y, x] != null)
                    {
                        OverlayArray[y, x].BlockInstance = Instantiate(_blockPrefabs[OverlayArray[y, x].BlockType - 1]);
                        OverlayArray[y, x].BlockInstance.transform.position = pos;
                    }
                }
            }
        }
        #endregion

        #region 블럭 관리
        private Vector3 GetWorldPosition(int x, int y)
        {
            float xPos, yPos;
            if (BlockPlate.BlockPlateWidth % 2 == 0)
            {
                xPos = x - BlockPlate.BlockPlateWidth / 2 + 0.5f;
            }
            else
            {
                xPos = x - BlockPlate.BlockPlateWidth / 2;
            }

            if (BlockPlate.BlockPlateHeight % 2 == 0)
            {
                yPos = y - BlockPlate.BlockPlateHeight / 2 + 0.5f;
            }
            else
            {
                yPos = y - BlockPlate.BlockPlateHeight / 2 + 0.5f;
            }

            return new Vector3(xPos, yPos, 0);
        }

        /// <summary>
        /// 해당 위치 바로 위에 있는 첫 블록이 방해물인지 확인합니다.
        /// '우물' 구조를 판단하기 위해 사용됩니다.
        /// </summary>
        /// <param name="x"> 확인할 열의 x 좌표 </param>
        /// <param name="y"> 기준이 되는 행의 y 좌표 </param>
        /// <returns> 바로 위의 첫 블록이 방해물이면 true, 아니면 false </returns>
        private bool IsWellBelowObstacle(int x, int y)
        {
            // y 바로 위부터 보드 최상단까지 확인
            for (int i = y + 1; i < BlockArray.GetLength(0); i++)
            {
                // 해당 위치에 블록이 있다면
                if (BlockArray[i, x] != null)
                {
                    // 그 블록이 방해물인지 여부를 반환
                    return BlockArray[i, x].IsObstacle;
                }
            }
            // 위로 블록이 전혀 없으면, 우물이 아님
            return false;
        }


        public IEnumerator RefillBoardCoroutine()
        {
            if (_isRefilling) yield break;
            _isRefilling = true;

            while (true)
            {
                bool activityThisStep = false;
                bool[,] movedToThisTick = new bool[BlockPlate.BlockPlateHeight, BlockPlate.BlockPlateWidth];

                // --- 1순위: 단순화된 낙하 로직 ---
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
                    {
                        if (BlockArray[y, x] == null && BlockPlate.BlockPlateArray[y, x] && !movedToThisTick[y, x])
                        {
                            Vector2Int sourcePos = new Vector2Int(-1, -1);
                            if (y + 1 < BlockArray.GetLength(0))
                            {
                                Block blockAbove = BlockArray[y + 1, x];
                                if (blockAbove != null)
                                {
                                    if (!blockAbove.IsObstacle)
                                    {
                                        // 위에 일반 블록이 있으면, 수직으로만 내립니다.
                                        sourcePos.Set(x, y + 1);
                                    }
                                    else // 위에 방해 블록이 있으면, 대각선만 확인합니다.
                                    {
                                        if (x > 0 && BlockArray[y + 1, x - 1] != null && !BlockArray[y + 1, x - 1].IsObstacle)
                                            sourcePos.Set(x - 1, y + 1);
                                        else if (x + 1 < BlockPlate.BlockPlateWidth && BlockArray[y + 1, x + 1] != null && !BlockArray[y + 1, x + 1].IsObstacle)
                                            sourcePos.Set(x + 1, y + 1);
                                    }
                                }
                            }

                            if (sourcePos.x != -1)
                            {
                                Block blockToMove = BlockArray[sourcePos.y, sourcePos.x];
                                BlockArray[y, x] = blockToMove;
                                BlockArray[sourcePos.y, sourcePos.x] = null;
                                movedToThisTick[y, x] = true;
                                activityThisStep = true;
                                StartCoroutine(MoveBlockCoroutine(blockToMove, GetWorldPosition(x, y), _stepDuration));
                            }
                        }
                    }
                }

                // --- 2순위: 모래 흐름 ---
                // 1순위에서 블록 움직임이 없었을 때만 실행하여, 두 로직이 한 틱에 동시 실행되는 것을 방지합니다.
                if (!activityThisStep)
                {
                    for (int y = BlockPlate.BlockPlateHeight - 1; y >= 1; y--)
                    {
                        for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
                        {
                            Block currentBlock = BlockArray[y, x];
                            if (currentBlock != null && !currentBlock.IsObstacle)
                            {
                                // 최종 조건: 바로 아래와 바로 위에 '블록'이 있고 && 흘러 들어갈 옆 칸이 방해물 아래의 '우물'일 때
                                if ((BlockArray[y + 1, x] == null || BlockArray[y + 1, x].IsObstacle) && BlockArray[y - 1, x] != null)
                                {
                                    // 왼쪽 아래로 흐르기
                                    if (x - 1 > 0 && BlockArray[y, x - 1] == null && BlockArray[y - 1, x - 1] == null && !movedToThisTick[y - 1, x - 1] && IsWellBelowObstacle(x - 1, y - 1))
                                    {
                                        BlockArray[y - 1, x - 1] = currentBlock;
                                        BlockArray[y, x] = null;
                                        movedToThisTick[y - 1, x - 1] = true;
                                        activityThisStep = true;
                                        StartCoroutine(MoveBlockCoroutine(currentBlock, GetWorldPosition(x - 1, y - 1), _stepDuration));
                                        continue;
                                    }
                                    // 오른쪽 아래로 흐르기
                                    if (x + 1 < BlockPlate.BlockPlateWidth && BlockArray[y, x + 1] == null && BlockArray[y - 1, x + 1] == null && !movedToThisTick[y - 1, x + 1] && IsWellBelowObstacle(x + 1, y - 1))
                                    {
                                        BlockArray[y - 1, x + 1] = currentBlock;
                                        BlockArray[y, x] = null;
                                        movedToThisTick[y - 1, x + 1] = true;
                                        activityThisStep = true;
                                        StartCoroutine(MoveBlockCoroutine(currentBlock, GetWorldPosition(x + 1, y - 1), _stepDuration));
                                    }
                                }
                            }
                        }
                    }
                }

                // --- 3순위: 새 블록 생성 ---
                for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
                {
                    if (BlockArray[BlockPlate.BlockPlateHeight, x] == null)
                    {
                        Block newBlock;
                        if (_blockWaitingQueue[x].Count > 0) newBlock = _blockWaitingQueue[x].Dequeue();
                        else
                        {
                            int num = Random.Range(SpawnRangeMin, SpawnRangeMax + 1);
                            newBlock = new Block { BlockType = num, GemType = (GemType)num - 1 };
                        }
                        BlockArray[BlockPlate.BlockPlateHeight, x] = newBlock;
                        activityThisStep = true;
                        Vector3 startPosition = GetWorldPosition(x, BlockPlate.BlockPlateHeight + 1);
                        GameObject blockPrefab = GetBlockTile(newBlock.BlockType);
                        newBlock.BlockInstance = Instantiate(blockPrefab, startPosition, Quaternion.identity);
                        Vector3 targetPosition = GetWorldPosition(x, BlockPlate.BlockPlateHeight);
                        StartCoroutine(MoveBlockCoroutine(newBlock, targetPosition, _stepDuration));
                    }
                }

                if (activityThisStep) yield return new WaitForSeconds(_stepDuration);
                else break;
            }

            _isRefilling = false;
        }

        /// <summary>
        /// 블록의 이동 애니메이션을 처리하는 헬퍼 코루틴입니다.
        /// </summary>
        private IEnumerator MoveBlockCoroutine(Block block, Vector3 targetPosition, float duration)
        {
            if (block == null || block.BlockInstance == null)
            {
                yield break;
            }

            Vector3 startPosition = block.BlockInstance.transform.position;
            float time = 0;

            while (time < duration)
            {
                if (block.BlockInstance == null) yield break; // 애니메이션 중 블록이 파괴될 경우를 대비

                block.BlockInstance.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            if (block.BlockInstance != null)
            {
                block.BlockInstance.transform.position = targetPosition; // 정확한 위치로 보정
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
                        int num = Random.Range(SpawnRangeMin, SpawnRangeMax + 1);
                        _blockWaitingQueue[x].Enqueue(new Block { BlockType = num, GemType = (GemType)num - 1 });
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
                            //if (BlockArray[y, x] is Cloche)
                            //{
                            //    SpawnRandomBlock(x, y);
                            //    continue;
                            //}

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
                if (BlockPlate.BlockPlateArray[y, x] && BlockArray[y, x].GemType > GemType.Sugar)
                {
                    x = Random.Range(0, BlockPlate.BlockPlateWidth);
                    y = Random.Range(0, BlockPlate.BlockPlateHeight);
                }
                else
                {
                    break;
                }
            }

            Destroy(BlockArray[y, x].BlockInstance);
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

        /// <summary>
        /// 지정 위치에 랜덤 일반 블록을 생성
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SpawnRandomBlock(int x, int y)
        {
            int randomBlockType = Random.Range(SpawnRangeMin, SpawnRangeMax + 1); // 1부터 6까지의 랜덤 블럭 타입
            SpawnBlock(x, y, randomBlockType);
        }

        public void ShuffleBlockArray()
        {
            // 블록 배열을 섞는 로직 구현
            // 각 배열 인덱스에 접근하여 일반 블럭이라면 기존 데이터를 제거하고 랜덤 일반 블럭을 생성
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
                {
                    if (BlockPlate.BlockPlateArray[y, x])
                    {
                        if (BlockArray[y, x] != null && BlockArray[y, x].BlockInstance != null && BlockArray[y, x].GemType <= GemType.Sugar)
                        {
                            Destroy(BlockArray[y, x].BlockInstance);
                            BlockArray[y, x] = null;
                            int randomBlockType = Random.Range(SpawnRangeMin, SpawnRangeMax + 1); // _spawnRangeMin부터 _spawnRangeMax까지의 랜덤 블럭 타입
                            SpawnBlock(x, y, randomBlockType);
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
            //Debug.Log($"빈칸 개수 : {blankBlockCount}");
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
        /// 배열을 순회하며 블록이 빈칸으로 움직일 수 있는지 확인합니다.
        /// RefillBoardCoroutine의 로직을 기반으로 하여, 실제 이동이 가능한지 여부만 체크합니다.
        /// </summary>
        /// <returns>이동할 블록이 하나라도 있으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public bool CanBlockMoveInArray()
        {
            // 1. 기존 블록이 아래나 대각선으로 이동할 수 있는지 확인
            for (int y = 0; y < BlockPlate.BlockPlateHeight; y++)
            {
                for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
                {
                    // 현재 위치가 비어있고, 블록이 존재할 수 있는 판인지 확인
                    if (BlockArray[y, x] == null && BlockPlate.BlockPlateArray[y, x])
                    {
                        // 바로 위(y+1)를 확인하기 전에 배열 범위를 벗어나지 않는지 확인
                        if (y + 1 >= BlockArray.GetLength(0)) continue;

                        // 조건 1: 바로 위에 방해물이 있는가?
                        bool obstacleAbove = (BlockArray[y + 1, x] != null) && (BlockArray[y + 1, x].IsObstacle);

                        if (obstacleAbove)
                        {
                            // 대각선 체크
                            // 좌상단
                            if (x > 0 && BlockArray[y + 1, x - 1] != null && !BlockArray[y + 1, x - 1].IsObstacle)
                            {
                                return true; // 이동 가능
                            }
                            // 우상단
                            if (x + 1 < BlockPlate.BlockPlateWidth && BlockArray[y + 1, x + 1] != null && !BlockArray[y + 1, x + 1].IsObstacle)
                            {
                                return true; // 이동 가능
                            }
                        }
                        else
                        {
                            // 수직 체크
                            if (BlockArray[y + 1, x] != null && !BlockArray[y + 1, x].IsObstacle)
                            {
                                return true; // 이동 가능
                            }
                        }
                    }
                }
            }

            // 2. 대기열에 새 블록이 있고, 보드 최상단에 들어갈 자리가 있는지 확인
            for (int x = 0; x < BlockPlate.BlockPlateWidth; x++)
            {
                // 최상단 바로 위(보이지 않는 생성 영역)가 비어있고, 해당 열의 대기열에 블록이 있다면
                if (BlockArray[BlockPlate.BlockPlateHeight, x] == null && _blockWaitingQueue[x].Count > 0)
                {
                    return true; // 새 블록이 내려올 수 있음
                }
            }

            return false; // 어떤 블록도 움직일 수 없음
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
                if (BlockArray[i, x] != null)
                {
                    if (BlockArray[i, x] is ObstacleBlock || BlockArray[i, x] is Cloche)
                    {
                        return true; // 장애물이 있으면 true 반환
                    }
                }
            }
            return false; // 장애물이 없으면 false 반환
        }
        #endregion
    }
}