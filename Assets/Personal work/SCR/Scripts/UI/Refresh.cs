using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refresh : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Manager.Language.ChangedLanguage += Refreshed;
    }

    void OnDestroy()
    {
        Manager.Language.ChangedLanguage -= Refreshed;
    }

    private void Refreshed()
    {
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
}
