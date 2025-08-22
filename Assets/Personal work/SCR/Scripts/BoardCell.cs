using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class BoardCell : MonoBehaviour
    {
        private Vector3Int _pos;
        private Donut _donut;
        // 고양 지폐, 선물 상자, 계란, 고양이 동상
        private Obstacle _obstacle;
        // 클로체, 얼음만 확인
        private Obstacle _donutObstacle;
        // 시럽, 반죽 뭉치만 확인
        private Obstacle _cellObstacle;

        public void SetDonut(Vector3Int pos, Donut donut)
        {
            _pos = pos;
            if (donut == null)
            {
                _donut = new();
                _donut.RandomDonut();
            }
            else _donut = donut;
            var newDonut = Instantiate(Board.GetPrefab(_donut.DonutType));
            newDonut.transform.position = _pos;
        }

        public void SetObstacle(Vector3Int pos, Obstacle obstacle, bool isFour = false)
        {
            _pos = pos;
            if (obstacle.GetCollider())
            {
                if (obstacle.GetBlockType() == GemType.Ice)
                {
                    _donutObstacle = obstacle;
                    SetDonut(pos, null);
                }
                else _obstacle = obstacle;
            }
            else
            {
                _cellObstacle = obstacle;
                SetDonut(pos, null);
            }
            if (isFour) return;
            InstantiateObstacle();
        }

        public GameObject GetCatStatues()
        {
            return _obstacle.CatGameObject;
        }

        private void InstantiateObstacle()
        {

            if (_obstacle != null)
            {
                if (_obstacle.GetBlockType() == GemType.CatStatues)
                {
                    if (!_obstacle.IsFourPos(_pos)) return;
                }
                _obstacle.CatGameObject = Instantiate(Board.GetPrefab(_obstacle.GetBlockType()));
                _obstacle.CatGameObject.transform.position = _pos;
            }
            if (_donutObstacle != null)
            {
                var newDonut = Instantiate(Board.GetPrefab(_donutObstacle.GetBlockType()));
                newDonut.transform.position = _pos;
            }
            if (_cellObstacle != null)
            {
                var newDonut = Instantiate(Board.GetPrefab(_cellObstacle.GetBlockType()));
                newDonut.transform.position = _pos;
            }
        }

        public void RemoveCell()
        {
            if (_obstacle != null)
                _obstacle = null;
        }

        public GemType getCellType()
        {
            if (_obstacle != null)
            {
                return _obstacle.GetBlockType();
            }
            else if (_donutObstacle != null)
            {
                if (_donutObstacle.GetBlockType() == GemType.Cloche)
                    return _obstacle.GetBlockType();
                else return _donut.DonutType;
            }
            else return _donut.DonutType;
        }

        public bool CanMove()
        {
            if (_obstacle != null)
            {
                if (_obstacle.GetBlockType() == GemType.Cloche ||
                    _obstacle.GetBlockType() == GemType.Ice ||
                    _obstacle.GetBlockType() == GemType.CatStatues)
                {

                    return false;
                }
                else return true;
            }
            else return true;
        }

        // 주변 피해로 데미지를 입었을 때
        public void SplashDamage()
        {
            if (_obstacle != null)
            {
                _obstacle.Damage(1);
            }
            if (_donutObstacle != null)
            {
                _obstacle.Damage(1);
            }
        }

        // 직접 데미지를 입었을 때
        public void Damage()
        {
            if (_obstacle != null)
            {
                _obstacle.Damage(1);
            }
            if (_donutObstacle != null)
            {
                _obstacle.Damage(1);
            }
            if (_donut != null)
            {
                _donut.Damage();
            }
            if (_cellObstacle != null)
            {
                _cellObstacle.Damage(1);
            }
        }
    }
}

