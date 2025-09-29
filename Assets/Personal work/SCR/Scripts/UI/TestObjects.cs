using UnityEngine;

public class TestObjects : MonoBehaviour
{
    [SerializeField] GameObject parents;
    void Awake()
    {
        if (!Manager.Data.GetTest())
        {
            gameObject.SetActive(false);
            if (parents != null)
            {
                parents.SetActive(false);
                parents.SetActive(true);
            }
        }

    }
}
