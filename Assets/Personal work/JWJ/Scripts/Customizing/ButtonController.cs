using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    private Dictionary<CosmeticCategory, CustomizingButton> _currentByCategory = new Dictionary<CosmeticCategory, CustomizingButton>();

    public void Select(CosmeticCategory category, CustomizingButton button)
    {
        CustomizingButton prev;
        bool hasPrev = _currentByCategory.TryGetValue(category, out prev);

        if (hasPrev == true && prev != null && prev != button)
        {
            prev.SetSelected(false);
        }

        _currentByCategory[category] = button;
        button.SetSelected(true);
    }

    public void Clear(CosmeticCategory category)
    {
        if (_currentByCategory.ContainsKey(category) == true)
        {
            _currentByCategory.Remove(category);
        }
    }
}
