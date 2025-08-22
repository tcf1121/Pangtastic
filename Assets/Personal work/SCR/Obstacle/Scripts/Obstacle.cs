using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace SCR
{
    public abstract class Obstacle : MonoBehaviour
    {
        public GemType _blockType;
        public GameObject CatGameObject;


        protected Vector3Int _cellPos;
        protected List<Vector3Int> _fourCellPos;


        protected int _currentHP;
        protected int _score;
        protected bool _canMove;
        protected bool _onCollider;
        protected bool _isSpawn;
        protected bool _isSplashDamage;

        private bool _isDone = false;

        public virtual void Init(Vector3Int cell)
        {
            _cellPos = cell;

            transform.position = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0);
            //보드의 cell 위치에 장애 블록 추가
            Board.AddObstacle(cell, this);
        }

        public GemType GetBlockType()
        {
            return _blockType;
        }

        public bool GetCollider()
        {
            return _onCollider;
        }

        public virtual void Clear()
        {
            //해당 오브젝트 삭제
            int x = (int)(transform.position.x - 0.5f);
            int y = (int)(transform.position.y - 0.5f);
            Board.RemoveGem(new Vector3Int(x, y, 0));
            Destroy(gameObject);
        }

        public bool IsFourPos(Vector3Int pos)
        {
            if (pos == _cellPos) return true;
            else return false;
        }

        // 매치 시 주변에 있을 때 파괴되는건지 확인
        public bool IsSplash()
        {
            return _isSplashDamage;
        }

        public void Damage(int amount)
        {
            if (ChangeState(_currentHP + amount)) Clear();
        }

        protected bool ChangeState(int newState)
        {
            if (_isDone)
                return false;

            _currentHP = newState;


            _isDone = true;
            return true;
        }
    }

}
