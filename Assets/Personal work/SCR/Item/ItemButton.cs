using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    [RequireComponent(typeof(Button))]
    public class ItemButton : MonoBehaviour
    {
        [SerializeField] private ItemType _type;
        private Button _button;

        private bool _selected = false;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }


        public void OnClick()
        {
            if (_selected)
            {
                ItemCheck.Deselect();
                _selected = false;
            }
            else
            {
                ItemCheck.Select(_type);
                _selected = true;
            }
        }

        public void ForceDeselect()
        {
            _selected = false;
        }
    }
}