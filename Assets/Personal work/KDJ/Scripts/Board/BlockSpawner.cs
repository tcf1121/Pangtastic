using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCR;

namespace KDJ
{
    [System.Serializable]
    public class BlockOverride
    {
        public int x;
        public int y;
        public GemType gemType;
    }

    public class BlockSpawner : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private List<GameObject> _blockPrefabs = new List<GameObject>();
        [SerializeField] private float _stepDuration = 0.08f; // 한 스텝(한 칸 낙하)에 걸리는 시간
        [SerializeField] private int _spawnRangeMax = 6;

        [Header("오브젝트 풀링")]
        public ObjectPool BlockPool;

        [SerializeField] private List<Sprite> normalBlockSprites = new List<Sprite>();

        [Header("테스트용 블록 교체 설정")]
        [SerializeField] private List<BlockOverride> _test_blockOverrides;
        [Header("블록 마스크")]
        [SerializeField] private GameObject _blockMaskPrefab;


        public GameBoardData GameBoardData { get; private set; }
        public List<GemType> DestroyedBlocks { get; private set; } = new List<GemType>();

        private Queue<Block>[] _blockWaitingQueue;
        private bool _isRefilling = false;

        /// <summary>
        /// 로드된 보드 데이터를 기반으로 Spawner를 초기화하고, 모든 블록을 생성합니다.
        /// </summary>
        public void Initialize(BoardManager boardManager, BoardData loadedBoardData, BlockPlate blockPlate, BlockMover blockMover)
        {
            GameBoardData = new GameBoardData(blockPlate);

            _blockWaitingQueue = new Queue<Block>[GameBoardData.Width];
            for (int i = 0; i < GameBoardData.Width; i++)
            {
                _blockWaitingQueue[i] = new Queue<Block>();
            }

            int arrayHeight = GameBoardData.BlockArray.GetLength(0);
            int boardHeight = GameBoardData.Height;
            int width = GameBoardData.Width;

            for (int y = 0; y < arrayHeight; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 블록이 존재할 수 있는 위치인지 확인 (플레이 영역 또는 대기열)
                    bool isSpawanablePosition = (y >= boardHeight) || blockPlate.BlockPlateArray[y, x];

                    if (isSpawanablePosition)
                    {
                        Block loadedBlock = loadedBoardData.BlockArray[y, x];
                        if (loadedBlock != null)
                        {
                            // 데이터가 있으면 해당 데이터 사용
                            if (loadedBlock.GemType == GemType.Random)
                            {
                                SetRandomBlock(x, y);
                            }
                            else
                            {
                                // 로드된 블록(ObstacleBlock 등)의 특성을 유지하기 위해 그대로 사용합니다.
                                GameBoardData.SetBlock(x, y, loadedBlock);
                            }
                        }
                        else
                        {
                            // 데이터가 없으면 랜덤 블록 생성
                            SetRandomBlock(x, y);
                        }
                    }

                    // 오버레이 블록 데이터 처리 (게임 보드 영역만)
                    if (y < boardHeight)
                    {
                        Block loadedOverlayBlock = loadedBoardData.OverlayArray[y, x];
                        if (loadedOverlayBlock != null)
                        {
                            // 로드된 오버레이 블록의 특성을 유지하기 위해 그대로 사용합니다.
                            GameBoardData.SetOverlayBlock(x, y, loadedOverlayBlock);
                        }
                    }
                }
            }

            // 게임 시작시 매치가 이루어진다면 데이터 셔플을 수행하여 매치되지 않는 상태를 만듭니다.
            if (boardManager.MatchChecker.AllBlockMatchCheck(boardManager))
            {
                DataShuffle(boardManager, 1000);
            }

            // 3. 모든 블록 GameObject 생성
            DrawAllBlocks(blockMover);
            DrawAllOverlayBlocks(blockMover);

            // 4. 현재 스테이지에 맞춰 스폰될 블록 종류 설정 
            // if (Manager.User.GetStage() < 52)
            // {
            //     _spawnRangeMax = 5;
            // }
            // else
            // {
            //     _spawnRangeMax = 6;
            // }

