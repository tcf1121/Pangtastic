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
        { BlockNum.Carrot, BlockNum.Carrot, BlockNum.Carrot, BlockNum.Lemon, BlockNum.Carrot, BlockNum.Lemon },
        { BlockNum.Carrot, BlockNum.Lemon, BlockNum.Carrot, BlockNum.Lemon, BlockNum.Carrot, BlockNum.Lemon },
        { BlockNum.Carrot, BlockNum.Carrot, BlockNum.Lemon, BlockNum.Carrot, BlockNum.Lemon, BlockNum.Lemon },
        { BlockNum.Grape,   BlockNum.Carrot, BlockNum.Carrot, BlockNum.Lemon, BlockNum.Carrot, BlockNum.Lemon },
        { BlockNum.Carrot, BlockNum.Lemon, BlockNum.Lemon, BlockNum.Carrot, BlockNum.Carrot, BlockNum.Lemon },
        { BlockNum.Carrot, BlockNum.Grape, BlockNum.Grape, BlockNum.Lemon, BlockNum.Grape, BlockNum.Carrot }
    };

    [SerializeField] private MatchChecker _matchChecker; 

    private GameObject[,] _blockObjects;
    private int _width;
    private int _height;

    private void Start()
    {
        _width = _mapArray.GetLength(1);
        _height = _mapArray.GetLength(0);
        _blockObjects = new GameObject[_height, _width];

        SpawnMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_matchChecker != null)
            {
                Debug.Log("스페이스바 입력됨 → 매치 검사 시작");
                _matchChecker.SetBoard(_mapArray, _blockObjects);
            }
            else
            {
                Debug.LogWarning("MatchChecker가 연결되지 않았습니다!");
            }
        }
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
                }
            }
        }
    }
}
