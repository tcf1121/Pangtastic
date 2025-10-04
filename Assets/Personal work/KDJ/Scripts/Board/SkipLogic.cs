using System.Collections.Generic;
using System.Linq;
using SCR;
using UnityEngine;

namespace KDJ
{
    public class SkipResult
    {
        public int GainedScore { get; set; }
        public Dictionary<GemType, int> GainedIngredients { get; set; } = new Dictionary<GemType, int>();
    }

    public class SkipLogic
    {
        private Block[,] _virtualBoard;
        private Block[,] _virtualOverlay;
        private GameBoardData _initialBoardState;
        private int _height; // 배열 높이
        private int _boardHeight; // 실제 게임 보드 높이
        private int _width;
        private int _spawnRangeMax;

        private Queue<Block>[] _blockWaitingQueue;
        private int _combo = 0;
        private SkipResult _result = new SkipResult();

        public SkipLogic(GameBoardData initialBoardState, int spawnRangeMax)
        {
            _initialBoardState = initialBoardState;
            _height = initialBoardState.BlockArray.GetLength(0);
            _boardHeight = initialBoardState.Height;
            _width = initialBoardState.Width;
            _spawnRangeMax = spawnRangeMax;

            // --- 깊은 복사 1단계: 객체 복제 ---
            _virtualBoard = new Block[_height, _width];
            _virtualOverlay = new Block[_height, _width];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (initialBoardState.BlockArray[y, x] != null) { _virtualBoard[y, x] = initialBoardState.BlockArray[y, x].Clone(); }
                    if (initialBoardState.OverlayArray != null && y < initialBoardState.OverlayArray.GetLength(0) && x < initialBoardState.OverlayArray.GetLength(1) && initialBoardState.OverlayArray[y, x] != null) { _virtualOverlay[y, x] = initialBoardState.OverlayArray[y, x].Clone(); }
                }
            }

