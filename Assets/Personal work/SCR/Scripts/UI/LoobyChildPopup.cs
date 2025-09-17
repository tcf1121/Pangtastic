using UnityEngine;

public class LoobyChildPopup : MonoBehaviour
{
    private LobbyPopUp parent;

    private void Awake()
    {
        parent = GetComponentInParent<LobbyPopUp>();
    }

    private void OnEnable()
    {
        parent.CheckPopup();
    }

    private void OnDisable()
    {
        parent.CheckPopup();
    }
}