            // 테스트용 블록 교체 실행
            if (_test_blockOverrides != null && _test_blockOverrides.Count > 0)
            {
                OverrideBlocksForTesting(blockMover);
            }
        }

        private void OverrideBlocksForTesting(BlockMover blockMover)
        {
            foreach (var overrideData in _test_blockOverrides)
            {
                int x = overrideData.x;
                int y = overrideData.y;
                GemType gemType = overrideData.gemType;

                if (y < 0 || y >= GameBoardData.Height || x < 0 || x >= GameBoardData.Width) continue;
                if (!GameBoardData.BlockPlate.BlockPlateArray[y, x]) continue;

                Block oldBlock = GameBoardData.GetBlock(x, y);
                if (gemType != GemType.Dust && gemType != GemType.Syrup && oldBlock != null && oldBlock.BlockInstance != null)
                {
                    PooledObject pooledObject = oldBlock.BlockInstance.GetComponent<PooledObject>();

                    if (pooledObject != null)
                    {
                        pooledObject.ReturnToPool();
                    }

                    else
                    {
                        Destroy(oldBlock.BlockInstance);
                    }
                }

                if (gemType == GemType.Random)
                {
                    gemType = (GemType)Random.Range(0, 6); // 0~5 사이의 GemType을 바로 생성
                    GameBoardData.BlockArray[y, x] = new Block()
                    {
                        GemType = gemType,
                    };
                }
                else if (gemType == GemType.Dust)
                {
                    GameBoardData.OverlayArray[y, x] = new Dust(x, y);
                }
                else if (gemType == GemType.Syrup)
                {
                    GameBoardData.OverlayArray[y, x] = new Syrup(x, y);
                }
                else if (gemType == GemType.Ice)
                {
                    GameBoardData.BlockArray[y, x] = new Block()
                    {
                        GemType = (GemType)Random.Range(0, 6),
                    };

                    GameBoardData.BlockArray[y, x].IsNormal = false;
                    GameBoardData.BlockArray[y, x].CanMove = false;

                    GameBoardData.OverlayArray[y, x] = new Ice(x, y);
                }
                else if (gemType == GemType.DonutBag) GameBoardData.BlockArray[y, x] = new DonutBag(x, y);
                else if (gemType == GemType.Coin) GameBoardData.BlockArray[y, x] = new Coin(x, y);
                else if (gemType == GemType.GiftBox) GameBoardData.BlockArray[y, x] = new GiftBox(x, y);
                else if (gemType == GemType.Egg) GameBoardData.BlockArray[y, x] = new Egg(x, y);
                else if (gemType == GemType.FlourBag) GameBoardData.BlockArray[y, x] = new FlourBag(GameBoardData.BlockArray, x, y);
                else if (gemType == GemType.Flour_s) { }
                else
                {
                    GameBoardData.BlockArray[y, x] = new Block()
                    {
                        GemType = gemType,
                    };
                }

                Vector3 position = blockMover.GridToWorld(new Vector2Int(x, y), GameBoardData.Width, GameBoardData.Height);

                if (gemType == GemType.Dust || gemType == GemType.Syrup)
                {
                    GameObject blockPrefab = GetBlockPrefab((int)gemType);
                    if (blockPrefab != null)
                        GameBoardData.OverlayArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                }
                else if (gemType == GemType.Ice)
                {
                    GameObject overlayPrefab = GetBlockPrefab((int)GemType.Ice);
                    if (overlayPrefab != null)
                        GameBoardData.OverlayArray[y, x].BlockInstance = Instantiate(overlayPrefab, position, Quaternion.identity);
                    GameObject blockPrefabIce = GetBlockPrefab((int)GameBoardData.BlockArray[y, x].GemType);
                    if (blockPrefabIce != null)
                        GameBoardData.BlockArray[y, x].BlockInstance = BlockPool.GetObject().gameObject;
                    GameBoardData.BlockArray[y, x].BlockInstance.transform.position = position;
                }
                else
                {
                    if (gemType < GemType.Milk)
                    {
                        PooledObject pooledObject = BlockPool.GetObject();
                        SpriteRenderer spriteRenderer = pooledObject.GetComponent<SpriteRenderer>();

                        if (spriteRenderer != null && (int)gemType < normalBlockSprites.Count)
                        {
                            spriteRenderer.sprite = normalBlockSprites[(int)gemType];
                        }

                        pooledObject.transform.position = position;
                        pooledObject.transform.SetParent(null);
                        GameBoardData.BlockArray[y, x].BlockInstance = pooledObject.gameObject;
                    }
                    else
                    {
                        GameObject blockPrefabSpecial = GetBlockPrefab((int)gemType);
                        if (blockPrefabSpecial != null)
                        {
                            if (gemType == GemType.FlourBag)
                            {
                                Debug.Log("FlourBag 생성");
                                position += new Vector3(0.5f, 0.5f, 0); // FlourBag는 중앙 정렬이 아니므로 위치 보정
                                GameBoardData.BlockArray[y, x].BlockInstance = Instantiate(blockPrefabSpecial, position, Quaternion.identity);
                            }
                            else
                            {
                                GameBoardData.BlockArray[y, x].BlockInstance = Instantiate(blockPrefabSpecial, position, Quaternion.identity);
                            }
                        }
                    }

                    if (GameBoardData.BlockArray[y, x].GemType == GemType.Ice)
                    {
                        Debug.Log("Ice블럭인가?" + (GameBoardData.BlockArray[y, x] is Ice));
                        if (GameBoardData.BlockArray[y, x] is Ice iceBlock && iceBlock.BlockInstance != null)
                        {
                            iceBlock.IceObject = iceBlock.BlockInstance;
                            Debug.Log("IceObject 설정됨:" + (iceBlock.IceObject != null));
                        }
                    }

                    GameBoardData.SetBlock(x, y, GameBoardData.BlockArray[y, x]);

                }
            }
        }

        /// <summary>
        /// 보드 데이터에 따라 모든 블록의 게임 오브젝트를 생성하고 배치합니다.
        /// </summary>
        public void DrawAllBlocks(BlockMover blockMover)
        {
            for (int x = 0; x < GameBoardData.Width; x++)
            {
                for (int y = 0; y < GameBoardData.BlockArray.GetLength(0); y++)
                {
                    Block block = GameBoardData.GetBlock(x, y);
                    if (block != null && block.BlockInstance == null)
                    {
                        Vector3 position = blockMover.GridToWorld(new Vector2Int(x, y), GameBoardData.Width, GameBoardData.Height);

                        if (block.GemType < GemType.Milk)
                        {
                            PooledObject pooledObject = BlockPool.GetObject();
                            pooledObject.transform.localScale = Vector3.one;
                            SpriteRenderer spriteRenderer = pooledObject.GetComponent<SpriteRenderer>();

                            if (spriteRenderer != null && (int)block.GemType < normalBlockSprites.Count)
                            {
                                spriteRenderer.sprite = normalBlockSprites[(int)block.GemType];
                            }

                            pooledObject.transform.position = position;
                            pooledObject.transform.SetParent(null);
                            block.BlockInstance = pooledObject.gameObject;
                        }
                        else
                        {
                            GameObject blockPrefabSpecial = GetBlockPrefab((int)block.GemType);
                            if (blockPrefabSpecial != null)
                            {
                                if (block.GemType == GemType.FlourBag)
                                {
                                    Debug.Log("FlourBag 생성");
                                    position += new Vector3(0.5f, 0.5f, 0); // FlourBag는 중앙 정렬이 아니므로 위치 보정
                                    block.BlockInstance = Instantiate(blockPrefabSpecial, position, Quaternion.identity);
                                }
                                else
                                {
                                    block.BlockInstance = Instantiate(blockPrefabSpecial, position, Quaternion.identity);
                                }
                            }
                        }


                        if (block.GemType > GemType.Oven)
                        {
                            // 방해 블록일때 설정
                            block.IsObstacle = true;
                            block.IsNormal = false;

                            if (block.GemType == GemType.Coin || block.GemType == GemType.GiftBox)
                            {
                                block.CanMove = true;
                            }
                            else
                            {
                                block.CanMove = false;
                            }
                        }
                        else if (block.GemType < GemType.Dust && block.GemType > GemType.Sugar)
                        {
                            // 특수 블록일때 설정
                            block.IsObstacle = false;
                            block.IsNormal = false;
                        }

                        // 블록 마스크 그리기
                        if (_blockMaskPrefab != null)
                        {
                            if (y < GameBoardData.Height && GameBoardData.BlockPlate.BlockPlateArray[y, x])
                            {
                                GameObject maskInstance = Instantiate(_blockMaskPrefab, position, Quaternion.identity, transform);
                                GameBoardData.BlockMask[y, x] = maskInstance;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 보드 데이터에 따라 모든 오버레이 블록의 게임 오브젝트를 생성하고 배치합니다.
        /// </summary>
        public void DrawAllOverlayBlocks(BlockMover blockMover)
        {
            for (int x = 0; x < GameBoardData.Width; x++)
            {
                for (int y = 0; y < GameBoardData.Height; y++)
                {
                    Block block = GameBoardData.GetOverlayBlock(x, y);
                    if (block != null && block.BlockInstance == null)
                    {
                        block.IsObstacle = true;
                        block.IsNormal = false;
                        block.CanMove = false;

                        if (block.GemType == GemType.Ice)
                        {
                            Block normalBlock = GameBoardData.GetBlock(x, y);
                            normalBlock.CanMove = false; // 얼음 위의 일반 블록은 움직일 수 없습니다.
                            normalBlock.IsNormal = false;
                        }

                        Vector3 position = blockMover.GridToWorld(new Vector2Int(x, y), GameBoardData.Width, GameBoardData.Height);
                        GameObject blockPrefab = GetBlockPrefab((int)block.GemType);
                        if (blockPrefab != null)
                        {
                            block.BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                        }
                    }
                }
            }
        }

        #region 블럭 관리 (리필 로직)

        // 모래 흐름 로직을 위한 헬퍼 메서드
        private bool IsWellBelowObstacle(int x, int y)
        {
            for (int i = y + 1; i < GameBoardData.BlockArray.GetLength(0); i++)
            {
                if (GameBoardData.GetBlock(x, i) != null)
                {
                    return !GameBoardData.GetBlock(x, i).CanMove; // CanMove가 false인 블록이 장애물 역할을 함
                }
            }
            return false;
        }

        public IEnumerator RefillBoardCoroutine(BlockMover blockMover)
        {
            if (_isRefilling) yield break;
            _isRefilling = true;

            yield return new WaitForSeconds(0.1f);

            while (true)
            {
                bool activityThisStep = false;
                bool[,] movedToThisTick = new bool[GameBoardData.Height, GameBoardData.Width];

                // --- 1순위: 가속 낙하 (단계별 푸시 방식) ---
                bool[,] movedFlags = new bool[GameBoardData.BlockArray.GetLength(0), GameBoardData.Width];

                // 1a. 수직 낙하: 모든 블록이 아래로 떨어지려는 힘을 계산합니다.
                for (int y = 1; y < GameBoardData.BlockArray.GetLength(0); y++)
                {
                    for (int x = 0; x < GameBoardData.Width; x++)
                    {
                        Block block = GameBoardData.GetBlock(x, y);

                        if (block != null && block.CanMove)
                        {
                            // 1. 이 블록이 도달할 수 있는 가장 낮은 빈칸을 찾습니다.
                            int lowestPossibleY = y;
                            for (int k = y - 1; k >= 0; k--)
                            {
                                if (GameBoardData.GetBlock(x, k) != null)
                                {
                                    lowestPossibleY = k + 1;
                                    break;
                                }
                                lowestPossibleY = k;
                            }

                            // 2. 원래 위치와 가장 낮은 위치 사이에서, 가장 높은 유효 플레이트를 목적지로 설정합니다.
                            int destY = y;
                            for (int k = y - 1; k >= lowestPossibleY; k--)
                            {
                                if (k < GameBoardData.Height && GameBoardData.BlockPlate.BlockPlateArray[k, x])
                                {
                                    destY = k; // 착지할 유효한 플레이트를 찾았습니다.
                                    break;
                                }
                            }

                            if (destY != y)
                            {
                                // 목적지가 이미 다른 블록에 의해 채워졌는지 확인
                                if (movedFlags[destY, x]) continue;

                                GameBoardData.SetBlock(x, destY, block);
                                GameBoardData.SetBlock(x, y, null);

                                if (GameBoardData.BlockArray[destY, x] is ObstacleBlock ob)
                                {
                                    ob.X = x;
                                    ob.Y = destY;
                                    ob.OnLand(destY);
                                }

                                movedFlags[destY, x] = true; // 이 틱에서 이동했음을 표시
                                activityThisStep = true;
                                StartCoroutine(MoveBlockCoroutine(block, blockMover.GridToWorld(new Vector2Int(x, destY), GameBoardData.Width, GameBoardData.Height), _stepDuration));
                            }
                        }
                    }
                }

                // 1b. 대각선 낙하: 장애물에 의해 경로가 막혔을 때의 원래 로직을 복원합니다.
                for (int y = 0; y < GameBoardData.Height; y++)
                {
                    for (int x = 0; x < GameBoardData.Width; x++)
                    {
                        // 이 칸이 비어있고, 이번 틱에 다른 블록이 채워지지 않았는지 확인합니다.
                        if (GameBoardData.GetBlock(x, y) == null && GameBoardData.BlockPlate.BlockPlateArray[y, x] && !movedFlags[y, x])
                        {
                            // 바로 위에 움직일 수 없는 블록(장애물)이 있는지 확인합니다.
                            Block blockAbove = GameBoardData.GetBlock(x, y + 1);
                            if (blockAbove != null && !blockAbove.CanMove)
                            {
                                // 왼쪽 대각선 위에서 이동할 블록을 찾습니다.
                                Block leftDiagonalBlock = (x > 0) ? GameBoardData.GetBlock(x - 1, y + 1) : null;
                                if (leftDiagonalBlock != null && leftDiagonalBlock.CanMove)
                                {
                                    // 수직 낙하가 먼저 실행되므로, GetBlock이 null이 아니라는 것은 해당 블록이 아직 움직이지 않았다는 의미입니다.
                                    GameBoardData.SetBlock(x, y, leftDiagonalBlock);
                                    GameBoardData.SetBlock(x - 1, y + 1, null);
                                    if (GameBoardData.BlockArray[y + 1, x - 1] is ObstacleBlock ob)
                                    {
                                        ob.X = x - 1;
                                        ob.Y = y + 1;
                                        ob.OnLand(y);
                                    }
                                    movedFlags[y, x] = true;
                                    activityThisStep = true;
                                    StartCoroutine(MoveBlockCoroutine(leftDiagonalBlock, blockMover.GridToWorld(new Vector2Int(x, y), GameBoardData.Width, GameBoardData.Height), _stepDuration));
                                }
                                // 왼쪽에서 못찾았으면, 오른쪽 대각선 위에서 이동할 블록을 찾습니다.
                                else
                                {
                                    Block rightDiagonalBlock = (x < GameBoardData.Width - 1) ? GameBoardData.GetBlock(x + 1, y + 1) : null;
                                    if (rightDiagonalBlock != null && rightDiagonalBlock.CanMove)
                                    {
                                        GameBoardData.SetBlock(x, y, rightDiagonalBlock);
                                        GameBoardData.SetBlock(x + 1, y + 1, null);
                                        if (GameBoardData.BlockArray[y + 1, x + 1] is ObstacleBlock ob)
                                        {
                                            ob.X = x + 1;
                                            ob.Y = y + 1;
                                            ob.OnLand(y);
                                        }
                                        movedFlags[y, x] = true;
                                        activityThisStep = true;
                                        StartCoroutine(MoveBlockCoroutine(rightDiagonalBlock, blockMover.GridToWorld(new Vector2Int(x, y), GameBoardData.Width, GameBoardData.Height), _stepDuration));
                                    }
                                }
                            }
                        }
                    }
                }

                // --- 2순위: 모래 흐름 ---
                if (!activityThisStep)
                {
                    for (int y = GameBoardData.Height - 1; y >= 1; y--)
                    {
                        for (int x = 0; x < GameBoardData.Width; x++)
                        {
                            Block currentBlock = GameBoardData.GetBlock(x, y);
                            if (currentBlock != null && currentBlock.CanMove)
                            {
                                Block blockAbove = GameBoardData.GetBlock(x, y + 1);
                                if ((blockAbove == null || !blockAbove.CanMove) && GameBoardData.GetBlock(x, y - 1) != null)
                                {
                                    if (x > 0 && GameBoardData.GetBlock(x - 1, y) == null && GameBoardData.GetBlock(x - 1, y - 1) == null && !movedToThisTick[y - 1, x - 1] && IsWellBelowObstacle(x - 1, y - 1))
                                    {
                                        GameBoardData.SetBlock(x - 1, y - 1, currentBlock);
                                        GameBoardData.SetBlock(x, y, null);
                                        if (GameBoardData.BlockArray[y - 1, x - 1] is ObstacleBlock ob)
                                        {
                                            ob.X = x - 1;
                                            ob.Y = y - 1;
                                            ob.OnLand(y - 1);
                                        }
                                        movedToThisTick[y - 1, x - 1] = true;
                                        activityThisStep = true;
                                        StartCoroutine(MoveBlockCoroutine(currentBlock, blockMover.GridToWorld(new Vector2Int(x - 1, y - 1), GameBoardData.Width, GameBoardData.Height), _stepDuration));
                                        continue;
                                    }
                                    if (x < GameBoardData.Width - 1 && GameBoardData.GetBlock(x + 1, y) == null && GameBoardData.GetBlock(x + 1, y - 1) == null && !movedToThisTick[y - 1, x + 1] && IsWellBelowObstacle(x + 1, y - 1))
                                    {
                                        GameBoardData.SetBlock(x + 1, y - 1, currentBlock);
                                        GameBoardData.SetBlock(x, y, null);
                                        if (GameBoardData.BlockArray[y - 1, x + 1] is ObstacleBlock ob)
                                        {
                                            ob.X = x + 1;
                                            ob.Y = y - 1;
                                            ob.OnLand(y - 1);
                                        }
                                        movedToThisTick[y - 1, x + 1] = true;
                                        activityThisStep = true;
                                        StartCoroutine(MoveBlockCoroutine(currentBlock, blockMover.GridToWorld(new Vector2Int(x + 1, y - 1), GameBoardData.Width, GameBoardData.Height), _stepDuration));
                                    }
                                }
                            }
                        }
                    }
                }

                // --- 3순위: 새 블록 생성 (대기열 채우기) ---
                if (RefillWaitingQueue(blockMover))
                {
                    activityThisStep = true;
                }

                // 이번 틱에 어떤 활동이라도 있었으면, 애니메이션 시간을 기다립니다.
                if (activityThisStep)
                {
                    yield return new WaitForSeconds(_stepDuration);
                }
                else
                {
                    break;
                }
            }

            _isRefilling = false;
        }

        private bool RefillWaitingQueue(BlockMover blockMover)
        {
            bool refilled = false;
            int queueRow = GameBoardData.Height;
            for (int x = 0; x < GameBoardData.Width; x++)
            {
                if (GameBoardData.GetBlock(x, queueRow) == null)
                {
                    if (_blockWaitingQueue[x].Count == 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            _blockWaitingQueue[x].Enqueue(new Block { GemType = (GemType)Random.Range(0, _spawnRangeMax) });
                        }
                    }

                    Block newBlock = _blockWaitingQueue[x].Dequeue();
                    GameBoardData.SetBlock(x, queueRow, newBlock);

                    Vector3 position = blockMover.GridToWorld(new Vector2Int(x, queueRow), GameBoardData.Width, GameBoardData.Height);

                    if (newBlock.GemType < GemType.Milk)
                    {
                        PooledObject pooledObject = BlockPool.GetObject();
                        pooledObject.transform.localScale = Vector3.one;

                        SpriteRenderer spriteRenderer = pooledObject.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null && (int)newBlock.GemType < normalBlockSprites.Count)
                        {
                            spriteRenderer.sprite = normalBlockSprites[(int)newBlock.GemType];
                        }
                        newBlock.BlockInstance = pooledObject.gameObject;
                        pooledObject.transform.SetParent(null);
                        newBlock.BlockInstance.transform.position = position;
                    }
                    else
                    {
                        GameObject blockPrefab = GetBlockPrefab((int)newBlock.GemType);
                        if (blockPrefab != null)
                        {
                            newBlock.BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
                        }
                    }


                    refilled = true;
                }
            }
            return refilled;
        }

        private Vector2Int FindBlockPosition(Block blockToFind)
        {
            for (int y = 0; y < GameBoardData.Height + 1; y++)
                for (int x = 0; x < GameBoardData.Width; x++)
                    if (GameBoardData.GetBlock(x, y) == blockToFind) return new Vector2Int(x, y);
            return new Vector2Int(-1, -1);
        }

        private IEnumerator MoveBlockCoroutine(Block block, Vector3 targetPosition, float duration)
        {
            if (block == null || block.BlockInstance == null) yield break;
            Vector3 startPosition = block.BlockInstance.transform.position;
            float time = 0;
            while (time < duration)
            {
                if (block.BlockInstance == null) yield break;
                block.BlockInstance.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            if (block.BlockInstance != null) block.BlockInstance.transform.position = targetPosition;
        }

        public void CheckAndClearDestroyedBlocks()
        {
            for (int x = 0; x < GameBoardData.Width; x++)
            {
                for (int y = 0; y < GameBoardData.Height; y++)
                {
                    Block block = GameBoardData.GetBlock(x, y);

                    if (block != null && block.BlockInstance == null)
                    {
                        if (block is FlourBag_s bag_S && bag_S.Owner.CurrentHP > 0)
                        {
                            continue;
                        }

                        GameBoardData.SetBlock(x, y, null);
                    }
                }
            }
        }

        private Block CreateNewBlock(GemType gemType)
        {
            var newBlock = new Block { GemType = gemType };

            if (gemType > GemType.Sugar && gemType < GemType.Dust)
            {
                newBlock.IsNormal = false;
                newBlock.CanMove = true;
            }
            else if (gemType >= GemType.Dust)
            {
                newBlock.IsNormal = false;
                newBlock.IsObstacle = true;
                switch (gemType)
                {
                    case GemType.Dust:
                    case GemType.Syrup:
                    case GemType.Ice:
                    case GemType.DonutBag:
                    case GemType.FlourBag:
                    case GemType.Flour_s:
                        newBlock.CanMove = false;
                        break;
                    default:
                        newBlock.CanMove = true;
                        break;
                }
            }

            return newBlock;
        }


        /// <summary>
        /// 특정 위치에 특정 종류의 블록을 생성하고 배치합니다.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="gemType"></param>
        /// <param name="blockMover"></param>
        public void SpawnBlock(int x, int y, GemType gemType, BlockMover blockMover)
        {
            Vector3 position = blockMover.GridToWorld(new Vector2Int(x, y), GameBoardData.Width, GameBoardData.Height);
            Block newBlock = CreateNewBlock(gemType);

            if (gemType < GemType.Milk)
            {
                PooledObject pooledObject = BlockPool.GetObject();
                pooledObject.transform.localScale = Vector3.one;
                SpriteRenderer spriteRenderer = pooledObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && (int)gemType < normalBlockSprites.Count)
                {
                    spriteRenderer.sprite = normalBlockSprites[(int)gemType];
                }
                pooledObject.transform.position = position;
                pooledObject.transform.SetParent(null);
                newBlock.BlockInstance = pooledObject.gameObject;
            }
            else
            {
                GameObject blockPrefabSpecial = GetBlockPrefab((int)gemType);
                if (blockPrefabSpecial == null) return;
                GameObject blockInstanceSpecial = Instantiate(blockPrefabSpecial, position, Quaternion.identity);
                newBlock.BlockInstance = blockInstanceSpecial;
            }

            GameBoardData.SetBlock(x, y, newBlock);
        }

        /// <summary>
        /// 랜덤한 위치에 특수 블록을 생성합니다.
        /// </summary>
        /// <param name="gemType"></param>
        public void RandomPosSpawnSpecialBlock(GemType gemType)
        {
            List<Vector2Int> validPositions = new List<Vector2Int>();
            int height = GameBoardData.Height;
            int width = GameBoardData.Width;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Block block = GameBoardData.GetBlock(x, y);
                    if (block != null && !block.IsObstacle && !(block.GemType > GemType.Sugar && block.GemType < GemType.Dust))
                    {
                        validPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (validPositions.Count > 0)
            {
                int randomIndex = Random.Range(0, validPositions.Count);
                Vector2Int spawnPos = validPositions[randomIndex];

                Block oldBlock = GameBoardData.GetBlock(spawnPos.x, spawnPos.y);
                if (oldBlock != null && oldBlock.BlockInstance != null)
                {
                    PooledObject pooledObject = oldBlock.BlockInstance.GetComponent<PooledObject>();

                    if (pooledObject != null)
                    {
                        pooledObject.ReturnToPool();
                    }
                }

                SpawnBlock(spawnPos.x, spawnPos.y, gemType, BoardManager.Instance.BlockMover);
            }
            else
            {
                Debug.LogWarning("RandomPosSpawnSpecialBlock: 특수 블록을 생성할 유효한 위치를 찾지 못했습니다.");
            }
        }


        /// <summary>
        /// 보드의 모든 일반 블록을 셔플합니다.
        /// 데이터 셔플이 끝나면 기존 블록 오브젝트를 모두 풀에 반환하고, 새로 그립니다.
        /// </summary>
        /// <param name="boardManager"></param>
        public void Shuffle(BoardManager boardManager)
        {
            DataShuffle(boardManager, 100);

            foreach (Block block in boardManager.Spawner.GameBoardData.BlockArray)
            {
                if (block != null && block.BlockInstance != null && block.IsNormal)
                {
                    PooledObject pooledObject = block.BlockInstance.GetComponent<PooledObject>();
                    if (pooledObject != null)
                    {
                        pooledObject.ReturnToPool();
                    }
                    block.BlockInstance = null;
                }
            }

            DrawAllBlocks(boardManager.BlockMover);
        }

        /// <summary>
        /// 보드의 모든 일반 블록을 셔플합니다. 매치가 없고, 매치 가능성이 있는 상태가 될 때까지 최대 maxAttempts 횟수만큼 시도합니다.
        /// 데이터만 셔플을 진행합니다.
        /// </summary>
        /// <param name="boardManager"></param>
        /// <param name="maxAttempts"></param>
        private void DataShuffle(BoardManager boardManager, int maxAttempts)
        {
            List<Block> normalBlocks = new List<Block>();
            List<Vector2Int> normalBlockPositions = new List<Vector2Int>();
            int maxTries = maxAttempts;
            int tries = 0;

            while (tries < maxTries)
            {
                normalBlocks.Clear();
                normalBlockPositions.Clear();

                for (int y = 0; y < GameBoardData.Height; y++)
                {
                    for (int x = 0; x < GameBoardData.Width; x++)
                    {
                        if (GameBoardData.BlockPlate.BlockPlateArray[y, x])
                        {
                            Block block = GameBoardData.GetBlock(x, y);
                            if (block != null && block.GemType <= GemType.Sugar)
                            {
                                normalBlocks.Add(block);
                                normalBlockPositions.Add(new Vector2Int(x, y));
                            }
                        }
                    }
                }

                for (int i = 0; i < normalBlocks.Count; i++)
                {
                    int randomIndex = Random.Range(i, normalBlocks.Count);
                    Block temp = normalBlocks[i];
                    normalBlocks[i] = normalBlocks[randomIndex];
                    normalBlocks[randomIndex] = temp;
                }

                for (int i = 0; i < normalBlockPositions.Count; i++)
                {
                    Vector2Int pos = normalBlockPositions[i];
                    GameBoardData.SetBlock(pos.x, pos.y, normalBlocks[i]);
                }

                if (!boardManager.MatchChecker.AllBlockMatchCheck(boardManager))
                {
                    if (boardManager.MatchChecker.AllBlockMatchPossibilityCheck(boardManager, out _))
                    {
                        break;
                    }
                }

                tries++;
                if (tries >= maxTries)
                {
                    Debug.LogWarning($"DataShuffle: {maxTries}회 셔플 후에도 보드가 계속 매치된 상태이거나, 매치 가능한 조합을 찾지 못했습니다.");
                    break;
                }
            }

            Debug.Log($"DataShuffle: {tries}회 만에 셔플 완료.");
        }

        public Block SetRandomBlock(int x, int y)
        {
            GemType gemType = (GemType)Random.Range(0, _spawnRangeMax);
            Block newBlock = CreateNewBlock(gemType);
            GameBoardData.SetBlock(x, y, newBlock);
            return newBlock;
        }

        public void SpawnRandomBlock(int x, int y)
        {
            GemType gemType = (GemType)Random.Range(0, _spawnRangeMax);
            Block newBlock = CreateNewBlock(gemType);
            GameObject blockPrefab = GetBlockPrefab((int)gemType);
            if (blockPrefab == null) return;
            Vector3 position = BoardManager.Instance.BlockMover.GridToWorld(new Vector2Int(x, y), GameBoardData.Width, GameBoardData.Height);
            GameObject blockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
            newBlock.BlockInstance = blockInstance;
            GameBoardData.SetBlock(x, y, newBlock);
        }

        private GameObject GetBlockPrefab(int blockType)
        {
            if (blockType >= 0 && blockType < _blockPrefabs.Count)
            {
                return _blockPrefabs[blockType];
            }
            return null;
        }

        public bool HasEmptyBlocks()
        {
            for (int x = 0; x < GameBoardData.Width; x++)
            {
                for (int y = 0; y < GameBoardData.Height; y++)
                {
                    if (GameBoardData.BlockPlate.BlockPlateArray[y, x] && GameBoardData.GetBlock(x, y) == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HasEmptyBlockObjects()
        {
            for (int x = 0; x < GameBoardData.Width; x++)
            {
                for (int y = 0; y < GameBoardData.Height; y++)
                {
                    if (GameBoardData.BlockPlate.BlockPlateArray[y, x] && GameBoardData.GetBlock(x, y) != null && GameBoardData.GetBlock(x, y).BlockInstance == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CanBlockMoveInArray()
        {
            if (GameBoardData == null) return false;

            // --- 1a. 수직 낙하 (가속 낙하) 확인 ---
            for (int y = 1; y < GameBoardData.BlockArray.GetLength(0); y++)
            {
                for (int x = 0; x < GameBoardData.Width; x++)
                {
                    Block block = GameBoardData.GetBlock(x, y);
                    if (block != null && block.CanMove)
                    {
                        int lowestPossibleY = y;
                        for (int k = y - 1; k >= 0; k--)
                        {
                            if (GameBoardData.GetBlock(x, k) != null) { lowestPossibleY = k + 1; break; }
                            lowestPossibleY = k;
                        }
                        int destY = y;
                        for (int k = y - 1; k >= lowestPossibleY; k--)
                        {
                            if (k < GameBoardData.Height && GameBoardData.BlockPlate.BlockPlateArray[k, x]) { destY = k; break; }
                        }
                        if (destY != y) return true;
                    }
                }
            }

            // --- 1b. 대각선 낙하 (장애물 회피) 확인 ---
            for (int y = 0; y < GameBoardData.Height; y++)
            {
                for (int x = 0; x < GameBoardData.Width; x++)
                {
                    if (GameBoardData.GetBlock(x, y) == null && GameBoardData.BlockPlate.BlockPlateArray[y, x])
                    {
                        Block blockAbove = GameBoardData.GetBlock(x, y + 1);
                        if (blockAbove != null && !blockAbove.CanMove)
                        {
                            Block leftDiagonalBlock = (x > 0) ? GameBoardData.GetBlock(x - 1, y + 1) : null;
                            if (leftDiagonalBlock != null && leftDiagonalBlock.CanMove) return true;
                            Block rightDiagonalBlock = (x < GameBoardData.Width - 1) ? GameBoardData.GetBlock(x + 1, y + 1) : null;
                            if (rightDiagonalBlock != null && rightDiagonalBlock.CanMove) return true;
                        }
                    }
                }
            }

            // --- 2순위: 모래 흐름 확인 ---
            for (int y = GameBoardData.Height - 1; y >= 1; y--)
            {
                for (int x = 0; x < GameBoardData.Width; x++)
                {
                    Block currentBlock = GameBoardData.GetBlock(x, y);
                    if (currentBlock != null && currentBlock.CanMove)
                    {
                        Block blockAbove = GameBoardData.GetBlock(x, y + 1);
                        if ((blockAbove == null || !blockAbove.CanMove) && GameBoardData.GetBlock(x, y - 1) != null)
                        {
                            if (x > 0 && GameBoardData.GetBlock(x - 1, y) == null && GameBoardData.GetBlock(x - 1, y - 1) == null && IsWellBelowObstacle(x - 1, y - 1)) return true;
                            if (x < GameBoardData.Width - 1 && GameBoardData.GetBlock(x + 1, y) == null && GameBoardData.GetBlock(x + 1, y - 1) == null && IsWellBelowObstacle(x + 1, y - 1)) return true;
                        }
                    }
                }
            }

            // --- 3순위: 새 블록 생성 확인 ---
            int queueRow = GameBoardData.Height;
            for (int x = 0; x < GameBoardData.Width; x++)
            {
                if (GameBoardData.GetBlock(x, queueRow) == null)
                {
                    return true; // 대기열에 새 블록이 채워질 수 있음
                }
            }

            return false; // 어떤 활동도 불가능
        }

        public int GetMaxDonutSpawnRange()
        {
            return _spawnRangeMax;
        }

        public IEnumerator SpawnWithAnimation(int x, int y, GemType gemType)
        {
            BoardManager.Instance.IsWaitingForAnimation = true;
            Vector3 position = BoardManager.Instance.BlockMover.GridToWorld(new Vector2Int(x, y), GameBoardData.Width, GameBoardData.Height);
            Block newBlock = CreateNewBlock(gemType);

            if (gemType < GemType.Milk)
            {
                PooledObject pooledObject = BlockPool.GetObject();
                SpriteRenderer spriteRenderer = pooledObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && (int)gemType < normalBlockSprites.Count)
                {
                    spriteRenderer.sprite = normalBlockSprites[(int)gemType];
                }
                pooledObject.transform.position = position;
                pooledObject.transform.SetParent(null);
                newBlock.BlockInstance = pooledObject.gameObject;
            }
            else
            {
                GameObject blockPrefabSpecial = GetBlockPrefab((int)gemType);
                if (blockPrefabSpecial == null) yield break;
                GameObject blockInstanceSpecial = Instantiate(blockPrefabSpecial, position, Quaternion.identity);
                newBlock.BlockInstance = blockInstanceSpecial;
            }

            GameBoardData.SetBlock(x, y, newBlock);

            newBlock.BlockInstance.transform.localScale = Vector3.zero;
            float timer = 0f;

            while (timer < 0.1f)
            {
                timer += Time.deltaTime;
                newBlock.BlockInstance.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer / 0.1f);
                yield return null;
            }

            // 확실하게 최종 크기로 설정
            newBlock.BlockInstance.transform.localScale = Vector3.one;
            BoardManager.Instance.IsWaitingForAnimation = false;
        }

        #endregion


    }
}