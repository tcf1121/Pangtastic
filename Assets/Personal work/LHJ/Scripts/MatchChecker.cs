using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class MatchChecker : MonoBehaviour
{
    private SCR.GemType[,] _boardData;
    private GameObject[,] _blockObjects;

    private int _width;
    private int _height;

    [SerializeField] private BonusGemList _gemList;
    private Vector2Int? _preferCell;

    public void SetBoard(SCR.GemType[,] boardData, GameObject[,] blockObjects, Vector2Int? preferCell = null)
    {
        _boardData = boardData;
        _blockObjects = blockObjects;
        _preferCell = preferCell;

        _height = _boardData.GetLength(0);
        _width = _boardData.GetLength(1);

        CheckMatches();
    }

    // 특수 블럭 우선순위
    private enum MatchType
    {
        Honeycomb = 1,
        FruitBasket = 2,
        Milk = 3,
        Knife = 4,
        Normal = 5
    }

    // 매치 후보
    private class MatchCandidate
    {
        public List<Vector2Int> Cells;
        public SCR.GemType Type;
        public Vector2Int SpawnCell;
        public MatchType Priority;
    }

    private void CheckMatches()
    {
        List<MatchCandidate> allCandidates = new();
        bool[,] inH3 = new bool[_height, _width];
        bool[,] inV3 = new bool[_height, _width];
        bool[,] visitedH = new bool[_height, _width];
        bool[,] visitedV = new bool[_height, _width];

        // 1. 가로 매치
        for (int y = 0; y < _height; y++)
        {
            int x = 0;
            while (x < _width)
            {
                SCR.GemType val = _boardData[y, x];
                if (!IsMatchable(val) || visitedH[y, x]) { x++; continue; }

                int start = x;
                int len = 1;
                while (x + len < _width && _boardData[y, x + len] == val) len++;
                for (int k = 0; k < len; k++) visitedH[y, start + k] = true;

                if (len >= 3)
                {
                    List<Vector2Int> group = new();
                    for (int k = 0; k < len; k++) group.Add(new Vector2Int(start + k, y));
                    Vector2Int pivot = new Vector2Int(start + (len / 2), y);
                    Vector2Int spawnCell = ChooseSpawnCell(group, pivot);

                    if (len == 4)
                        allCandidates.Add(new MatchCandidate { Cells = group, Type = GemType.Knife_h, SpawnCell = spawnCell, Priority = MatchType.Knife });
                    else if (len >= 5)
                        allCandidates.Add(new MatchCandidate { Cells = group, Type = GemType.Honeycomb, SpawnCell = spawnCell, Priority = MatchType.Honeycomb });

                    if (len == 3)
                        allCandidates.Add(new MatchCandidate { Cells = group, Type = val, SpawnCell = spawnCell, Priority = MatchType.Normal });

                    for (int k = 0; k < len; k++) inH3[y, start + k] = true;
                }

                x = start + len;
            }
        }

        // 2. 세로 매치
        for (int x = 0; x < _width; x++)
        {
            int y = 0;
            while (y < _height)
            {
                SCR.GemType val = _boardData[y, x];
                if (!IsMatchable(val) || visitedV[y, x]) { y++; continue; }

                int start = y;
                int len = 1;
                while (y + len < _height && _boardData[y + len, x] == val) len++;
                for (int k = 0; k < len; k++) visitedV[start + k, x] = true;

                if (len >= 3)
                {
                    List<Vector2Int> group = new();
                    for (int k = 0; k < len; k++) group.Add(new Vector2Int(x, start + k));
                    Vector2Int pivot = new Vector2Int(x, start + (len / 2));
                    Vector2Int spawnCell = ChooseSpawnCell(group, pivot);

                    if (len == 4)
                        allCandidates.Add(new MatchCandidate { Cells = group, Type = GemType.Knife_v, SpawnCell = spawnCell, Priority = MatchType.Knife });
                    else if (len >= 5)
                        allCandidates.Add(new MatchCandidate { Cells = group, Type = GemType.Honeycomb, SpawnCell = spawnCell, Priority = MatchType.Honeycomb });

                    if (len == 3)
                        allCandidates.Add(new MatchCandidate { Cells = group, Type = val, SpawnCell = spawnCell, Priority = MatchType.Normal });

                    for (int k = 0; k < len; k++) inV3[start + k, x] = true;
                }

                y = start + len;
            }
        }

        // 3. 정사각형 (우유)
        for (int y = 0; y < _height - 1; y++)
        {
            for (int x = 0; x < _width - 1; x++)
            {
                SCR.GemType v = _boardData[y, x];
                if (!IsMatchable(v)) continue;

                if (_boardData[y, x + 1] == v && _boardData[y + 1, x] == v && _boardData[y + 1, x + 1] == v)
                {
                    List<Vector2Int> group = new()
                {
                    new Vector2Int(x, y),
                    new Vector2Int(x + 1, y),
                    new Vector2Int(x, y + 1),
                    new Vector2Int(x + 1, y + 1),
                };
                    Vector2Int spawnCell = ChooseSpawnCell(group, new Vector2Int(x, y));
                    allCandidates.Add(new MatchCandidate { Cells = group, Type = GemType.Milk, SpawnCell = spawnCell, Priority = MatchType.Milk });
                }
            }
        }

        // 과일 바구니
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (inH3[y, x] && inV3[y, x])
                {
                    List<Vector2Int> group = new() { new Vector2Int(x, y) };
                    Vector2Int spawnCell = ChooseSpawnCell(group, new Vector2Int(x, y));
                    allCandidates.Add(new MatchCandidate { Cells = group, Type = GemType.FruitBasket, SpawnCell = spawnCell, Priority = MatchType.FruitBasket });
                }
            }
        }

        // 우선순위
        List<MatchCandidate> filteredCandidates = new(); // 특수블럭 생성용
        HashSet<Vector2Int> matched = new();             // 제거 대상 셀

        foreach (var cand in allCandidates)
        {
            List<Vector2Int> available = new List<Vector2Int>(cand.Cells);

            foreach (var other in filteredCandidates)
            {
                foreach (var cell in other.Cells)
                {
                    if (cand.Priority >= other.Priority && available.Contains(cell))
                        available.Remove(cell);
                }
            }

            if (available.Count >= 3)
            {
                foreach (var cell in available)
                    matched.Add(cell);

                // 특수블럭만 생성 후보로 추가
                if (cand.Priority != MatchType.Normal)
                {
                    filteredCandidates.Add(new MatchCandidate
                    {
                        Cells = available,
                        Type = cand.Type,
                        SpawnCell = ChooseSpawnCell(available, cand.SpawnCell),
                        Priority = cand.Priority
                    });
                }
            }
        }

        // 매치 블럭 제거
        foreach (var p in matched)
        {
            if (_blockObjects[p.y, p.x] != null)
            {
                Destroy(_blockObjects[p.y, p.x]);
                _blockObjects[p.y, p.x] = null;
            }
            _boardData[p.y, p.x] = (GemType)(-1);
        }

        // 특수블럭 생성
        foreach (var cand in filteredCandidates)
        {
            if (_gemList == null) continue;
            Vector3 world = CellToWorld(cand.SpawnCell.x, cand.SpawnCell.y);
            GameObject obj = _gemList.Spawn(cand.Type, world, null);
            if (obj != null)
                _blockObjects[cand.SpawnCell.y, cand.SpawnCell.x] = obj;
        }

        Debug.Log($"[MatchChecker] 매치 제거: {matched.Count}개, 특수 생성: {filteredCandidates.Count}개");
    }

    private bool IsMatchable(SCR.GemType block)
    {
        return block == GemType.Carrot || block == GemType.Lemon || block == GemType.Grape ||
               block == GemType.Strawberry || block == GemType.Apple || block == GemType.Cabbage;
    }

    private Vector2Int ChooseSpawnCell(List<Vector2Int> groupCells, Vector2Int defaultPivot)
    {
        if (_preferCell.HasValue)
        {
            foreach (var cell in groupCells)
                if (cell == _preferCell.Value)
                    return _preferCell.Value;
        }
        return defaultPivot;
    }

    private Vector3 CellToWorld(int x, int y)
    {
        float wx = x - _width / 2f + 0.5f;
        float wy = y - _height / 2f + 0.5f;
        return new Vector3(wx, wy, 0f);
    }

    // 매치가 존재하는지 확인
    public bool HasAnyMatch(SCR.GemType[,] tempMap)
    {
        int height = tempMap.GetLength(0);
        int width = tempMap.GetLength(1);

        // 가로 검사
        for (int y = 0; y < height; y++)
        {
            int x = 0;
            while (x < width)
            {
                SCR.GemType val = tempMap[y, x];
                if (!IsMatchable(val)) { x++; continue; }

                int len = 1;
                while (x + len < width && tempMap[y, x + len] == val) len++;
                if (len >= 3)
                    return true;

                x += len;
            }
        }

        // 세로 검사
        for (int x = 0; x < width; x++)
        {
            int y = 0;
            while (y < height)
            {
                SCR.GemType val = tempMap[y, x];
                if (!IsMatchable(val)) { y++; continue; }

                int len = 1;
                while (y + len < height && tempMap[y + len, x] == val) len++;
                if (len >= 3)
                    return true;

                y += len;
            }
        }

        // 우유 검사
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                SCR.GemType val = tempMap[y, x];
                if (!IsMatchable(val)) continue;

                if (tempMap[y, x + 1] == val &&
                    tempMap[y + 1, x] == val &&
                    tempMap[y + 1, x + 1] == val)
                    return true;
            }
        }

        // 과일 바구니 검사
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!IsMatchable(tempMap[y, x])) continue;

                bool hasH = x >= 1 && x + 1 < width &&
                            tempMap[y, x - 1] == tempMap[y, x] && tempMap[y, x + 1] == tempMap[y, x];
                bool hasV = y >= 1 && y + 1 < height &&
                            tempMap[y - 1, x] == tempMap[y, x] && tempMap[y + 1, x] == tempMap[y, x];

                if (hasH && hasV)
                    return true;
            }
        }
        return false;
    }
}
