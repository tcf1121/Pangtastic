using KDJ;
using LHJ;

public class CopyBoardManager : BoardManager
{

    public bool IsItemSelected { get; private set; } = false;
    public ItemType SelectedItemType { get; private set; }

    // 아이템 선택
    public void SelectItem(ItemType type)
    {
        SelectedItemType = type;
        IsItemSelected = true;
    }
    
    // 아이템 선택 해제
    public void ClearItemSelection()
    {
        IsItemSelected = false;
    }
}
