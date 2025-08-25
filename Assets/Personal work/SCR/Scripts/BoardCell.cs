using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class BoardCell
    {
        private Vector3Int _pos;
        private Donut _donut;
        // 고양 지폐, 선물 상자, 계란, 고양이 동상
        private Obstacle _obstacle;
        // 클로체, 얼음만 확인
        private Obstacle _donutObstacle;
        // 시럽, 반죽 뭉치만 확인
        private Obstacle _cellObstacle;

        public BoardCell(Vector3Int pos, GemType gem)
        {
            SetCellPos(pos);
            SetObject(gem);
        }

        private void SetCellPos(Vector3Int pos)
        {
            _pos = pos;
        }

        public void SetObject(GemType gem)
        {
            if (gem < GemType.RollingPin_v) SetDonut(gem);
            else if (gem < GemType.Dough) { }
            else if (gem < GemType.Empty) SetObstacle(gem);
            if (gem == GemType.Random) SetDonut(gem);
        }

        public void SetDonut(Donut donut)
        {
            _donut = donut;
        }

        private void SetDonut(GemType donut)
        {
            if (donut == GemType.Random)
            {
                donut = (GemType)Random.Range(0, 6);
            }
            _donut = Board.GetDonut(_pos, donut);
        }

        public void SetObstacle(Obstacle obstacle)
        {
            _obstacle = obstacle;
        }

        private void SetObstacle(GemType obstacle)
        {
            if (obstacle < GemType.Ice)
            {
                _cellObstacle = Board.GetObstacle(_pos, obstacle);
                _cellObstacle.Init(_pos);
                SetDonut(GemType.Random);
            }
            else if (obstacle < GemType.Coin)
            {
                if (obstacle == GemType.Ice) SetDonut(GemType.Random);
                _donutObstacle = Board.GetObstacle(_pos, obstacle);
                _donutObstacle.Init(_pos);
            }
            else
            {
                if (obstacle == GemType.CatStatues_s)
                {
                    return;
                }
                _obstacle = Board.GetObstacle(_pos, obstacle);
                _obstacle.Init(_pos);
            }
        }

        public bool IsEmpty()
        {
            if (_obstacle == null && _donut == null && _donutObstacle == null) return true;
            else return false;
        }

        public void RemoveCell()
        {
            if (_obstacle != null)
                _obstacle = null;
        }

        public GemType getCellType()
        {
            if (_donutObstacle != null)
            {
                if (_donutObstacle.GetBlockType() == GemType.Cloche)
                    return GemType.Empty;
                else return _donut.DonutType;
            }
            if (_donut != null) return _donut.DonutType;
            else return GemType.Empty;
        }

        public bool CanMove()
        {
            if (_donutObstacle != null)
                return false;
            if (_obstacle != null && (_obstacle.GetBlockType() == GemType.CatStatues ||
                    _obstacle.GetBlockType() == GemType.CatStatues_s))
                return false;
            return true;
        }

        public void MoveObject(BoardCell moveCell)
        {
            if (_donut != null)
            {
                moveCell.SetDonut(_donut);
                _donut = null;
            }
            if (_obstacle != null && _obstacle.ObstaclType < GemType.CatStatues)
            {
                moveCell.SetObstacle(_obstacle);
                _obstacle = null;
            }
        }

        public Donut GetDonut()
        {
            if (_donut != null)
            {
                return _donut;
            }
            return null;
        }

        public Obstacle GetObstacle()
        {
            if (_obstacle != null)
            {
                return _obstacle;
            }
            return null;
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
                _donutObstacle.Damage(1);
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
                _donutObstacle.Damage(1);
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

