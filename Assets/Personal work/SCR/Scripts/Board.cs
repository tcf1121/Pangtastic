using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCR
{
    public enum GemType
    {
        Lavender,
        Chocolate,
        Blueberry,
        Cheese,
        Strawberry,
        Sugar,
        RollingPin_v,
        RollingPin_h,
        Milk,
        Popcorn,
        DonutBox,
        Cloche,
        Syrup,
        Coin,
        GiftBox,
        Egg,
        CatStatues,
        Ice,
        Dough,
        Empty
    }

    [DefaultExecutionOrder(-9999)]
    public class Board : MonoBehaviour
    {
        private static Board instance;

        public List<Vector3Int> SpawnPoint = new();
        public Dictionary<Vector3Int, GemType> CellType = new();
        public Dictionary<Vector3Int, GemType> CellNonCollider = new();
        public Dictionary<Vector3Int, BoardCell> CellContent = new();
        [SerializeField] private PrefabList prefabList;

        private Grid _grid;

        public void Awake()
        {
            instance = this;
            GetReference();
        }

        // 빈칸 추가
        public static void AddCell(Vector3Int pos)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            if (!instance.CellContent.ContainsKey(pos))
                instance.CellContent.Add(pos, new BoardCell());
        }

        // 스포너 추가
        public static void AddSpawner(Vector3Int pos)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            instance.SpawnPoint.Add(pos);
        }

        // 장애 블록 추가
        public static void AddObstacle(Vector3Int pos, Obstacle obstacle)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            if (!instance.CellContent.ContainsKey(pos)) AddCell(pos);
            instance.CellContent[pos].SetObstacle(pos, obstacle);
        }

        public static void SetCatStatues(Vector3Int pos)
        {
            Vector3Int fourpos;
            Obstacle obstacle = instance.CellContent[pos].GetCatStatues().GetComponent<Obstacle>();
            fourpos = new Vector3Int(pos.x + 1, pos.y);
            if (!instance.CellContent.ContainsKey(fourpos)) AddCell(fourpos);
            instance.CellContent[fourpos].SetObstacle(fourpos, obstacle, true);
            fourpos = new Vector3Int(pos.x + 1, pos.y + 1);
            if (!instance.CellContent.ContainsKey(fourpos)) AddCell(fourpos);
            instance.CellContent[fourpos].SetObstacle(fourpos, obstacle, true);
            fourpos = new Vector3Int(pos.x, pos.y + 1);
            if (!instance.CellContent.ContainsKey(fourpos)) AddCell(fourpos);
            instance.CellContent[fourpos].SetObstacle(fourpos, obstacle, true);
        }


        // 도넛 추가
        public static void AddDonut(Vector3Int pos, Donut donut)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            if (!instance.CellContent.ContainsKey(pos)) AddCell(pos);
            instance.CellContent[pos].SetDonut(pos, donut);
        }

        // 블록 삭제
        public static void RemoveGem(Vector3Int pos)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            instance.CellType.Remove(pos);
            instance.CellContent[pos].RemoveCell();
        }

        // 블록 삭제
        public static GameObject GetPrefab(GemType gemType)
        {
            return instance.prefabList.GemPrefab[(int)gemType];
        }

        // 움직일 수 있는 블럭인지 확인
        public static bool IsCanMove(Vector3Int pos)
        {
            if (!instance.CellType.ContainsKey(pos))
                return false;

            return instance.CellContent[pos].CanMove();
        }
        // 현재 공간이 비어있는지 확인 후
        public static void IsEmpty(Vector3Int pos)
        {
            Vector3Int movePos;
            // 비어있으면
            if (!instance.CellType.ContainsKey(pos))
            {
                // 위에 부터 확인
                if (IsCanMove(movePos = new Vector3Int(pos.x, pos.y - 1)))
                {
                    MoveDown(movePos, pos);
                }
                // 아니면 왼쪽 대각선 위
                else if (IsCanMove(movePos = new Vector3Int(pos.x - 1, pos.y - 1)))
                {
                    MoveDown(movePos, pos);
                }
                // 아니면 오른쪽 대각선 위
                else if (IsCanMove(movePos = new Vector3Int(pos.x + 1, pos.y - 1)))
                {
                    MoveDown(movePos, pos);
                }
                // 아무것도 찾지 못했으면 아무것도 하지 않는다.
                else
                {
                    return;
                }
            }
        }

        // 비어있으면 위에서 가져옴
        public static void MoveDown(Vector3Int pos, Vector3Int moveToPos)
        {
            GemType value = instance.CellType[pos];
            instance.CellType.Remove(pos);

            instance.CellType.Add(moveToPos, value);
        }

        public static Dictionary<Vector3Int, BoardCell> GetDictionary()
        {
            return instance.CellContent;
        }



        private void GetReference()
        {
            _grid = GetComponent<Grid>();
        }
    }
}
