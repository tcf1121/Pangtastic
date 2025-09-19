using TMPro;
using UnityEngine;

public class GoodsIHIcon : MonoBehaviour
{
    [SerializeField] TMP_Text _indexText;
    public float Index { get { return index; } }
    private float index;
    public void SetIndex(float num)
    {
        index = num;
        if (num >= 1f)
            _indexText.text = $"{num}h";
        else
            _indexText.text = $"{(int)(num * 60)}m";
    }
}
