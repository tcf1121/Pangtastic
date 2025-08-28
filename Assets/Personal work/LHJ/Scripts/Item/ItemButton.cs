using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LHJ
{
    public class ItemButton : MonoBehaviour
    {
        [SerializeField] private ItemType _type;
        [SerializeField] private Button _button;

        private void Awake()
        {
            if (_button == null) _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            var manager = CopyBoardManager.Instance;
            if (manager == null) return;

            if (manager.IsItemSelected && manager.SelectedItemType == _type)
                manager.ClearItemSelection();
            else
                manager.SelectItem(_type);
        }
    }
}