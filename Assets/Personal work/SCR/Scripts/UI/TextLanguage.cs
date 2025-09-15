using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextLanguage : MonoBehaviour
{
    [SerializeField] private StringSO _stringSO;
    [SerializeField] private TMP_Text _text;

    public void ChangeLanguage()
    {
        Debug.Log(_text);
        _text.font = Manager.Language.GetFont();
        if (_stringSO != null)
            _text.text = _stringSO.GetText(Manager.Language.GetLanguage());
    }
}
