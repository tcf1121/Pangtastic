using KDJ;
using System.Collections;
using UnityEngine;

public class SyrupAnim : MonoBehaviour
{
    [Header("애니메이션 세팅")]
    public float duration = 0.1f;
    public AnimationCurve scaleX;
    public AnimationCurve scaleY;

    public IEnumerator SyrupAnimation()
    {
        BoardManager.Instance.IsWaitingForAnimation = true;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / duration;
            float scaleXValue = scaleX.Evaluate(normalizedTime);
            float scaleYValue = scaleY.Evaluate(normalizedTime);
            transform.localScale = new Vector3(scaleXValue, scaleYValue, 1f);
            yield return null;
        }
    }
}
