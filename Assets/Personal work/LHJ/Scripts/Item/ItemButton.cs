using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LHJ
{
    public class ItemButton : MonoBehaviour
    {
        [SerializeField] private ItemType _type;
        [SerializeField] private ItemCheck _checker;
        [SerializeField] private Button _button;

        private bool _selected = false;

        private void Awake()
        {
            if (_button == null) _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);

            if (_checker != null)
                _checker.OnDeselected += ForceDeselect;
        }

        private void OnDestroy()
        {
            if (_checker != null)
                _checker.OnDeselected -= ForceDeselect;
        }

        private void OnClick()
        {
            if (_checker == null) return;

            if (_selected)
            {
                _checker.Deselect();
                _selected = false;
            }
            else
            {
                _checker.Select(_type);
                _selected = true;
            }
        }

        public void ForceDeselect()
        {
            _selected = false;
        }
    }
}