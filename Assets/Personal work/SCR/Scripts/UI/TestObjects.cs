using UnityEngine;

public class TestObjects : MonoBehaviour
{
    void Awake()
    {
        if (!Manager.Data.GetTest()) gameObject.SetActive(false);
    }
}
