using TMPro;
using UnityEngine;

public class GoodsIcon : MonoBehaviour
{
    [SerializeField] TMP_Text _indexText;
    public int Index { get { return index; } }
    private int index;
    public void SetIndex(int num)
    {
        index = num;
        if (num == 1) _indexText.text = $"";
        else _indexText.text = $"{num}";
    }
}