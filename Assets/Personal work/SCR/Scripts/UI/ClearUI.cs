using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClearUI : MonoBehaviour
{
    [SerializeField] private RectTransform star;    // 움직일 별
    [SerializeField] private RectTransform targetStar;  // 목표 위치 (별 카운트 아이콘)
    [SerializeField] private RectTransform coin;    // 움직일 코인
    [SerializeField] private RectTransform targetCoin;  // 목표 위치 (코인 카운트 아이콘)
    [SerializeField] private Button button;
    [SerializeField] private GameObject RewardObj;
    [SerializeField] private GameObject textObj;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private Button rewardDubbleButton;
    private int completedCount = 0;
    private int totalToComplete = 2;
    private Action finMove;

    void Awake()
    {
        Manager.User.AddHeart();

        button.onClick.AddListener(MoveUI);
        finMove += ClearGame;
        if (!Manager.Ad.RemovedAD)
        {
            rewardDubbleButton.onClick.AddListener(DoubleRewardButton);
        }
        else
        {
            coinText.text = $"{InGameManager.GetCoin()} x2";
            InGameManager.AddCoin(InGameManager.GetCoin());
            rewardDubbleButton.gameObject.SetActive(false);
        }
    }

    public void MoveUI()
    {
        button.interactable = false;
        RewardObj.SetActive(false);
        textObj.SetActive(false);
        UserInfoUI.Instance.SetActive(true);
        AddGold(coin, targetCoin);
        AddStar(star, targetStar);
    }

    public void ClearGame()
    {
        Manager.Stage.AdvanceStage();
        InGameManager.ClearGame();
    }

    private void MoveUIItem(RectTransform item, RectTransform target,
                           System.Action onComplete = null)
    {
        Vector3 startScale = item.localScale;
        Vector3 targetScale = target.localScale;

        Sequence seq = DOTween.Sequence();
        seq.Append(item.DOMove(target.position, 1f).SetEase(Ease.InOutQuad));
        seq.Join(item.DOScale(targetScale, 1f));
        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    private void AddStar(RectTransform star, RectTransform target)
    {
        MoveUIItem(star, target, () =>
        {
            Manager.User.AddStar(Manager.Stage.CurrentStage.LevelValue);
            Manager.Audio.PlaySFX("Star_Add");
            star.gameObject.SetActive(false);
            completedCount++;
            if (completedCount >= totalToComplete)
                finMove?.Invoke();
        });
    }

    private void AddGold(RectTransform coin, RectTransform target)
    {
        MoveUIItem(coin, target, () =>
        {
            int stageCoin = InGameManager.GetCoin();
            int maxCoin = Manager.Stage.CurrentStage.MaxGoldGain;
            if (stageCoin > maxCoin)
                stageCoin = maxCoin;
            Manager.User.AddCoin(stageCoin);
            Manager.Audio.PlaySFX("Coin_Add");
            coin.gameObject.SetActive(false);
            coinText.gameObject.SetActive(false);
            completedCount++;
            if (completedCount >= totalToComplete)
                finMove?.Invoke();
        });
    }

    private void DoubleRewardButton()
    {
        rewardDubbleButton.gameObject.SetActive(false);
        if (!Manager.Ad.RemovedAD)
        {
            Manager.Ad.OnRewardAdClosed = null;
            Manager.Ad.OnRewardAdClosed += DoubleGold;
            Manager.Ad.ShowAD();
        }
    }

    private void DoubleGold()
    {
        int startValue = InGameManager.GetCoin();
        int endValue = startValue * 2;  // 2배로 증가
        StartCoroutine(AnimateNumber(startValue, endValue, 1f));
    }

    private IEnumerator AnimateNumber(int start, int end, float duration)
    {
        float elapsed = 0f;
        InGameManager.AddCoin(InGameManager.GetCoin());
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int current = Mathf.RoundToInt(Mathf.Lerp(start, end, t));
            coinText.text = current.ToString();
            yield return null;
        }
        coinText.text = end.ToString(); // 마지막 값 보정
    }
}
