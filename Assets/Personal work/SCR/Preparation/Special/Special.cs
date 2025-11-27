using UnityEngine;

namespace SCR
{
    public abstract class Special : MonoBehaviour
    {
        public GemType SpecialType;

        protected Vector3Int _cellPos;
        protected bool _isHorizon;
        protected int _hp;

        public virtual void Init(Vector3Int cell, bool isHorizon = false)
        {
            _hp = 1;
            _cellPos = cell;
            _isHorizon = isHorizon;
            transform.position = new Vector3(cell.x, cell.y, 0);
            transform.rotation = transform.rotation;
            //보드의 cell 위치에 특수 블록 추가
        }

        // 데미지를 받았을 때
        public virtual void Damage()
        {
            _hp--;
            if (_hp == 0)
                Use();

        }

        public void UsedSpecial()
        {
            Destroy(gameObject);
        }

        // 특수 블록을 사용할 때의 효과
        public virtual void Use(Special special = null)
        {
            Destroy(gameObject);
            if (special == null)
                Board.UseSpeical(_cellPos, SpecialType);
            else
                Board.UseSpeical(_cellPos, SpecialType, special.SpecialType);
        }

    }

}
