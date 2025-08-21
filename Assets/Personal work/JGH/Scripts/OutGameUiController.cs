using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OutGameUiController : MonoBehaviour
{
    public void ChangeInGameScene()
    {
        SceneManager.LoadScene("JGH_InGameUI");
    }
}
