using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _gemTile1;
    [SerializeField] private GameObject _gemTile2;
    [SerializeField] private GameObject _gemTile3;
    [SerializeField] private BlockPlate _blockPlate;

    private GameObject[,] _blockArray;
    // 0 = 빈 블록, 1 = 보석1, 2 = 보석2, 3 = 보석3
    private int[,] _blockDataArray;

    private void Start()
    {
        InitBlockArray();
        DrawBlock();
    }

    private void DrawBlock()
    {
        for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
        {
            for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
            {
                if (_blockDataArray[y, x] != 0)
                {
                    Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                    GameObject gemPrefab = GetRandomGemTile(_blockDataArray[y, x]);
                    _blockArray[y, x] = Instantiate(gemPrefab, position, Quaternion.identity);
                }
                else
                {
                    _blockArray[y, x] = null;
                }
            }
        }
    }

    private void CheckBlockDataArray()
    {
        int blankCount = 0;

        for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
        {
            for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
            {
                if (_blockDataArray[y, x] == 0)
                {
                    blankCount++;
                }
            }
        }

        if (blankCount > 0)
        {
            SortBlockDataArray();

            for (int i = 0; i < blankCount; i++)
            {
                SpawnBlock();
            }
        }
    }

    private void SortBlockDataArray()
    {
        for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
        {
            for (int y = _blockPlate.BlockPlateHeight - 1; y >= 0; y--)
            {
                if (_blockDataArray[y, x] == 0)
                {
                    for (int k = y - 1; k >= 0; k--)
                    {
                        if (_blockDataArray[k, x] != 0)
                        {
                            _blockDataArray[y, x] = _blockDataArray[k, x];
                            _blockDataArray[k, x] = 0;
                            break;
                        }
                    }
                }
            }
        }
    }

    private void SpawnBlock()
    {
        for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
        {
            for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
            {
                if (_blockDataArray[y, x] == 0)
                {
                    _blockDataArray[y, x] = Random.Range(1, 4);
                    Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                    GameObject gemPrefab = GetRandomGemTile(_blockDataArray[y, x]);
                    _blockArray[y, x] = Instantiate(gemPrefab, position, Quaternion.identity);
                    return;
                }
            }
        }
    }

    private void InitBlockArray()
    {
        _blockDataArray = new int[_blockPlate.BlockPlateWidth, _blockPlate.BlockPlateHeight];
        _blockArray = new GameObject[_blockPlate.BlockPlateWidth, _blockPlate.BlockPlateHeight];

        for (int x = 0; x < _blockDataArray.GetLength(0); x++)
        {
            for (int y = 0; y < _blockDataArray.GetLength(1); y++)
            {
                if (_blockPlate.BlockPlateArray[y, x])
                {
                    _blockDataArray[y, x] = Random.Range(1, 4);
                }
                else
                {
                    _blockDataArray[y, x] = 0; // 빈 블록
                }

                _blockArray[y, x] = null;
            }
        }
    }

    private GameObject GetRandomGemTile(int blockNum)
    {
        switch (blockNum)
        {
            case 1:
                return _gemTile1;
            case 2:
                return _gemTile2;
            case 3:
                return _gemTile3;
            default:
                return null; // Should not happen
        }
    }
}
