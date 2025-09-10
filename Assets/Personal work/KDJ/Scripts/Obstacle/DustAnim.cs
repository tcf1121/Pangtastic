using KDJ;
using System.Collections;
using UnityEngine;

public class DustAnim : MonoBehaviour
{
    [SerializeField] private GameObject dust1;
    [SerializeField] private GameObject dust2;
    [SerializeField] private GameObject dust3;

    public IEnumerator SizeAnim(float startSize, float endSize)
    {
        BoardManager.Instance.IsWaitingForAnimation = true;
        float timer = 0f;

        while (timer < 0.15f)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(startSize, endSize, timer / 0.1f);
            dust1.transform.localScale = new Vector3(scale, scale, 1f);
            dust2.transform.localScale = new Vector3(scale, scale, 1f);
            dust3.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        BoardManager.Instance.IsWaitingForAnimation = false;
    }
}
