using UnityEngine;

namespace SCR
{
    public abstract class Special : MonoBehaviour
    {
        public Sprite Sprite;

        protected SpriteRenderer spriteRenderer;

        protected Vector3Int _cellPos;

        private bool _isDone = false;

        public virtual void Init(Vector3Int cell)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprite;
            _cellPos = cell;

            //보드의 cell 위치에 특수 블록 추가
        }

        // 사용할 때의 효과
        public virtual void Use()
        {

        }

        // 생성 조건 확인
        public virtual bool CheckCondition()
        {
            return false;
        }

        // 다른 특수 효과 아이템이랑 사용할 때의 효과
        public virtual void UseWith(Special special)
        {

        }

    }

}
