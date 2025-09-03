using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCR;
using LHJ;
using SCR_B;
using TMPro;
using SCR_O;

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
        [Header("테스트용 블록 교체 설정")]
        [SerializeField] private List<BlockOverride> _test_blockOverrides;


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

            // 3. 모든 블록 GameObject 생성
            DrawAllBlocks(blockMover);
            DrawAllOverlayBlocks(blockMover);

            // 4. 초기 보드가 이미 매치된 상태이면, 매치되지 않은 상태가 될 때까지 셔플합니다. (최대 1000회)
            int shuffleTries = 0;
            int maxShuffleTries = 1000;
            while (boardManager.MatchChecker.AllBlockMatchCheck(boardManager) && shuffleTries < maxShuffleTries)
            {
                Shuffle(boardManager);
                shuffleTries++;
            }

            if (shuffleTries >= maxShuffleTries)
            {
                Debug.LogError("Initialize: 1000회 셔플 후에도 보드가 계속 매치된 상태입니다. 다른 해결 방법이 필요합니다.");
            }

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
                if (oldBlock != null && oldBlock.BlockInstance != null)
                {
                    Destroy(oldBlock.BlockInstance);
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
                    int donutNum = Random.Range(0, 6);
                    GameBoardData.BlockArray[y, x] = new Block()
                    {
                        GemType = (GemType)donutNum,
                    };
                }
                else if (gemType == GemType.Syrup)
                {
                    GameBoardData.OverlayArray[y, x] = new Syrup(x, y);
                    int donutNum = Random.Range(0, 6);
                    GameBoardData.BlockArray[y, x] = new Block()
                    {
                        GemType = (GemType)donutNum,
                    };
                }
                else if (gemType == GemType.Ice) GameBoardData.BlockArray[y, x] = new Ice(x, y);
                else if (gemType == GemType.DonutBag) GameBoardData.BlockArray[y, x] = new DonutBag(x, y);
                else if (gemType == GemType.Coin) GameBoardData.BlockArray[y, x] = new Coin(x, y);
                else if (gemType == GemType.GiftBox) GameBoardData.BlockArray[y, x] = new GiftBox(x, y);
                else if (gemType == GemType.Egg) GameBoardData.BlockArray[y, x] = new Egg(x, y);
                else if (gemType == GemType.Flour_s) { }
                else
                {
                    GameBoardData.BlockArray[y, x] = new Block()
                    {
                        GemType = gemType,
                    };
                }

                Vector3 position = blockMover.GridToWorld(new Vector2Int(x, y), GameBoardData.Width, GameBoardData.Height);
                GameObject blockPrefab = GetBlockPrefab((int)gemType);
                GameBoardData.BlockArray[y, x].BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);

                GameBoardData.SetBlock(x, y, GameBoardData.BlockArray[y, x]);

            }
        }

        /// <summary>
        /// 보드 데이터에 따라 모든 일반 블록의 게임 오브젝트를 생성하고 배치합니다.
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
                        GameObject blockPrefab = GetBlockPrefab((int)block.GemType);
                        if (blockPrefab != null)
                        {
                            block.BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
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
                                        movedToThisTick[y - 1, x - 1] = true;
                                        activityThisStep = true;
                                        StartCoroutine(MoveBlockCoroutine(currentBlock, blockMover.GridToWorld(new Vector2Int(x - 1, y - 1), GameBoardData.Width, GameBoardData.Height), _stepDuration));
                                        continue;
                                    }
                                    if (x < GameBoardData.Width - 1 && GameBoardData.GetBlock(x + 1, y) == null && GameBoardData.GetBlock(x + 1, y - 1) == null && !movedToThisTick[y - 1, x + 1] && IsWellBelowObstacle(x + 1, y - 1))
                                    {
                                        GameBoardData.SetBlock(x + 1, y - 1, currentBlock);
                                        GameBoardData.SetBlock(x, y, null);
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
                    // 어떤 활동도 없었으면 보드가 안정된 상태이므로 루프를 종료합니다.
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
                // 대기열 칸이 비어있으면 무조건 채웁니다.
                if (GameBoardData.GetBlock(x, queueRow) == null)
                {
                    // 백업 큐가 비어있으면 새로 채웁니다.
                    if (_blockWaitingQueue[x].Count == 0)
                    {
                        for (int i = 0; i < 5; i++) // 임의로 5개를 미리 생성
                        {
                            _blockWaitingQueue[x].Enqueue(new Block { GemType = (GemType)Random.Range(0, _spawnRangeMax + 1) });
                        }
                    }

                    // 백업 큐에서 블록을 꺼내 대기열에 배치
                    Block newBlock = _blockWaitingQueue[x].Dequeue();
                    GameBoardData.SetBlock(x, queueRow, newBlock);

                    // 게임 오브젝트 생성
                    Vector3 position = blockMover.GridToWorld(new Vector2Int(x, queueRow), GameBoardData.Width, GameBoardData.Height);
                    GameObject blockPrefab = GetBlockPrefab((int)newBlock.GemType);
                    if (blockPrefab != null)
                    {
                        newBlock.BlockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
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

                    // 데이터상으론 블록이 있지만, 실제 게임오브젝트는 파괴된 경우
                    if (block != null && block.BlockInstance == null)
                    {
                        // 데이터에서 블록을 제거합니다.
                        // 재료 추가, 스플래시 데미지 등 매치의 결과는 BoardMatchChecker에서 처리합니다.
                        GameBoardData.SetBlock(x, y, null);
                    }
                }
            }
        }

        private Block CreateNewBlock(GemType gemType)
        {
            var newBlock = new Block { GemType = gemType };

            // GemType에 따라 IsObstacle, CanMove 속성 설정
            if (gemType > GemType.Sugar && gemType < GemType.Dust) // 특수 블록
            {
                newBlock.IsNormal = false;
                newBlock.CanMove = true;
            }
            else if (gemType >= GemType.Dust) // 기타 방해물
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
                        newBlock.CanMove = false; // 움직일 수 없는 방해물
                        break;
                    default:
                        newBlock.CanMove = true; // 기본적으로 움직일 수 있는 방해물
                        break;
                }
            }

            return newBlock;
        }

        public void SpawnBlock(int x, int y, GemType gemType, BlockMover blockMover)
        {
            GameObject blockPrefab = GetBlockPrefab((int)gemType);
            if (blockPrefab == null) return;
            Vector3 position = blockMover.GridToWorld(new Vector2Int(x, y), GameBoardData.Width, GameBoardData.Height);
            GameObject blockInstance = Instantiate(blockPrefab, position, Quaternion.identity);

            Block newBlock = CreateNewBlock(gemType);
            newBlock.BlockInstance = blockInstance;

            GameBoardData.SetBlock(x, y, newBlock);
        }

        public void RandomPosSpawnSpecialBlock(GemType gemType)
        {
            List<Vector2Int> validPositions = new List<Vector2Int>();
            int height = GameBoardData.Height;
            int width = GameBoardData.Width;

            // 1. 일반 블록(장애물 제외)이 있는 모든 위치를 찾습니다.
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

            // 2. 유효한 위치가 있으면, 그 중 하나를 골라 기존 블록을 파괴하고 새 특수 블록을 생성합니다.
            if (validPositions.Count > 0)
            {
                int randomIndex = Random.Range(0, validPositions.Count);
                Vector2Int spawnPos = validPositions[randomIndex];

                // 기존 블록의 게임 오브젝트를 파괴합니다.
                Block oldBlock = GameBoardData.GetBlock(spawnPos.x, spawnPos.y);
                if (oldBlock != null && oldBlock.BlockInstance != null)
                {
                    Destroy(oldBlock.BlockInstance);
                }

                // 새로운 특수 블록을 생성합니다.
                SpawnBlock(spawnPos.x, spawnPos.y, gemType, BoardManager.Instance.BlockMover);
            }
            else
            {
                Debug.LogWarning("RandomPosSpawnSpecialBlock: 특수 블록을 생성할 유효한 위치를 찾지 못했습니다.");
            }
        }

        public void Shuffle(BoardManager boardManager)
        {
            int maxTries = 100;
            int tries = 0;

            List<Block> normalBlocks = new List<Block>();
            List<Vector2Int> normalBlockPositions = new List<Vector2Int>();

            while (tries < maxTries)
            {
                normalBlocks.Clear();
                normalBlockPositions.Clear();

                // 1. 셔플할 일반 블록만 수집합니다.
                for (int y = 0; y < GameBoardData.Height; y++)
                {
                    for (int x = 0; x < GameBoardData.Width; x++)
                    {
                        Block block = GameBoardData.GetBlock(x, y);
                        if (block != null && !block.IsObstacle)
                        {
                            normalBlocks.Add(block);
                            normalBlockPositions.Add(new Vector2Int(x, y));
                        }
                    }
                }

                // 2. 수집한 일반 블록 리스트를 섞습니다.
                for (int i = 0; i < normalBlocks.Count; i++)
                {
                    int randomIndex = Random.Range(i, normalBlocks.Count);
                    Block temp = normalBlocks[i];
                    normalBlocks[i] = normalBlocks[randomIndex];
                    normalBlocks[randomIndex] = temp;
                }

                // 3. 섞인 블록을 원래 위치에 다시 배치합니다.
                for (int i = 0; i < normalBlockPositions.Count; i++)
                {
                    Vector2Int pos = normalBlockPositions[i];
                    GameBoardData.SetBlock(pos.x, pos.y, normalBlocks[i]);
                }

                // 4. 매치 가능한 블록이 있는지 확인합니다.
                if (boardManager.MatchChecker.AllBlockMatchPossibilityCheck(boardManager, out _))
                {
                    break; // 매치 가능하면 루프 탈출
                }

                tries++;
            }

            if (tries >= maxTries)
            {
                Debug.LogWarning("Shuffle: 100회 시도 후에도 매치 가능한 조합을 찾지 못했습니다.");
            }

            // 5. 화면을 갱신합니다: 셔플된 블록들의 기존 오브젝트를 파괴하고 다시 그립니다.
            foreach (Block block in normalBlocks)
            {
                if (block != null && block.BlockInstance != null)
                {
                    Destroy(block.BlockInstance);
                    block.BlockInstance = null; // 다시 그려지도록 인스턴스를 null로 설정
                }
            }

            DrawAllBlocks(boardManager.BlockMover);
        }

        public Block SetRandomBlock(int x, int y)
        {
            GemType gemType = (GemType)Random.Range(0, _spawnRangeMax + 1);
            Block newBlock = CreateNewBlock(gemType);
            GameBoardData.SetBlock(x, y, newBlock);
            return newBlock;
        }

        public void SpawnRandomBlock(int x, int y)
        {
            GemType gemType = (GemType)Random.Range(0, _spawnRangeMax + 1);
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

            // RefillBoardCoroutine의 새 로직을 정확히 반영하여 이동 가능성을 체크합니다.

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

        #endregion
    }
}
