using LHJ;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class MatchChecker : MonoBehaviour
{
    private BlockNum[,] _boardData;
    private GameObject[,] _blockObjects;

    private int _width;
    private int _height;

    [SerializeField] private BonusGemList _gemList;
    [SerializeField] private float _coffeePercent;

    public void SetBoard(BlockNum[,] boardData, GameObject[,] blockObjects)
    {
        _boardData = boardData;
        _blockObjects = blockObjects;

        _height = _boardData.GetLength(0);
        _width = _boardData.GetLength(1);

        CheckMatches();
    }

    private void CheckMatches()
    {
        HashSet<Vector2Int> matched = new HashSet<Vector2Int>();
        List<SpawnRequest> spawnQueue = new List<SpawnRequest>();
        HashSet<Vector2Int> spawnedAt = new HashSet<Vector2Int>();

        bool[,] inH3 = new bool[_height, _width];
        bool[,] inV3 = new bool[_height, _width];
        bool[,] visitedH = new bool[_height, _width];
        bool[,] visitedV = new bool[_height, _width];

        // 가로 검사
        for (int y = 0; y < _height; y++)
        {
            int x = 0;
            while (x < _width)
            {
                BlockNum val = _boardData[y, x];
                if (!IsMatchable(val) || visitedH[y, x]) { x++; continue; }

                int start = x;
                int len = 1;
                while (x + len < _width && _boardData[y, x + len] == val) len++;
                for (int k = 0; k < len; k++) visitedH[y, start + k] = true;

                if (len >= 3)
                {
                    for (int k = 0; k < len; k++)
                    {
                        matched.Add(new Vector2Int(start + k, y));
                        inH3[y, start + k] = true;
                    }

                    Vector2Int pivot = new Vector2Int(start + (len / 2), y);
                    if (len == 3 && Random.Range(0f, 100f) < _coffeePercent)
                        spawnQueue.Add(new SpawnRequest { cell = pivot, type = BlockNum.Coffee });
                    else if (len == 4)
                        spawnQueue.Add(new SpawnRequest { cell = pivot, type = BlockNum.Horizontal });
                    else if (len >= 5)
                        spawnQueue.Add(new SpawnRequest { cell = pivot, type = BlockNum.Honey });
                }

                x = start + len;
            }
        }

        // 세로 검사
        for (int x = 0; x < _width; x++)
        {
            int y = 0;
            while (y < _height)
            {
                BlockNum val = _boardData[y, x];
                if (!IsMatchable(val) || visitedV[y, x]) { y++; continue; }

                int start = y;
                int len = 1;
                while (y + len < _height && _boardData[y + len, x] == val) len++;
                for (int k = 0; k < len; k++) visitedV[start + k, x] = true;

                if (len >= 3)
                {
                    for (int k = 0; k < len; k++)
                    {
                        matched.Add(new Vector2Int(x, start + k));
                        inV3[start + k, x] = true;
                    }

                    Vector2Int pivot = new Vector2Int(x, start + (len / 2));
                    if (len == 3 && Random.Range(0f, 100f) < _coffeePercent)
                        spawnQueue.Add(new SpawnRequest { cell = pivot, type = BlockNum.Coffee });
                    else if (len == 4)
                        spawnQueue.Add(new SpawnRequest { cell = pivot, type = BlockNum.Vertical });
                    else if (len >= 5)
                        spawnQueue.Add(new SpawnRequest { cell = pivot, type = BlockNum.Honey });
                }

                y = start + len;
            }
        }

        // 우유 블럭
        for (int y = 0; y < _height - 1; y++)
        {
            for (int x = 0; x < _width - 1; x++)
            {
                BlockNum v = _boardData[y, x];
                if (!IsMatchable(v)) continue;

                if (_boardData[y, x + 1] == v &&
                    _boardData[y + 1, x] == v &&
                    _boardData[y + 1, x + 1] == v)
                {
                    matched.Add(new Vector2Int(x, y));
                    matched.Add(new Vector2Int(x + 1, y));
                    matched.Add(new Vector2Int(x, y + 1));
                    matched.Add(new Vector2Int(x + 1, y + 1));

                    spawnQueue.Add(new SpawnRequest { cell = new Vector2Int(x, y), type = BlockNum.Milk });
                }
            }
        }

        // 폭탄 블럭
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (inH3[y, x] && inV3[y, x])
                {
                    spawnQueue.Add(new SpawnRequest { cell = new Vector2Int(x, y), type = BlockNum.Bomb });
                }
            }
        }

        // 매치 제거
        foreach (Vector2Int p in matched)
        {
            if (_blockObjects[p.y, p.x] != null)
            {
                Destroy(_blockObjects[p.y, p.x]);
                _blockObjects[p.y, p.x] = null;
            }
            _boardData[p.y, p.x] = (BlockNum)(-1);
        }

        // 특수 블럭 생성
        foreach (var r in spawnQueue)
        {
            matched.Remove(r.cell);
            if (_gemList == null) continue;

            Vector3 world = CellToWorld(r.cell.x, r.cell.y);
            GameObject obj = _gemList.Spawn(r.type, world, null);
            if (obj != null)
            {
                _blockObjects[r.cell.y, r.cell.x] = obj;
            }
        }

        Debug.Log($"[MatchChecker] 매치 제거: {matched.Count}개, 스폰: {spawnQueue.Count}개");
    }

    private bool IsMatchable(BlockNum block)
    {
        switch (block)
        {
            case BlockNum.Carrot:
            case BlockNum.Lemon:
            case BlockNum.Grape:
            case BlockNum.Strawberry:
            case BlockNum.Apple:
            case BlockNum.Cabbage:
                return true;
            default:
                return false;
        }
    }

    private Vector3 CellToWorld(int x, int y)
    {
        float wx = x - _width / 2f + 0.5f;
        float wy = y - _height / 2f + 0.5f;
        return new Vector3(wx, wy, 0f);
    }
    public bool HasAnyMatch(BlockNum[,] board)
    {
        int height = board.GetLength(0);
        int width = board.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            int x = 0;
            while (x < width)
            {
                BlockNum val = board[y, x];
                if (!IsMatchable(val)) { x++; continue; }

                int len = 1;
                while (x + len < width && board[y, x + len] == val) len++;

                if (len >= 3) return true;

                x += len;
            }
        }


        for (int x = 0; x < width; x++)
        {
            int y = 0;
            while (y < height)
            {
                BlockNum val = board[y, x];
                if (!IsMatchable(val)) { y++; continue; }

                int len = 1;
                while (y + len < height && board[y + len, x] == val) len++;

                if (len >= 3) return true;

                y += len;
            }
        }

        return false;
    }

    private struct SpawnRequest
    {
        public Vector2Int cell;
        public BlockNum type;
    }
}
