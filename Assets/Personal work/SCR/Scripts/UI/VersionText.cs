using TMPro;
using UnityEngine;

public class VersionText : MonoBehaviour
{
    [SerializeField] TMP_Text versionText;

    void Start()
    {
        versionText.text = $"Ver {Application.version}";
    }
}
