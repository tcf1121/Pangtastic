using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KDJ;

namespace LHJ
{
    public class ItemButton : MonoBehaviour
    {
        [SerializeField] private ItemType _type; // 버튼에 해당하는 아이템 종류
        [SerializeField] private Button _button;
        [SerializeField] private BoardManager _board;
        [SerializeField] private ItemCheck _itemCheck;

        private void Awake()
        {
            if (_button == null) _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (_type == ItemType.DonutPan)
            {
                if (_itemCheck != null)
                    _itemCheck.UseDonutPan();  
                return;
            }

            // 즉시 발동형: 커피
            if (_type == ItemType.Coffee)
            {
                if (_itemCheck != null)
                    _itemCheck.UseCoffee(30f); 
                return;
            }


            // 가위, 거품기 선택/해제
            if (_board == null) return;

            if (_board.IsItemSelected && _board.SelectedItemType == _type)
                _board.ClearItemSelection();
            else
                _board.SelectItem(_type);
        }
    }
}