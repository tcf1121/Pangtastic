using System;
using System.Linq.Expressions;
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

        protected int _currentHP;
        protected int _score;
        protected bool _canMove;
        protected bool _onCollider;
        protected bool _isSpawn;

        private bool _isDone = false;

        public virtual void Init(Vector3Int cell)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = LockStates[0].Sprite;
            _cellPos = cell;

            //보드의 cell 위치에 장애 블록 추가
        }

        public virtual void Clear()
        {

        }

        public virtual bool IsDamage()
        {
            return true;
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
