using SCR;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR_B
{
    public class PuzzleBoard : MonoBehaviour
    {
        private static PuzzleBoard instance;
        private Tilemap tilemap;
        Vector3Int _clickPos;
        Vector3Int _dragDir;
        private float _dragThreshold = 0.7f;
        float CameraZ;

        void Awake()
        {
            instance = this;
            tilemap = GetComponent<Tilemap>();
            CameraZ = Camera.main.transform.position.z;
        }

        void Update()
        {
#if UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    OnTouchDown(touch.position);
                }
                if (touch.phase == TouchPhase.Ended)
                {
                    OnTouchUp(touch.position);
                }
            }
#endif

        }

        private void OnTouchDown(Vector3 touchPos)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(touchPos);
            _clickPos = tilemap.WorldToCell(mouseWorldPos);
            if (ItemCheck.IsSelect())
            {
                Debug.Log(ItemCheck.GetItemType());
                ItemCheck.Deselect();
            }
        }

        private void OnTouchUp(Vector3 touchPos)
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(touchPos);

            Vector3 directionVector = mouseWorldPosition - _clickPos;
            if (directionVector.magnitude < _dragThreshold)
            {
                BoardManager.UseItem();
                return;
            }

            _dragDir = Vector3Int.zero;

            if (Mathf.Abs(directionVector.x) > Mathf.Abs(directionVector.y))
            {
                // 좌우 이동
                if (directionVector.x > 0) _dragDir = Vector3Int.right;
                else _dragDir = Vector3Int.left;
            }
            else
            {
                // 상하 이동
                if (directionVector.y > 0) _dragDir = Vector3Int.up;
                else _dragDir = Vector3Int.down;
            }
            BoardManager.Move();
        }
#if UNITY_EDITOR
        void OnMouseDown()
        {

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -CameraZ;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
            _clickPos = tilemap.WorldToCell(mouseWorldPos);
            if (ItemCheck.IsSelect())
            {
                Debug.Log(ItemCheck.GetItemType());
                ItemCheck.Deselect();
            }
        }

        void OnMouseUp()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -CameraZ;
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePos);

            Vector3 directionVector = mouseWorldPosition - _clickPos;
            if (directionVector.magnitude < _dragThreshold)
            {
                BoardManager.UseItem();
                return;
            }


            _dragDir = Vector3Int.zero;

            if (Mathf.Abs(directionVector.x) > Mathf.Abs(directionVector.y))
            {
                // 좌우 이동
                if (directionVector.x > 0) _dragDir = Vector3Int.right;
                else _dragDir = Vector3Int.left;
            }
            else
            {
                // 상하 이동
                if (directionVector.y > 0) _dragDir = Vector3Int.up;
                else _dragDir = Vector3Int.down;
            }
            BoardManager.Move();
        }
#endif
        public static Vector2Int GetStartPos()
        {
            return new Vector2Int(instance._clickPos.x, instance._clickPos.y);
        }

        public static Vector2Int GetEndPos()
        {
            return GetStartPos() + new Vector2Int(instance._dragDir.x, instance._dragDir.y);
        }
    }
}