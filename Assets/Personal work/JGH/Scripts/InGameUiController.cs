using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUiController : MonoBehaviour
{
    public void ChangeOutGameScene()
    {
        SceneManager.LoadScene("JGH_OutGameUI");
    }
}
