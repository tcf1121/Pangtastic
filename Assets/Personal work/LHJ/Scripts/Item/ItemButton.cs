using UnityEngine;
using UnityEngine.UI;
using KDJ;
using TMPro;

namespace LHJ
{
    public class ItemButton : MonoBehaviour
    {
        [SerializeField] private ItemType _type; // 버튼에 해당하는 아이템 종류
        [SerializeField] private Button _button;
        [SerializeField] private BoardManager _board;
        [SerializeField] private ItemCheck _itemCheck;
        [SerializeField] private GameObject _lockGO;
        [SerializeField] private TMP_Text _countText;

        private void Awake()
        {
            if (_button == null) _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
            SetItemState();
        }

        private void LateUpdate()
        {
            SetItemState();
        }

        private void OnClick()
        {
            if (_type == ItemType.DonutPan)
            {
                if (_itemCheck != null)
                    _itemCheck.UseDonutPan();

                Manager.User.UseItem(_type);
                SetItemState();
                return;
            }

            // 즉시 발동형: 커피
            if (_type == ItemType.Coffee)
            {
                if (_itemCheck != null)
                    _itemCheck.UseCoffee(30f);
                Manager.User.UseItem(_type);
                SetItemState();
                return;
            }


            // 가위, 거품기 선택/해제
            if (_board == null) return;

            if (_board.IsItemSelected && _board.SelectedItemType == _type)
                _board.ClearItemSelection();
            else
                _board.SelectItem(_type);
        }

        private void Lock()
        {
            if (!Manager.User.CanUseItem(_type))
            {
                _button.interactable = false;
                _lockGO.SetActive(true);
            }
        }

        public void SetItemState()
        {
            Debug.Log("보유 아이템 상태 : " + _type + " " + Manager.User.CanUseItem(_type));
            if (Manager.User.CanUseItem(_type))
            {
                _button.interactable = true;
                int count = GetItemCount(_type);
                if (_countText != null)
                    _countText.text = count.ToString();
                _lockGO.SetActive(false);
            }
            else
            {
                _button.interactable = false;
                _countText.text = "";
                _lockGO.SetActive(true);
            }
        }

        public int GetItemCount(ItemType itemType)
        {
            if (itemType == _type)
            {
                int count = 0;
                if (itemType == ItemType.Roller) count = Manager.User.GetItem().Roller;
                else if (itemType == ItemType.DonutBox) count = Manager.User.GetItem().DonutBox;
                else if (itemType == ItemType.Oven) count = Manager.User.GetItem().Oven;
                else if (itemType == ItemType.Whisk) count = Manager.User.GetItem().Whisk;
                else if (itemType == ItemType.Scissors) count = Manager.User.GetItem().Scissors;
                else if (itemType == ItemType.DonutPan) count = Manager.User.GetItem().DonutPan;
                else if (itemType == ItemType.Coffee) count = Manager.User.GetItem().Coffee;

                return count;
            }

            return 0;
        }
    }
}