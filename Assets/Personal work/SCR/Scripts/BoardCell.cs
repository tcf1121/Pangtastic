using LHJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class BoardCell
    {
        private Vector3Int _pos;
        // 기본 블럭
        private Donut _donut;
        // 특수 블럭
        private Special _special;
        // 고양 지폐, 선물 상자, 계란, 고양이 동상
        private Obstacle _obstacle;
        // 클로체, 얼음만 확인
        private Obstacle _donutObstacle;
        // 시럽, 반죽 뭉치만 확인
        private Obstacle _cellObstacle;
        private GemType _firstGemtype;
        private GemType _firstDonut;
        private bool _catS;

        public BoardCell(Vector3Int pos, GemType gem)
        {
            SetCellPos(pos);
            _catS = false;
            if (gem == GemType.Random) _firstGemtype = (GemType)Random.Range(0, 6);
            else
            {
                _firstGemtype = gem;
                if (gem == GemType.Dough ||
                    gem == GemType.Syrup ||
                    gem == GemType.Ice)
                    _firstDonut = (GemType)Random.Range(0, 6);
            }
        }

        private void SetCellPos(Vector3Int pos)
        {
            _pos = pos;
        }

        public void SetObject(GemType gem)
        {
            if (gem < GemType.Roller_v) SetDonut(gem);
            else if (gem < GemType.Dough) SetSpecial(gem);
            else if (gem < GemType.Empty) SetObstacle(gem);
            if (gem == GemType.Random) SetDonut(gem);
        }

        public void Init()
        {
            if (_firstGemtype == GemType.CatStatues_s) _catS = true;
            else SetObject(_firstGemtype);
            _firstGemtype = GemType.Empty;
        }



        public void SetDonut(Donut donut)
        {
            _donut = null;
            _donut = donut;
        }

        private void SetDonut(GemType donut)
        {
            if (donut == GemType.Random) donut = (GemType)Random.Range(0, 6);
            _donut = Board.GetDonut(_pos, donut);
        }

        public void SetObstacle(Obstacle obstacle)
        {
            _obstacle = null;
            _obstacle = obstacle;
        }

        private void SetObstacle(GemType obstacle)
        {
            if (obstacle < GemType.Ice)
            {
                _cellObstacle = Board.GetObstacle(_pos, obstacle);
                _cellObstacle.Init(_pos);
                SetDonut(_firstDonut);
            }
            else if (obstacle < GemType.Coin)
            {
                if (obstacle == GemType.Ice) SetDonut(_firstDonut);
                _donutObstacle = Board.GetObstacle(_pos, obstacle);
                _donutObstacle.Init(_pos);
            }
            else
            {
                _obstacle = Board.GetObstacle(_pos, obstacle);
                _obstacle.Init(_pos);
            }
        }

        public void SetSpecial(Special special)
        {
            _special = null;
            _special = special;
        }

        private void SetSpecial(GemType special)
        {
            _special = Board.GetSpecial(_pos, special);
            _special.Init(_pos);
        }

        public bool IsEmpty()
        {
            if (_catS) return false;
            if (_obstacle == null && _donut == null && _donutObstacle == null && _special == null) return true;
            else return false;
        }

        public GemType getCellType()
        {
            if (_firstGemtype != GemType.Empty)
            {
                if (_firstGemtype < GemType.Roller_v) return _firstGemtype;
                else
                {
                    if (_firstGemtype == GemType.Dough ||
                    _firstGemtype == GemType.Syrup ||
                    _firstGemtype == GemType.Ice) return _firstDonut;
                    return GemType.Empty;
                }
            }
            if (_donutObstacle != null)
            {
                if (_donutObstacle.GetBlockType() == GemType.Cloche)
                    return GemType.Empty;
                else return _donut.DonutType;
            }
            if (_donut != null) return _donut.DonutType;
            else return GemType.Empty;
        }

        public GemType GetCatStatuse()
        {
            if (_catS) return GemType.CatStatues_s;
            if (_obstacle != null &&
            _obstacle.GetBlockType() == GemType.CatStatues)
                return GemType.CatStatues;
            else return GemType.Empty;
        }

        public bool CanMove()
        {
            if (_donutObstacle != null)
                return false;
            if (_obstacle != null && _obstacle.GetBlockType() == GemType.CatStatues)
                return false;
            if (_catS) return false;
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
            if (_special != null)
            {
                moveCell.SetSpecial(_special);
                _special = null;
            }
        }

        public IEnumerator SetPos(float duration)
        {
            float elapsedTime = 0f;
            Transform target = null;
            if (_donut != null)
            {
                target = _donut.gameObject.transform;
            }
            else if (_special != null)
            {
                target = _special.gameObject.transform;
            }
            else if (_obstacle != null && _obstacle.ObstaclType < GemType.CatStatues)
            {
                target = _obstacle.gameObject.transform;
            }
            if (target != null)
                if (target.gameObject != null)
                {
                    Vector3 startPosition = target.position;

                    while (elapsedTime < duration)
                    {
                        if (target != null)
                            target.position = Vector3.Lerp(startPosition, _pos, (elapsedTime / duration));
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }
                    if (target != null)
                        target.position = _pos;
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

        public Special GetSpecial()
        {
            if (_special != null)
            {
                return _special;
            }
            return null;
        }

        // 주변 피해로 데미지를 입었을 때
        public void SplashDamage()
        {
            if (_obstacle != null)
            {
                _obstacle.Damage();
                if (_obstacle != null)
                    if (_obstacle.gameObject == null)
                        _obstacle = null;
            }
            if (_donutObstacle != null)
            {
                if (_donutObstacle.GetBlockType() == GemType.Cloche)
                {
                    _donutObstacle.Damage();
                    if (_donutObstacle != null)
                        if (_donutObstacle.gameObject == null)
                            _donutObstacle = null;
                }
            }
        }

        // 직접 데미지를 입었을 때
        public void Damage()
        {
            if (_obstacle != null)
            {
                _obstacle.Damage();
                if (_obstacle != null)
                    if (_obstacle.gameObject == null)
                        _obstacle = null;
            }
            if (_donutObstacle != null)
            {
                _donutObstacle.Damage();
                if (_donutObstacle != null)
                    if (_donutObstacle.gameObject == null)
                        _donutObstacle = null;
            }
            if (_donut != null)
            {
                _donut.Damage();
                if (_donut != null)
                    if (_donut.gameObject == null)
                        _donut = null;
            }
            if (_cellObstacle != null)
            {
                _cellObstacle.Damage();
                if (_cellObstacle != null)
                    if (_cellObstacle.gameObject == null)
                        _cellObstacle = null;
            }
            if (_special != null)
            {
                _special.Damage();
                if (_special != null)
                    if (_special.gameObject == null)
                        _special = null;
            }

        }

        public void DestroyCat()
        {
            if (_catS) _catS = false;
        }

        public void UseTwoSpecial(Special special = null)
        {
            if (special = null)
            {
                _special.UsedSpecial();
                if (_special != null)
                    if (_special.gameObject == null)
                        _special = null;
            }
            else
            {
                _special.Use(special);
                if (_special != null)
                    if (_special.gameObject == null)
                        _special = null;
            }
        }
    }
}

