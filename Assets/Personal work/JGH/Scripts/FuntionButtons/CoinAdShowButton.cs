using UnityEngine;
using UnityEngine.UI;

public class CoinAdShowButton : MonoBehaviour
{
    [SerializeField] private Button bmxButton; 

    private void Start()
    {
        if (AdSystem.Instance == null) return;

        bmxButton.onClick.AddListener(() => AdSystem.Instance.ShowAD());
    }
}
