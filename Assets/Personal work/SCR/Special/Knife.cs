using UnityEngine;

namespace SCR
{
    public class Knife : Special
    {
        private bool _isHorizon;

        public void Init(Vector3Int cell, bool isHorizon)
        {
            base.Init(cell);
            _isHorizon = isHorizon;
        }

        public override bool CheckCondition()
        {
            // 세로 혹은 가로로 4줄
            return false;
        }

        public override void Use()
        {
            if (_isHorizon)
            {
                // 세로로 터트리기
            }


            else
            {
                // 가로로 터트리기
            }
        }

        public override void UseWith(Special special)
        {

        }
    }
}
