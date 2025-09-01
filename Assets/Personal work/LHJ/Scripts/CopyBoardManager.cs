using KDJ;
using LHJ;

public class CopyBoardManager : BoardManager
{
    public static CopyBoardManager Instance { get; private set; }


    public bool IsItemSelected { get; private set; } = false;
    public ItemType SelectedItemType { get; private set; }
    private void OnEnable()
    {
        Instance = this;
    }

    public void SelectItem(ItemType type)
    {
        SelectedItemType = type;
        IsItemSelected = true;
    }

    public void ClearItemSelection()
    {
        IsItemSelected = false;
    }
}
