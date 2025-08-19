using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public enum GemType
    {
        Carrot,
        Lemon,
        Grape,
        Strawberry,
        Apple,
        Cabbage,
        Coffee,
        Knife_v,
        Knife_h,
        Milk,
        Honeycomb,
        FruitBasket,
        Box,
        Syrup,
        Coin,
        GiftBox,
        Egg,
        CatStatues,
        Dirt
    }

    [DefaultExecutionOrder(-9999)]
    public class Board : MonoBehaviour
    {
        private static Board instance;

        public List<Vector3Int> SpawnPoint = new();
        public Dictionary<Vector3Int, GemType> CellType = new();
        public Dictionary<Vector3Int, GemType> CellNonCollider = new();
        public Dictionary<Vector3Int, BoardCell> CellContent = new();
        public GemType[,] BlockTypesBoard;

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

        // 블록 추가
        public static void AddObstacle(Vector3Int pos, Obstacle obstacle)
        {
            AddGem(pos, obstacle.GetBlockType());
            instance.CellContent[pos].Obstacle = obstacle;
        }

        // 블록 추가
        public static void AddGem(Vector3Int pos, GemType gemType)
        {
            if (instance == null)
            {
                instance = GameObject.Find("Grid").GetComponent<Board>();
                instance.GetReference();
            }
            if (gemType == GemType.Dirt || gemType == GemType.Syrup)
                instance.CellNonCollider.Add(pos, gemType);
            else
                instance.CellType.Add(pos, gemType);
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

        // 움직일 수 있는 블럭인지 확인
        public static bool IsCanMove(Vector3Int pos)
        {
            if (!instance.CellType.ContainsKey(pos))
                return false;

            if (instance.CellType[pos] == GemType.Syrup ||
                instance.CellType[pos] == GemType.CatStatues ||
                instance.CellType[pos] == GemType.Dirt
                /*|| instance.CellContent[pos]가 아이스 상태일때*/ )
                return false;

            else return true;
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



        private void GetReference()
        {
            _grid = GetComponent<Grid>();
        }
    }
}
