using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{

    public class Donut : MonoBehaviour
    {
        public GemType DonutType;
        private int score = 10;

        private Vector3Int _cellPos;
        private bool _onCollider;

        public void RandomDonut()
        {
            DonutType = (GemType)Random.Range(0, 6);
        }

        public void Init(Vector3Int cell)
        {
            _cellPos = cell;

            transform.position = new Vector3(cell.x, cell.y, 0);
            //보드의 cell 위치에 도넛 추가
            //Board.AddObstacle(cell, this);
        }

        public virtual GemType GetBlockType()
        {
            return DonutType;
        }

        public void Damage()
        {
            //해당 오브젝트 삭제
            int x = (int)(transform.position.x - 0.5f);
            int y = (int)(transform.position.y - 0.5f);
            Board.RemoveGem(new Vector3Int(x, y, 0));
            // 점수 주기
            Destroy(gameObject);
        }
    }
}