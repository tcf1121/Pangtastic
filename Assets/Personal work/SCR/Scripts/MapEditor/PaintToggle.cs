using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    public class PaintToggle : MonoBehaviour
    {
        ToggleGroup _toggleGroup;
        [SerializeField] GemToggle gemToggle;
        [SerializeField] TileList tile;
        int _num;
        void Awake()
        {
            _toggleGroup = GetComponent<ToggleGroup>();
        }

        void Start()
        {
            for (int i = 0; i < tile.tiles.Count; i++)
            {
                var toggleObj = Instantiate(gemToggle.gameObject, this.gameObject.transform);
                GemToggle newToggle = toggleObj.GetComponent<GemToggle>();
                newToggle.Toggle.group = _toggleGroup;
                newToggle.SetToggle(i);
            }
            gameObject.transform.parent.gameObject.SetActive(false);
        }
    }
}