            // --- 깊은 복사 2단계: 참조 수정 ---
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_virtualBoard[y, x] is FlourBag_s flourBagS)
                    {
                        Vector2Int ownerPos = flourBagS.OwnerPos();
                        if (ownerPos.x >= 0 && ownerPos.x < _width && ownerPos.y >= 0 && ownerPos.y < _height)
                        {
                            if (_virtualBoard[ownerPos.y, ownerPos.x] is FlourBag ownerBag) { flourBagS.SetOwner(ownerBag); }
                        }
                    }
                }
            }
            
            // --- 리필 큐 초기화 ---
            _blockWaitingQueue = new Queue<Block>[_width];
            for (int i = 0; i < _width; i++)
            {
                _blockWaitingQueue[i] = new Queue<Block>();
            }
        }

        public SkipResult Simulate(List<GemType> rewards)
        {
            CreateRewardBlocks_DataOnly(rewards);

            while (true)
            {
                var matchResult = ProcessMatches_DataOnly();
                if (matchResult.coordsToDestroy.Count == 0)
                {
                    break;
                }

                CreateSpecialBlocks_DataOnly(matchResult.specialsToCreate);
                DestroyBlocks_DataOnly(matchResult.coordsToDestroy);
                RefillBoard_DataOnly();
            }

            return _result;
        }

        #region 메인 시뮬레이션 단계

        private void CreateRewardBlocks_DataOnly(List<GemType> rewards)
        {
            if (rewards == null) return;

            for (int i = 0; i < rewards.Count; i++)
            {
                Vector2Int randPos;
                int attempts = 0;
                while (true)
                {
                    randPos = new Vector2Int(Random.Range(0, _width), Random.Range(0, _boardHeight));
                    if (_initialBoardState.BlockPlate.BlockPlateArray[randPos.y, randPos.x] &&
                        _virtualBoard[randPos.y, randPos.x] != null &&
                        _virtualBoard[randPos.y, randPos.x].IsNormal &&
                        !(_virtualOverlay[randPos.y, randPos.x] is Ice))
                    {
                        break;
                    }

                    attempts++;
                    if (attempts > _width * _height)
                    {
                        Debug.Log("SkipLogic: 보상 블록을 놓을 유효한 위치를 찾지 못했습니다. 루프를 중단합니다.");
                        break; 
                    }
                }
                _virtualBoard[randPos.y, randPos.x] = CreateNewBlock_DataOnly(rewards[i], randPos.x, randPos.y);
            }
        }

        private void DestroyBlocks_DataOnly(HashSet<Vector2Int> coordsToDestroy)
        {
            foreach (var coord in coordsToDestroy)
            {
                if (_virtualBoard[coord.y, coord.x] is FlourBag_s flourS)
                {
                    if (flourS.Owner.CurrentHP > 0) continue;
                }
                _virtualBoard[coord.y, coord.x] = null;
            }
        }

        private void CreateSpecialBlocks_DataOnly(List<(Vector2Int pos, int type, List<Vector2Int> matchCoords)> specialsToCreate)
        {
            if (specialsToCreate.Count == 0) return;

            foreach (var special in specialsToCreate)
            {
                Vector2Int spawnPos = CalculateSpecialBlockSpawnPosition_DataOnly(special.matchCoords, null);
                _virtualBoard[spawnPos.y, spawnPos.x] = CreateNewBlock_DataOnly((GemType)special.type, spawnPos.x, spawnPos.y);
            }
        }

        private void RefillBoard_DataOnly()
        {
            while (true)
            {
                bool activityThisStep = false;
                bool[,] movedToThisTick = new bool[_boardHeight, _width];
                bool[,] movedFlags = new bool[_height, _width];

                // 수직 낙하
                for (int y = 1; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        Block block = _virtualBoard[y, x];
                        if (block != null && block.CanMove)
                        {
                            int lowestPossibleY = y;
                            for (int k = y - 1; k >= 0; k--) { if (_virtualBoard[k, x] != null) { lowestPossibleY = k + 1; break; } lowestPossibleY = k; }

                            int destY = y;
                            for (int k = y - 1; k >= lowestPossibleY; k--) { if (k < _boardHeight && _initialBoardState.BlockPlate.BlockPlateArray[k, x]) { destY = k; break; } }

                            if (destY != y)
                            {
                                if (movedFlags[destY, x]) continue;
                                _virtualBoard[destY, x] = block;
                                _virtualBoard[y, x] = null;
                                if (_virtualBoard[destY, x] is ObstacleBlock ob) { ob.X = x; ob.Y = destY; ob.OnLand(destY); }
                                movedFlags[destY, x] = true;
                                activityThisStep = true;
                            }
                        }
                    }
                }

                // 대각선 낙하
                for (int y = 0; y < _boardHeight; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        if (_virtualBoard[y, x] == null && _initialBoardState.BlockPlate.BlockPlateArray[y, x] && !movedFlags[y, x])
                        {
                            Block blockAbove = _virtualBoard[y + 1, x];
                            if (blockAbove != null && !blockAbove.CanMove)
                            {
                                Block leftDiagonalBlock = (x > 0) ? _virtualBoard[y + 1, x - 1] : null;
                                if (leftDiagonalBlock != null && leftDiagonalBlock.CanMove)
                                {
                                    _virtualBoard[y, x] = leftDiagonalBlock;
                                    _virtualBoard[y + 1, x - 1] = null;
                                    if (_virtualBoard[y, x] is ObstacleBlock ob) { ob.X = x; ob.Y = y; ob.OnLand(y); }
                                    movedFlags[y, x] = true;
                                    activityThisStep = true;
                                }
                                else
                                {
                                    Block rightDiagonalBlock = (x < _width - 1) ? _virtualBoard[y + 1, x + 1] : null;
                                    if (rightDiagonalBlock != null && rightDiagonalBlock.CanMove)
                                    {
                                        _virtualBoard[y, x] = rightDiagonalBlock;
                                        _virtualBoard[y + 1, x + 1] = null;
                                        if (_virtualBoard[y, x] is ObstacleBlock ob) { ob.X = x; ob.Y = y; ob.OnLand(y); }
                                        movedFlags[y, x] = true;
                                        activityThisStep = true;
                                    }
                                }
                            }
                        }
                    }
                }

                // 모래 흐름
                if (!activityThisStep)
                {
                    for (int y = _boardHeight - 1; y >= 1; y--)
                    {
                        for (int x = 0; x < _width; x++)
                        {
                            Block currentBlock = _virtualBoard[y, x];
                            if (currentBlock != null && currentBlock.CanMove)
                            {
                                Block blockAbove = _virtualBoard[y + 1, x];
                                if ((blockAbove == null || !blockAbove.CanMove) && _virtualBoard[y - 1, x] != null)
                                {
                                    if (x > 0 && _virtualBoard[y, x - 1] == null && _virtualBoard[y - 1, x - 1] == null && !movedToThisTick[y - 1, x - 1] && IsWellBelowObstacle_DataOnly(x - 1, y - 1))
                                    {
                                        _virtualBoard[y - 1, x - 1] = currentBlock;
                                        _virtualBoard[y, x] = null;
                                        if (_virtualBoard[y - 1, x - 1] is ObstacleBlock ob) { ob.X = x - 1; ob.Y = y - 1; ob.OnLand(y - 1); }
                                        movedToThisTick[y - 1, x - 1] = true;
                                        activityThisStep = true;
                                        continue;
                                    }
                                    if (x < _width - 1 && _virtualBoard[y, x + 1] == null && _virtualBoard[y - 1, x + 1] == null && !movedToThisTick[y - 1, x + 1] && IsWellBelowObstacle_DataOnly(x + 1, y - 1))
                                    {
                                        _virtualBoard[y - 1, x + 1] = currentBlock;
                                        _virtualBoard[y, x] = null;
                                        if (_virtualBoard[y - 1, x + 1] is ObstacleBlock ob) { ob.X = x + 1; ob.Y = y - 1; ob.OnLand(y - 1); }
                                        movedToThisTick[y - 1, x + 1] = true;
                                        activityThisStep = true;
                                    }
                                }
                            }
                        }
                    }
                }

                // 새 블록 생성
                if (RefillWaitingQueue_DataOnly()) { activityThisStep = true; }

                if (!activityThisStep) { break; }
            }
        }

        #endregion

        #region 매치 로직 (BoardMatchChecker에서 가져옴)

        private (HashSet<Vector2Int> coordsToDestroy, List<(Vector2Int pos, int type, List<Vector2Int> matchCoords)> specialsToCreate) ProcessMatches_DataOnly()
        {
            var coordsToDestroy = new HashSet<Vector2Int>();
            var specialsToCreate = new List<(Vector2Int pos, int type, List<Vector2Int> matchCoords)>();
            bool[,] visited = new bool[_boardHeight, _width];

            // 우선순위 1: 5개 한 줄 매치
            for (int y = 0; y < _boardHeight; y++) { for (int x = 0; x < _width; x++) { if (visited[y, x] || !CheckBlockIsAllValid(x, y)) continue; var hMatch = FindFullLineMatch(x, y, true); if (hMatch.Count >= 5) { if (!hMatch.Any(c => visited[c.y, c.x])) specialsToCreate.Add((new Vector2Int(x, y), 10, hMatch)); foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } } var vMatch = FindFullLineMatch(x, y, false); if (vMatch.Count >= 5) { if (!vMatch.Any(c => visited[c.y, c.x])) specialsToCreate.Add((new Vector2Int(x, y), 10, vMatch)); foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } } } }

            // 우선순위 2: L/T 모양 매치
            for (int y = 0; y < _boardHeight; y++) { for (int x = 0; x < _width; x++) { if (visited[y, x] || !CheckBlockIsAllValid(x, y)) continue; var hMatch = FindFullLineMatch(x, y, true); var vMatch = FindFullLineMatch(x, y, false); if (hMatch.Count >= 3 && vMatch.Count >= 3) { var combinedMatch = new List<Vector2Int>(hMatch.Union(vMatch)); if (!combinedMatch.Any(c => visited[c.y, c.x])) specialsToCreate.Add((new Vector2Int(x, y), 9, combinedMatch)); foreach (var c in combinedMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } } } }

            // 우선순위 3: 2x2 큐브 매치
            for (int y = 0; y < _boardHeight - 1; y++) { for (int x = 0; x < _width - 1; x++) { if (IsCubeMatched(x, y)) { var coords = new List<Vector2Int> { new(x, y), new(x + 1, y), new(x, y + 1), new(x + 1, y + 1) }; if (!coords.Any(c => visited[c.y, c.x])) specialsToCreate.Add((new Vector2Int(x, y), 6, coords)); foreach (var c in coords) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } } } }

            // 우선순위 4: 4개 한 줄 매치
            for (int y = 0; y < _boardHeight; y++) { for (int x = 0; x < _width; x++) { if (visited[y, x] || !CheckBlockIsAllValid(x, y)) continue; var hMatch = FindFullLineMatch(x, y, true); if (hMatch.Count == 4) { if (!hMatch.Any(c => visited[c.y, c.x])) specialsToCreate.Add((new Vector2Int(x, y), 7, hMatch)); foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } } var vMatch = FindFullLineMatch(x, y, false); if (vMatch.Count == 4) { if (!vMatch.Any(c => visited[c.y, c.x])) specialsToCreate.Add((new Vector2Int(x, y), 8, vMatch)); foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } } } }

            // 우선순위 5: 3개 한 줄 매치
            for (int y = 0; y < _boardHeight; y++) { for (int x = 0; x < _width; x++) { if (visited[y, x] || !CheckBlockIsAllValid(x, y)) continue; var hMatch = FindFullLineMatch(x, y, true); if (hMatch.Count >= 3) { foreach (var c in hMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } } var vMatch = FindFullLineMatch(x, y, false); if (vMatch.Count >= 3) { foreach (var c in vMatch) { coordsToDestroy.Add(c); visited[c.y, c.x] = true; } } } }

            if (coordsToDestroy.Count > 0)
            {
                _combo++;
                var damagedObstaclesThisMatch = new HashSet<Block>();
                foreach (var coord in coordsToDestroy)
                {
                    Block block = _virtualBoard[coord.y, coord.x];
                    if (block != null)
                    {
                        int score = CalculateScore(block.Score);
                        _result.GainedScore += score;
                        if (_result.GainedIngredients.ContainsKey(block.GemType)) _result.GainedIngredients[block.GemType]++;
                        else _result.GainedIngredients[block.GemType] = 1;

                        ApplySplashDamageToNeighbors_DataOnly(coord.x, coord.y, damagedObstaclesThisMatch);
                    }
                    ApplyDamageToOverlayBlock_DataOnly(coord.x, coord.y, damagedObstaclesThisMatch);
                }
            }
            else { _combo = 0; }

            return (coordsToDestroy, specialsToCreate);
        }

        private void ApplySplashDamageToNeighbors_DataOnly(int x, int y, HashSet<Block> damagedObstacles)
        {
            Vector2Int[] neighborCoords = { new(x, y + 1), new(x, y - 1), new(x + 1, y), new(x - 1, y) };
            foreach (var coord in neighborCoords)
            {
                if (CheckInOfArray(coord.x, coord.y))
                {
                    Block neighbor = _virtualBoard[coord.y, coord.x];
                    if (neighbor == null) continue;

                    ObstacleBlock obstacleToDamage = null;
                    if (neighbor is FlourBag_s flourBagS)
                    {
                        obstacleToDamage = flourBagS.Owner;
                    }
                    else if (neighbor is ObstacleBlock)
                    {
                        obstacleToDamage = neighbor as ObstacleBlock;
                    }

                    if (obstacleToDamage != null && !damagedObstacles.Contains(obstacleToDamage))
                    {
                        TakeDamage_DataOnly(obstacleToDamage);
                        damagedObstacles.Add(obstacleToDamage);
                    }
                }
            }
        }

        private void ApplyDamageToOverlayBlock_DataOnly(int x, int y, HashSet<Block> damagedObstacles)
        {
            if (_virtualOverlay[y, x] is ObstacleBlock obstacle && !damagedObstacles.Contains(obstacle))
            {
                TakeDamage_DataOnly(obstacle);
                damagedObstacles.Add(obstacle);
            }
        }

        private void TakeDamage_DataOnly(ObstacleBlock obstacle)
        {
            obstacle.CurrentHP--;
            if (obstacle.CurrentHP <= 0)
            {
                Broken_DataOnly(obstacle);
            }
        }

        private void Broken_DataOnly(ObstacleBlock obstacle)
        {
            if (obstacle is DonutBag db) { _virtualBoard[db.Y, db.X] = new Block { GemType = (GemType)Random.Range(0, _spawnRangeMax) }; }
            else if (obstacle is GiftBox gb) { _virtualBoard[gb.Y, gb.X] = new Block { GemType = GemType.Milk }; }
            else if (obstacle is FlourBag f) { _virtualBoard[f.Y, f.X] = null; }
            else if (obstacle is Ice i)
            {
                _virtualOverlay[i.Y, i.X] = null;
                if (_virtualBoard[i.Y, i.X] != null) _virtualBoard[i.Y, i.X].CanMove = true;
            }
            else if (obstacle is Dust d) { _virtualOverlay[d.Y, d.X] = null; }
            else if (obstacle is Syrup s) { _virtualOverlay[s.Y, s.X] = null; }
            else { _virtualBoard[obstacle.Y, obstacle.X] = null; }
        }

        private List<Vector2Int> FindFullLineMatch(int startX, int startY, bool isHorizontal)
        {
            var m = new List<Vector2Int>();
            var b = _virtualBoard[startY, startX];
            if (b == null || !b.IsNormal) return m;

            var t = b.GemType;
            m.Add(new Vector2Int(startX, startY));

            if (isHorizontal)
            {
                for (int x = startX - 1; x >= 0; x--)
                {
                    var cb = _virtualBoard[startY, x];
                    if (cb != null && cb.IsNormal && cb.GemType == t) m.Add(new Vector2Int(x, startY));
                    else break;
                }
                for (int x = startX + 1; x < _width; x++)
                {
                    var cb = _virtualBoard[startY, x];
                    if (cb != null && cb.IsNormal && cb.GemType == t) m.Add(new Vector2Int(x, startY));
                    else break;
                }
            }
            else
            {
                for (int y = startY - 1; y >= 0; y--)
                {
                    var cb = _virtualBoard[y, startX];
                    if (cb != null && cb.IsNormal && cb.GemType == t) m.Add(new Vector2Int(startX, y));
                    else break;
                }
                for (int y = startY + 1; y < _boardHeight; y++)
                {
                    var cb = _virtualBoard[y, startX];
                    if (cb != null && cb.IsNormal && cb.GemType == t) m.Add(new Vector2Int(startX, y));
                    else break;
                }
            }
            return m;
        }

        private bool IsCubeMatched(int x, int y)
        {
            var sb = _virtualBoard[y, x];
            if (sb == null || !sb.IsNormal) return false;

            var sgt = sb.GemType;
            for (int i = y; i < y + 2; i++)
            {
                for (int j = x; j < x + 2; j++)
                {
                    var cb = _virtualBoard[i, j];
                    if (cb == null || !cb.IsNormal || cb.GemType != sgt) return false;
                }
            }
            return true;
        }

        private bool CheckInOfArray(int x, int y)
        {
            return y >= 0 && y < _boardHeight && x >= 0 && x < _width;
        }

        private bool CheckBlockIsValue(int x, int y)
        {
            return _initialBoardState.BlockPlate.BlockPlateArray[y, x] && _virtualBoard[y, x] != null;
        }

        private bool CheckBlockIsAllValid(int x, int y)
        {
            return CheckInOfArray(x, y) && CheckBlockIsValue(x, y);
        }

        private int CalculateScore(int score)
        {
            return (_combo > 1) ? (int)(score + _combo * 0.5f * score) : score;
        }

        private Vector2Int CalculateSpecialBlockSpawnPosition_DataOnly(List<Vector2Int> matchCoords, List<Vector2Int> swapPositions)
        {
            if (swapPositions != null)
            {
                foreach (var swapPos in swapPositions)
                {
                    if (matchCoords.Contains(swapPos)) return swapPos;
                }
            }

            Vector2Int bl = new Vector2Int(int.MaxValue, int.MaxValue);
            foreach (var coord in matchCoords)
            {
                if (_virtualOverlay[coord.y, coord.x] is Ice) continue;
                if (coord.y < bl.y)
                {
                    bl = coord;
                }
                else if (coord.y == bl.y)
                {
                    if (coord.x < bl.x) bl.x = coord.x;
                }
            }
            return bl;
        }

        private Block CreateNewBlock_DataOnly(GemType gemType, int x, int y)
        {
            if (gemType == GemType.DonutBag) return new DonutBag(x, y);
            if (gemType == GemType.Coin) return new Coin(x, y);
            if (gemType == GemType.GiftBox) return new GiftBox(x, y);
            if (gemType == GemType.Egg) return new Egg(x, y);
            if (gemType == GemType.FlourBag) return new FlourBag(_virtualBoard, x, y);

            var nb = new Block { GemType = gemType };
            if (gemType > GemType.Sugar && gemType < GemType.Dust)
            {
                nb.IsNormal = false;
            }
            return nb;
        }

        private bool IsWellBelowObstacle_DataOnly(int x, int y)
        {
            for (int i = y + 1; i < _height; i++)
            {
                if (_virtualBoard[i, x] != null) return !_virtualBoard[i, x].CanMove;
            }
            return false;
        }

        private bool RefillWaitingQueue_DataOnly()
        {
            bool refilled = false;
            int queueRow = _boardHeight;
            for (int x = 0; x < _width; x++)
            {
                if (_virtualBoard[queueRow, x] == null)
                {
                    if (_blockWaitingQueue[x].Count == 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            _blockWaitingQueue[x].Enqueue(new Block { GemType = (GemType)Random.Range(0, _spawnRangeMax) });
                        }
                    }
                    Block newBlock = _blockWaitingQueue[x].Dequeue();
                    _virtualBoard[queueRow, x] = newBlock;
                    refilled = true;
                }
            }
            return refilled;
        }

        #endregion
    }
}
