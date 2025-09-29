using UnityEngine;

public class ContinueUI : MonoBehaviour
{
    [SerializeField] GameObject _clearingText;

    void OnEnable()
    {
        if (Manager.Stage.GetClearing() > 0)
            _clearingText.SetActive(true);
        else
            _clearingText.SetActive(false);
    }
}
