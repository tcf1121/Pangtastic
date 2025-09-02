using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCR;

namespace KDJ
{
    public class BlockSpawner : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private List<GameObject> _blockPrefabs = new List<GameObject>();
        [SerializeField] private float _stepDuration = 0.08f; // 한 스텝(한 칸 낙하)에 걸리는 시간
        [SerializeField] private int _spawnRangeMin = 1;
        [SerializeField] private int _spawnRangeMax = 6;

        public GameBoardData GameBoardData { get; private set; }
        public List<GemType> DestroyedBlocks { get; private set; } = new List<GemType>();

        private bool _isRefilling = false;

        /// <summary>
        /// 로드된 보드 데이터를 기반으로 Spawner를 초기화하고, 모든 블록을 생성합니다.
        /// </summary>
        public void Initialize(BoardManager boardManager, BoardData loadedBoardData, BlockPlate blockPlate, BlockMover blockMover)
        {
            GameBoardData = new GameBoardData(blockPlate);

            for (int y = 0; y < GameBoardData.Height; y++)
            {
                for (int x = 0; x < GameBoardData.Width; x++)
                {
                    // 1. 일반 블록 데이터 처리
                    Block loadedBlock = loadedBoardData.BlockArray[y, x];
                    if (loadedBlock != null)
                    {
                        if (loadedBlock.GemType == GemType.Random)
                        {
                            SpawnRandomBlock(x, y);
                        }
                        else
                        {
                            Block newBlock = new Block { GemType = loadedBlock.GemType };
                            GameBoardData.SetBlock(x, y, newBlock);
                        }
                    }

                    // 2. 오버레이 블록 데이터 처리
                    Block loadedOverlayBlock = loadedBoardData.OverlayArray[y, x];
                    if (loadedOverlayBlock != null)
                    {
                        Block newOverlayBlock = new Block { GemType = loadedOverlayBlock.GemType };
                        GameBoardData.SetOverlayBlock(x, y, newOverlayBlock);
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
        }

        /// <summary>
        /// 보드 데이터에 따라 모든 일반 블록의 게임 오브젝트를 생성하고 배치합니다.
        /// </summary>
        public void DrawAllBlocks(BlockMover blockMover)
        {
            for (int x = 0; x < GameBoardData.Width; x++)
            {
                for (int y = 0; y < GameBoardData.Height; y++)
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

        #region 블럭 관리 (하이브리드 리필 알고리즘)

        public IEnumerator RefillBoardCoroutine(BlockMover blockMover)
        {
            if (_isRefilling) yield break;
            _isRefilling = true;

            yield return new WaitForSeconds(0.1f); // 파괴 애니메이션을 위한 짧은 대기

            while (true)
            {
                // 1. 선계산: 다음 스텝에 움직일 블록들을 모두 찾음
                Dictionary<Block, Vector2Int> moves = FindNextStepMoves();

                // 2. 새 블록 생성: 최상단에 빈 공간이 있으면 새 블록을 생성하고 움직임 목록에 추가
                int newBlocks = SpawnNewBlocks(moves);

                // 3. 실행: 움직일 블록이 없으면 리필 종료
                if (moves.Count == 0) break;

                // 4. 데이터 업데이트 및 애니메이션
                List<Coroutine> moveCoroutines = new List<Coroutine>();
                foreach (var move in moves)
                {
                    Block block = move.Key;
                    Vector2Int from = FindBlockPosition(block); // 현재 위치를 찾아야 함
                    Vector2Int to = move.Value;

                    // 데이터 위치 업데이트
                    GameBoardData.SetBlock(to.x, to.y, block);
                    if(from.x != -1) GameBoardData.SetBlock(from.x, from.y, null);

                    // 애니메이션 실행
                    Vector3 targetWorldPos = blockMover.GridToWorld(to, GameBoardData.Width, GameBoardData.Height);
                    moveCoroutines.Add(StartCoroutine(MoveBlockCoroutine(block, targetWorldPos, _stepDuration)));
                }
                
                // 모든 블록이 한 스텝 움직일 때까지 대기
                foreach(var coroutine in moveCoroutines)
                {
                    yield return coroutine;
                }
            }

            _isRefilling = false;
        }

        private Dictionary<Block, Vector2Int> FindNextStepMoves()
        {
            Dictionary<Block, Vector2Int> moves = new Dictionary<Block, Vector2Int>();

            // 1순위: 수직 낙하
            for (int x = 0; x < GameBoardData.Width; x++)
            {
                for (int y = 1; y < GameBoardData.Height; y++)
                {
                    Block block = GameBoardData.GetBlock(x, y);
                    if (block != null && !block.IsObstacle)
                    {
                        Block below = GameBoardData.GetBlock(x, y - 1);
                        if (below == null && GameBoardData.BlockPlate.BlockPlateArray[y - 1, x])
                        {
                            moves[block] = new Vector2Int(x, y - 1);
                        }
                    }
                }
            }
            // TODO: 2순위 대각선 흐름 로직 추가

            return moves;
        }

        private int SpawnNewBlocks(Dictionary<Block, Vector2Int> moves)
        {
            int createdCount = 0;
            for (int x = 0; x < GameBoardData.Width; x++)
            {
                if (GameBoardData.GetBlock(x, GameBoardData.Height - 1) == null && GameBoardData.BlockPlate.BlockPlateArray[GameBoardData.Height - 1, x])
                {
                    Block newBlock = SpawnRandomBlock(x, GameBoardData.Height);
                    Vector3 startPos = new Vector3(newBlock.BlockInstance.transform.position.x, newBlock.BlockInstance.transform.position.y + 1, 0);
                    newBlock.BlockInstance.transform.position = startPos;
                    moves[newBlock] = new Vector2Int(x, GameBoardData.Height - 1);
                    createdCount++;
                }
            }
            return createdCount;
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
                        DestroyedBlocks.Add(block.GemType);
                        GameBoardData.SetBlock(x, y, null);
                    }
                }
            }
        }

        public void SpawnBlock(int x, int y, GemType gemType, BlockMover blockMover)
        {
            GameObject blockPrefab = GetBlockPrefab((int)gemType);
            if (blockPrefab == null) return;
            Vector3 position = blockMover.GridToWorld(new Vector2Int(x, y), GameBoardData.Width, GameBoardData.Height);
            GameObject blockInstance = Instantiate(blockPrefab, position, Quaternion.identity);
            Block newBlock = new Block { BlockInstance = blockInstance, GemType = gemType };
            GameBoardData.SetBlock(x, y, newBlock);
        }

        public void RandomPosSpawnSpecialBlock(GemType gemType)
        {
            int x = Random.Range(0, GameBoardData.Width);
            int y = Random.Range(0, GameBoardData.Height);

            if (GameBoardData.BlockPlate.BlockPlateArray[y, x])
            {
                return;
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

        public Block SpawnRandomBlock(int x, int y)
        {
            int randomTypeInt = Random.Range(_spawnRangeMin, _spawnRangeMax + 1);
            GemType gemType = (GemType)(randomTypeInt - 1);
            Block newBlock = new Block { GemType = gemType };
            GameBoardData.SetBlock(x, y, newBlock);
            return newBlock;
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
            for (int y = 0; y < GameBoardData.Height; y++)
            {
                for (int x = 0; x < GameBoardData.Width; x++)
                {
                    if (GameBoardData.GetBlock(x, y) == null && GameBoardData.BlockPlate.BlockPlateArray[y, x])
                    {
                        // TODO: 이 부분은 FindNextStepMoves와 같은 로직을 사용해야 함
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
