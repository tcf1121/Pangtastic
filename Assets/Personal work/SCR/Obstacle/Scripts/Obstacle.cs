using System;
using System.Linq.Expressions;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace SCR
{
    public abstract class Obstacle : MonoBehaviour
    {
        [Serializable]
        public class LockStateData
        {
            public Sprite Sprite;
        }

        public LockStateData[] LockStates;

        protected SpriteRenderer spriteRenderer;

        protected Vector3Int _cellPos;

        protected GemType _blockType;
        protected int _currentHP;
        protected int _score;
        protected bool _canMove;
        protected bool _onCollider;
        protected bool _isSpawn;
        protected bool _isSplashDamage;

        private bool _isDone = false;

        public virtual void Init(Vector3Int cell)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = LockStates[0].Sprite;
            _cellPos = cell;

            transform.position = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0);
            //보드의 cell 위치에 장애 블록 추가
            Board.AddObstacle(cell, this);
        }

        public virtual GemType GetBlockType()
        {
            return _blockType;
        }

        public virtual void Clear()
        {
            //해당 오브젝트 삭제
            int x = (int)(transform.position.x - 0.5f);
            int y = (int)(transform.position.y - 0.5f);
            Board.RemoveGem(new Vector3Int(x, y, 0));
            Destroy(gameObject);
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

            if (_currentHP < LockStates.Length)
            {
                spriteRenderer.sprite = LockStates[_currentHP].Sprite;
                return false;
            }

            _isDone = true;
            return true;
        }
    }

}
