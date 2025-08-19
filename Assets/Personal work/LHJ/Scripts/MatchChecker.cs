using LHJ;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class MatchChecker : MonoBehaviour
{
    private int[,] _boardData;
    private GameObject[,] _blockObjects;

    private int _width;
    private int _height;

    [SerializeField] private Transform _boardParent;        
    [SerializeField] private BonusGemList _gemList;
    [SerializeField] private float _coffeePercent;
    public void SetBoard(int[,] boardData, GameObject[,] blockObjects, Transform parent)
    {
        _boardData = boardData;
        _blockObjects = blockObjects;

        _height = _boardData.GetLength(0);
        _width = _boardData.GetLength(1);

        if (parent != null) _boardParent = parent; 
        if (_boardParent == null) _boardParent = transform;

        CheckMatches();
    }

    private void CheckMatches()
    {
        HashSet<Vector2Int> matched = new HashSet<Vector2Int>();
        List<SpawnRequest> spawnQueue = new List<SpawnRequest>();
        HashSet<Vector2Int> spawnedAt = new HashSet<Vector2Int>(); 

        bool[,] visitedH = new bool[_height, _width];
        bool[,] visitedV = new bool[_height, _width];

        for (int y = 0; y < _height; y++)
        {
            int x = 0;
            while (x < _width)
            {
                int val = _boardData[y, x];
                if (val == 0 || visitedH[y, x])
                {
                    x++;
                    continue;
                }

                int start = x;
                int len = 1;
                while (x + len < _width && _boardData[y, x + len] == val) len++;
                for (int k = 0; k < len; k++) visitedH[y, start + k] = true;

                if (len >= 3)
                {
                    for (int k = 0; k < len; k++) matched.Add(new Vector2Int(start + k, y));
                    Vector2Int pivot = new Vector2Int(start + (len / 2), y);

                    if (len == 3)
                    {
                        float r = Random.Range(0f, 100f);
                        if (r < _coffeePercent && !spawnedAt.Contains(pivot))
                        {
                            spawnedAt.Add(pivot);
                            spawnQueue.Add(new SpawnRequest { cell = pivot, type = BonusGemType.Coffee });
                        }
                    }
                    else if (len == 4)
                    {
                        if (!spawnedAt.Contains(pivot))
                        {
                            spawnedAt.Add(pivot);
                            spawnQueue.Add(new SpawnRequest { cell = pivot, type = BonusGemType.Horizontal });
                        }
                    }
                }
                x = start + len;
            }
        }

        for (int x = 0; x < _width; x++)
        {
            int y = 0;
            while (y < _height)
            {
                int val = _boardData[y, x];
                if (val == 0 || visitedV[y, x])
                {
                    y++;
                    continue;
                }

                int start = y;
                int len = 1;
                while (y + len < _height && _boardData[y + len, x] == val) len++;
                for (int k = 0; k < len; k++) visitedV[start + k, x] = true;

                if (len >= 3)
                {
                    for (int k = 0; k < len; k++) matched.Add(new Vector2Int(x, start + k));
                    Vector2Int pivot = new Vector2Int(x, start + (len / 2));

                    if (len == 3)
                    {
                        float r = Random.Range(0f, 100f);
                        if (r < _coffeePercent && !spawnedAt.Contains(pivot))
                        {
                            spawnedAt.Add(pivot);
                            spawnQueue.Add(new SpawnRequest { cell = pivot, type = BonusGemType.Coffee });
                        }
                    }
                    else if (len == 4)
                    {
                        if (!spawnedAt.Contains(pivot))
                        {
                            spawnedAt.Add(pivot);
                            spawnQueue.Add(new SpawnRequest { cell = pivot, type = BonusGemType.Vertical });
                        }
                    }
                }

                y = start + len;
            }
        }

        foreach (Vector2Int p in matched)
        {
            if (_blockObjects[p.y, p.x] != null)
            {
                Destroy(_blockObjects[p.y, p.x]);
                _blockObjects[p.y, p.x] = null;
            }
            _boardData[p.y, p.x] = 0;
        }

        for (int i = 0; i < spawnQueue.Count; i++)
        {
            SpawnRequest r = spawnQueue[i];
            if (_gemList == null) continue;

            Vector3 world = CellToWorld(r.cell.x, r.cell.y);
            GameObject obj = _gemList.Spawn(r.type, world, _boardParent != null ? _boardParent : transform);
            if (obj != null)
            {
                _blockObjects[r.cell.y, r.cell.x] = obj;
            }
        }

        Debug.Log($"[MatchChecker] 매치 제거: {matched.Count}개, 스폰: {spawnQueue.Count}개");
    }
    private Vector3 CellToWorld(int x, int y)
    {
        float wx = x - _width / 2f + 0.5f;
        float wy = y - _height / 2f + 0.5f;
        return new Vector3(wx, wy, 0f);
    }
    private struct SpawnRequest
    {
        public Vector2Int cell;
        public BonusGemType type;
    }
}
