using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ClearUI : MonoBehaviour
{
    [SerializeField] private RectTransform star;    // 움직일 별
    [SerializeField] private RectTransform targetStar;  // 목표 위치 (별 카운트 아이콘)
    [SerializeField] private RectTransform coin;    // 움직일 코인
    [SerializeField] private RectTransform targetCoin;  // 목표 위치 (코인 카운트 아이콘)
    [SerializeField] private Button button;
    private int completedCount = 0;
    private int totalToComplete = 2;
    private Action finMove;

    void Awake()
    {
        Manager.User.AddHeart();

        button.onClick.AddListener(MoveUI);
        finMove += ClearGame;
    }

    public void MoveUI()
    {
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
            completedCount++;
            if (completedCount >= totalToComplete)
                finMove?.Invoke();
        });
    }
}
