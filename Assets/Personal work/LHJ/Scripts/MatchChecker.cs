using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchChecker : MonoBehaviour
{
    private int[,] _boardData;
    private GameObject[,] _blockObjects;

    private int _width;
    private int _height;
    public void SetBoard(int[,] boardData, GameObject[,] blockObjects)
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

        // 가로
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x <= _width - 3; x++)
            {
                int val = _boardData[y, x];
                if (val == 0) continue;

                if (_boardData[y, x + 1] == val && _boardData[y, x + 2] == val)
                {
                    matched.Add(new Vector2Int(x, y));
                    matched.Add(new Vector2Int(x + 1, y));
                    matched.Add(new Vector2Int(x + 2, y));
                }
            }
        }

        // 세로
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y <= _height - 3; y++)
            {
                int val = _boardData[y, x];
                if (val == 0) continue;

                if (_boardData[y + 1, x] == val && _boardData[y + 2, x] == val)
                {
                    matched.Add(new Vector2Int(x, y));
                    matched.Add(new Vector2Int(x, y + 1));
                    matched.Add(new Vector2Int(x, y + 2));
                }
            }
        }

        Debug.Log($"매치된 블럭 수: {matched.Count}");

        foreach (var pos in matched)
        {
            _boardData[pos.y, pos.x] = 0;

            if (_blockObjects[pos.y, pos.x] != null)
            {
                Destroy(_blockObjects[pos.y, pos.x]);
                _blockObjects[pos.y, pos.x] = null;
            }
        }
    }
}
