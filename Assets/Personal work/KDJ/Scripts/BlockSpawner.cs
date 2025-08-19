using System.Collections;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _block1;
    [SerializeField] private GameObject _block2;
    [SerializeField] private GameObject _block3;
    [SerializeField] private GameObject _block4;
    [SerializeField] private GameObject _block5;
    [SerializeField] private BlockPlate _blockPlate;

    private GameObject[,] _blockArray;
    // 0 = 빈 블록, 1 = 블록1, 2 = 블록2, 3 = 블록3
    private int[,] _blockDataArray;
    private int _blankCount = 0;

    private Coroutine _testCoroutine;

    private void Start()
    {
        InitBlockArray();
        DrawBlock();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestDeleteBlock();
        }

        CheckBlockDataArray();
    }

    private void DrawBlock()
    {
        for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
        {
            for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
            {
                if (_blockPlate.BlockPlateWidth % 2 == 0)
                {
                    if (_blockDataArray[y, x] != 0)
                    {
                        Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                        GameObject blockPrefab = GetBlockTile(_blockDataArray[y, x]);
                        _blockArray[y, x] = Instantiate(blockPrefab, position, Quaternion.identity);
                    }
                    else
                    {
                        _blockArray[y, x] = null;
                    }
                }
                else
                {
                    if (_blockDataArray[y, x] != 0)
                    {
                        Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2, y - _blockPlate.BlockPlateHeight / 2, 0);
                        GameObject blockPrefab = GetBlockTile(_blockDataArray[y, x]);
                        _blockArray[y, x] = Instantiate(blockPrefab, position, Quaternion.identity);
                    }
                    else
                    {
                        _blockArray[y, x] = null;
                    }
                }
            }
        }
    }

    private void CheckBlockDataArray()
    {

        for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
        {
            for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
            {
                if (_blockDataArray[y, x] == 0)
                {
                    _blankCount++;
                }
            }
        }

        // 블록 정리 및 생성을 시각화 하기 위해 지연 추가
        if (_testCoroutine == null && _blankCount > 0)
        {
            _testCoroutine = StartCoroutine(TestCoroutine());
        }

        //if (_blankCount > 0)
        //{
        //    SortBlockDataArray();
        //
        //    for (int i = 0; i < _blankCount; i++)
        //    {
        //        SpawnBlock();
        //    }
        //}
    }

    private void SortBlockDataArray()
    {
        for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
        {
            for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
            {
                if (_blockDataArray[y, x] == 0 && _blockPlate.BlockPlateArray[y, x])
                {
                    // 빈 블록 발견 시 해당 블록 윗칸의 블록을 아래로 내림
                    // 배열 인덱스에 맞춰 블럭을 배치했기에 [0,0]은 최하단 우측에 위치하므로 배열을 뒤집어 탐색
                    for (int k = y + 1; k < _blockPlate.BlockPlateHeight; k++)
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
        // 테스트를 위해 블록을 모두 제거하고 새로 생성

        for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
        {
            for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
            {
                if (_blockDataArray[y, x] == 0 && _blockPlate.BlockPlateArray[y, x])
                {
                    if (_blockPlate.BlockPlateWidth % 2 == 0)
                    {
                        _blockDataArray[y, x] = Random.Range(1, 4);
                        Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                        GameObject gemPrefab = GetBlockTile(_blockDataArray[y, x]);
                        _blockArray[y, x] = Instantiate(gemPrefab, position, Quaternion.identity);
                    }
                    else
                    {
                        _blockDataArray[y, x] = Random.Range(1, 4);
                        Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2, y - _blockPlate.BlockPlateHeight / 2, 0);
                        GameObject gemPrefab = GetBlockTile(_blockDataArray[y, x]);
                        _blockArray[y, x] = Instantiate(gemPrefab, position, Quaternion.identity);
                    }
                }
            }
        }

        _blankCount = 0;
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
                    _blockDataArray[y, x] = Random.Range(1, 6);
                }
                else
                {
                    _blockDataArray[y, x] = 0; // 빈 블록
                }

                _blockArray[y, x] = null;
            }
        }
    }

    private GameObject GetBlockTile(int blockNum)
    {
        switch (blockNum)
        {
            case 1:
                return _block1;
            case 2:
                return _block2;
            case 3:
                return _block3;
            case 4:
                return _block4;
            case 5:
                return _block5;
            default:
                return null; // Should not happen
        }
    }

    /// <summary>
    /// 테스트 코드/ 하단 ㅗ자로 블록을 삭제합니다.
    /// </summary>
    private void TestDeleteBlock()
    {
        _blockDataArray[0, 1] = 0;
        _blockDataArray[0, 2] = 0;
        _blockDataArray[0, 3] = 0;
        _blockDataArray[1, 2] = 0;
        Destroy(_blockArray[0, 1]);
        Destroy(_blockArray[0, 2]);
        Destroy(_blockArray[0, 3]);
        Destroy(_blockArray[1, 2]);
        _blockArray[0, 1] = null;
        _blockArray[0, 2] = null;
        _blockArray[0, 3] = null;
        _blockArray[1, 2] = null;
    }

    private void RefreshBlock()
    {
        for (int x = 0; x < _blockPlate.BlockPlateWidth; x++)
        {
            for (int y = 0; y < _blockPlate.BlockPlateHeight; y++)
            {
                if (_blockDataArray[y, x] == 0 && _blockPlate.BlockPlateArray[y, x])
                {
                    if (_blockArray[y, x] != null)
                    {
                        Destroy(_blockArray[y, x]);
                        _blockArray[y, x] = null;
                    }
                }

                if (_blockPlate.BlockPlateArray[y, x] && _blockDataArray[y, x] != 0)
                {
                    if (_blockPlate.BlockPlateWidth % 2 == 0)
                    {
                        Destroy(_blockArray[y, x]);
                        Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2 + 0.5f, y - _blockPlate.BlockPlateHeight / 2 + 0.5f, 0);
                        GameObject gemPrefab = GetBlockTile(_blockDataArray[y, x]);
                        Debug.Log($"Spawning block at {y}, {x} with type {_blockDataArray[y, x]}");
                        _blockArray[y, x] = Instantiate(gemPrefab, position, Quaternion.identity);
                    }
                    else
                    {
                        Destroy(_blockArray[y, x]);
                        Vector3 position = new Vector3(x - _blockPlate.BlockPlateWidth / 2, y - _blockPlate.BlockPlateHeight / 2, 0);
                        GameObject gemPrefab = GetBlockTile(_blockDataArray[y, x]);
                        Debug.Log($"Spawning block at {y}, {x} with type {_blockDataArray[y, x]}");
                        _blockArray[y, x] = Instantiate(gemPrefab, position, Quaternion.identity);
                    }
                }
            }
        }
    }

    private IEnumerator TestCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        SortBlockDataArray();
        RefreshBlock();

        yield return new WaitForSeconds(0.25f);
        SpawnBlock();

        _testCoroutine = null;
    }
}
