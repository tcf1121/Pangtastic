using LHJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestMatchMap : MonoBehaviour
{
    [SerializeField] private List<GameObject> _blockPrefabs;

    [SerializeField]
    private BlockNum[,] _mapArray = new BlockNum[,]
    {
        { BlockNum.Carrot, BlockNum.Lemon, BlockNum.Lemon, BlockNum.Grape, BlockNum.Grape, BlockNum.Lemon },
        { BlockNum.Carrot, BlockNum.Lemon, BlockNum.Lemon, BlockNum.Grape, BlockNum.Lemon, BlockNum.Grape },
        { BlockNum.Grape, BlockNum.Grape, BlockNum.Carrot, BlockNum.Lemon, BlockNum.Grape, BlockNum.Grape },
        { BlockNum.Carrot, BlockNum.Lemon, BlockNum.Lemon, BlockNum.Carrot, BlockNum.Lemon, BlockNum.Lemon },
        { BlockNum.Carrot, BlockNum.Grape, BlockNum.Lemon, BlockNum.Lemon, BlockNum.Carrot, BlockNum.Carrot },
        { BlockNum.Grape, BlockNum.Grape, BlockNum.Carrot, BlockNum.Grape, BlockNum.Grape, BlockNum.Carrot },
    };

    [SerializeField] private MatchChecker _matchChecker;
    [SerializeField] private float _dragThreshold = 30f;

    private Vector2Int? _dragStartPos = null;
    private Vector2 _dragStartScreenPos;

    private GameObject[,] _blockObjects;
    private int _width;
    private int _height;

    private void Start()
    {
        _width = _mapArray.GetLength(1);
        _height = _mapArray.GetLength(0);
        _blockObjects = new GameObject[_height, _width];

        SpawnMap();
        //_matchChecker.SetBoard(_mapArray, _blockObjects);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _dragStartScreenPos = Input.mousePosition;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;

            _dragStartPos = WorldToGrid(worldPos);
        }

        else if (Input.GetMouseButtonUp(0) && _dragStartPos.HasValue)
        {
            Vector2 delta = (Vector2)Input.mousePosition - _dragStartScreenPos;

            if (delta.magnitude >= _dragThreshold)
            {
                Vector2Int dir;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    dir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
                else
                    dir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

                Vector2Int from = _dragStartPos.Value;
                Vector2Int to = from + dir;

                if (IsInsideGrid(from) && IsInsideGrid(to))
                {
                    Gem fromGem = _blockObjects[from.y, from.x]?.GetComponent<Gem>();
                    Gem toGem = _blockObjects[to.y, to.x]?.GetComponent<Gem>();

                    if (fromGem != null && toGem != null)
                    {
                        BlockNum[,] tempMap = (BlockNum[,])_mapArray.Clone();
                        (tempMap[from.y, from.x], tempMap[to.y, to.x]) = (tempMap[to.y, to.x], tempMap[from.y, from.x]);

                        if (_matchChecker.HasAnyMatch(tempMap))
                        {
                            SwapBlocks(fromGem, toGem);
                            _matchChecker.SetBoard(_mapArray, _blockObjects);
                        }
                        else
                        {
                            Debug.LogWarning("매치가 되지 않아 스왑할 수 없습니다.");
                        }
                    }
                }
            }

            _dragStartPos = null;
        }
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    if (_matchChecker != null)
        //    {
        //        Debug.Log("스페이스바 입력됨 → 매치 검사 시작");
        //        _matchChecker.SetBoard(_mapArray, _blockObjects);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("MatchChecker가 연결되지 않았습니다!");
        //    }
        //}
    }

    private void SpawnMap()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                BlockNum blockType = _mapArray[y, x];
                int id = (int)blockType;

                Vector3 pos = new Vector3(
                    x - _width / 2 + 0.5f,
                    y - _height / 2 + 0.5f,
                    0
                );

                if (id < _blockPrefabs.Count && _blockPrefabs[id] != null)
                {
                    GameObject obj = Instantiate(_blockPrefabs[id], pos, Quaternion.identity, transform);
                    _blockObjects[y, x] = obj;

                    Gem gem = obj.GetComponent<Gem>();
                    if (gem != null)
                    {
                        gem.Init(this, new Vector2Int(x, y), blockType);
                    }
                }
            }
        }
    }

    private void SwapBlocks(Gem a, Gem b)
    {
        Vector2Int posA = a.GetPosition();
        Vector2Int posB = b.GetPosition();

        (_mapArray[posA.y, posA.x], _mapArray[posB.y, posB.x]) = (_mapArray[posB.y, posB.x], _mapArray[posA.y, posA.x]);
        GameObject objA = _blockObjects[posA.y, posA.x];
        GameObject objB = _blockObjects[posB.y, posB.x];
        (_blockObjects[posA.y, posA.x], _blockObjects[posB.y, posB.x]) = (objB, objA);

        Vector3 tempPos = objA.transform.position;
        objA.transform.position = objB.transform.position;
        objB.transform.position = tempPos;

        a.SetPosition(posB);
        b.SetPosition(posA);
    }
    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x + _width / 2);
        int y = Mathf.FloorToInt(worldPos.y + _height / 2);
        return new Vector2Int(x, y);
    }
    private bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
    }
}
