using UnityEngine;
using UnityEngine.UI;

public class CoinAdShowButton : MonoBehaviour
{
    [SerializeField] private Button bmxButton;

    private void Start()
    {
        if (Manager.Ad == null) return;

        bmxButton.onClick.AddListener(() => Manager.Ad.ShowAD());
    }
}
