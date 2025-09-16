using UnityEngine;

public class LobbyPopUp : MonoBehaviour
{
    [SerializeField] MapMover mapMover;

    public void CheckPopup()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                mapMover.isMove = true;
                return;
            }
        }

        mapMover.isMove = false;
    }
}
