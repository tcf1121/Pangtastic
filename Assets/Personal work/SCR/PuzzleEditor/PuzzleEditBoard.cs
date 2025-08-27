using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR
{

    [DefaultExecutionOrder(-9999)]
    public class PuzzelEditBoard : MonoBehaviour
    {
        private static PuzzelEditBoard instance;
        public Tilemap BlankTilemap;
        public Tilemap SpawnerTilemap;
        public Tilemap GemTilemap;
        [SerializeField] private Tilemap _mainTilemap;
        [SerializeField] private Grid _grid;
        [SerializeField] TileList tileList;

        public List<Vector3Int> SpawnPoint = new();
        public List<Vector3Int> CellList = new();
        public Dictionary<Vector3Int, GemType> CellGemType = new();
        private int _cellCount;
        private Vector3Int _clickPos;
        private int _selectTile = 0;
        private float _cameraZ;
        private bool _isClick = false;


        public void Awake()
        {
            instance = this;
            GetReference();
            instance._cellCount = GetTotalTilesOnMap();
        }

        public void Update()
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<PuzzelEditBoard>();
                instance.GetReference();
            }
            //     AllCheck();


            if (Input.GetKeyDown(KeyCode.Q))
            {
                SortCells();
            }

        }

        void OnMouseDown()
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<PuzzelEditBoard>();
                instance.GetReference();
            }
            _isClick = true;
        }

        void OnMouseUp()
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<PuzzelEditBoard>();
                instance.GetReference();
            }
            _isClick = false;
        }

        void OnMouseOver()
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<PuzzelEditBoard>();
                instance.GetReference();
            }

            if (_isClick)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = -_cameraZ;
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
                _clickPos = _mainTilemap.WorldToCell(mouseWorldPos);
                DrawTileMap(_clickPos, tileList.tiles[instance._selectTile].Tile);
            }

        }

        public static void SetClickPos(Vector3Int pos)
        {
            instance._clickPos = pos;
        }

        public static void SelectTile(int num)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<PuzzelEditBoard>();
                instance.GetReference();
            }
            instance._selectTile = num;
        }

        public static Dictionary<Vector3Int, GemType> GetPuzzleInfo()
        {
            instance.SortCells();
            return instance.CellGemType;
        }

        public static List<Vector3Int> GetSpawnPoint()
        {
            return instance.SpawnPoint;
        }

        private int GetTotalTilesOnMap()
        {
            int count = 0;
            // 타일맵의 유효한 셀 영역을 순회
            BoundsInt bounds = BlankTilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (BlankTilemap.HasTile(pos))
                {
                    count++;
                }
            }
            return count;
        }

        public static void DrawTileMap(Vector3Int pos, TileBase tile)
        {
            if (tile == instance.tileList.Cell())
            {
                instance.BlankTilemap.SetTile(pos, tile);
                AddCell(pos);
            }
            else if (tile == instance.tileList.Spawner())
            {
                instance.SpawnerTilemap.SetTile(pos, tile);
                AddSpawner(pos);
            }
            else if (tile == instance.tileList.Eraser())
            {
                instance.BlankTilemap.SetTile(pos, null);
                instance.SpawnerTilemap.SetTile(pos, null);
                instance.GemTilemap.SetTile(pos, null);
                DeleteObject(pos);
            }
            else if (tile == instance.tileList.CatStatues())
            {
                instance.GemTilemap.SetTile(pos, tile);
                instance.GemTilemap.SetTile(pos + Vector3Int.up, null);
                instance.GemTilemap.SetTile(pos + Vector3Int.right, null);
                instance.GemTilemap.SetTile(pos + Vector3Int.up + Vector3Int.right, null);
                DrawObject(pos, GemType.CatStatues);
            }
            else
            {
                instance.GemTilemap.SetTile(pos, tile);
                DrawObject(pos, (GemType)instance._selectTile);
            }
        }

        public static void DeleteObject(Vector3Int pos)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<PuzzelEditBoard>();
                instance.GetReference();
            }
            if (instance.CellGemType.ContainsKey(pos)) instance.CellGemType.Remove(pos);
            if (instance.CellList.Contains(pos)) instance.CellList.Remove(pos);
            if (instance.SpawnPoint.Contains(pos)) instance.SpawnPoint.Remove(pos);
        }


        public static void DrawObject(Vector3Int pos, GemType gem)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<PuzzelEditBoard>();
                instance.GetReference();
            }
            if (!instance.CellGemType.ContainsKey(pos))
            {
                Debug.Log("오브젝트 추가");
                instance.CellGemType.Add(pos, gem);
            }
            else
                instance.CellGemType[pos] = gem;
        }

        // 빈칸 추가
        public static void AddCell(Vector3Int pos)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<PuzzelEditBoard>();
                instance.GetReference();
            }
            Debug.Log("빈칸 추가");
            if (!instance.CellList.Contains(pos)) instance.CellList.Add(pos);

        }

        // 스포너 추가
        public static void AddSpawner(Vector3Int pos)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<PuzzelEditBoard>();
                instance.GetReference();
            }
            Debug.Log("스포너 추가");
            if (!instance.SpawnPoint.Contains(pos)) instance.SpawnPoint.Add(pos);

        }

        public void SortCells()
        {
            CellList = CellList.OrderBy(pos => pos.x).ThenBy(pos => pos.y).ToList();
            CellList.Reverse();
        }

        private void GetReference()
        {
            _grid = GetComponent<Grid>();
        }
    }
}
