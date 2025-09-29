using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using KDJ;
using KDJ.States;
using TMPro;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private List<GameObject> _maskImages;
    [SerializeField] private GameObject _donutInfo;
    [SerializeField] private Image _handImage;
    [SerializeField] private TMP_Text _closeText;
    [SerializeField] private float _handMoveDuration = 0.5f;
    [SerializeField] private CanvasScaler _canvas;
    [Range(0f, 1f)]
    [SerializeField] private float _resolutionCorrectMultiplier = 1f;

    public int Count = 0;
    public List<Vector2Int> SwappableBlocks { get; private set; } = new List<Vector2Int>();

    public void PlayTutorial(Vector3 startPos)
    {
        if (Manager.User.GetStage() == 0)
            StartCoroutine(HandMoveCoroutineWithInfo(startPos));
        else
            StartCoroutine(HandMoveCoroutine(startPos));
    }

    /// <summary>
    /// 1스테이지에서 사용할 튜토리얼 코루틴
    /// (손가락 이동 + 도넛 설명)
    /// </summary>
    /// <param name="startPos"></param>
    /// <returns></returns>
    public IEnumerator HandMoveCoroutineWithInfo(Vector3 startPos)
    {
        BoardManager.Instance.IsPlayingTutorial = true;
        BoardManager.Instance.IsTutorialPlayed = true;
        int width = BoardManager.Instance.Spawner.GameBoardData.Width;
        int height = BoardManager.Instance.Spawner.GameBoardData.Height;
        RectTransform handRect = _handImage.transform.parent.GetComponent<RectTransform>();
        Vector2 localSPos, localEPos;
        Vector3 sPos = startPos - new Vector3(0, 1f, 0);
        Vector3 screenStartPos = Camera.main.WorldToScreenPoint(sPos);
        Vector3 ePos = sPos + new Vector3(-1.5f, 0, 0); // 손가락이 이동할 끝 위치 설정
        Vector3 screenEndPos = Camera.main.WorldToScreenPoint(ePos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(handRect, screenStartPos, null, out localSPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(handRect, screenEndPos, null, out localEPos);

        bool animationEnd = false;
        SwappableBlocks.Clear();
        Vector2Int block1GridPos = BoardManager.Instance.BlockMover.WorldToGrid(sPos + new Vector3(-0.5f, 2f, 0), width, height);
        Vector2Int block2GridPos = block1GridPos + new Vector2Int(-1, 0);
        SwappableBlocks.Add(block1GridPos);
        SwappableBlocks.Add(block2GridPos);

        Debug.Log($"이동 가능한 블록 좌표 {block1GridPos}, {block2GridPos}");

        // 튜토리얼 재생 전 해상도 보정
        // ResolutionCorrect();
        SetMatchValue();

        yield return new WaitForSeconds(1f);

        _maskImages[Manager.User.GetStage()].SetActive(true);
        _handImage.enabled = true;

        _handImage.rectTransform.rotation = Quaternion.Euler(0, 0, 30);
        // 사용자의 입력으로 상태가 변경될때까지 반복
        while (true)
        {
            float elapsedTime = 0f;
            _handImage.color = new Color(1f, 1f, 1f, 1f); // 손가락 이미지 초기화
            // 튜토리얼 손가락 이동
            while (elapsedTime < _handMoveDuration)
            {
                if (BoardManager.Instance.CurrentState is ReadyState == false) { animationEnd = true; break; }
                _handImage.rectTransform.anchoredPosition = Vector3.Lerp(localSPos, localEPos, (elapsedTime / _handMoveDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 서서히 사라지는 효과
            elapsedTime = 0f;
            while (elapsedTime < 0.3f)
            {
                if (BoardManager.Instance.CurrentState is ReadyState == false) { animationEnd = true; break; }
                _handImage.color = new Color(1f, 1f, 1f, 1f - (elapsedTime / 0.3f));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _handImage.color = new Color(1f, 1f, 1f, 0f); // 완전히 투명하게 설정

            if (animationEnd) break;

            yield return new WaitForSeconds(0.5f); // 잠시 대기

            yield return null;
        }

        // 튜토리얼 종료 시 손가락 이미지와 마스크 이미지 비활성화
        _handImage.enabled = false;
        _maskImages[Manager.User.GetStage()].SetActive(false);

        yield return new WaitWhile(() => BoardManager.Instance.IsBoardBusy);

        yield return new WaitForSeconds(0.5f);

        // 목표 도넛 정보창 튜토리얼 활성화
        SetMatchValueForResolution();
        _donutInfo.SetActive(true);
        BoardManager.SetTouch(false);

        yield return new WaitForSeconds(1f);

        _closeText.gameObject.SetActive(true);
        bool textAnimationEnd = false;

        while (true)
        {
            float timer = 0f;

            while (timer < 0.75f)
            {
                if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                {
                    _donutInfo.SetActive(false);
                    textAnimationEnd = true;
                    break;
                }

                timer += Time.deltaTime;
                _closeText.color = new Color(1f, 1f, 1f, 0f + timer / 0.75f);
                yield return null;
            }

            _closeText.color = new Color(1f, 1f, 1f, 1f);
            timer = 0f;

            while (timer < 0.75f)
            {
                if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                {
                    _donutInfo.SetActive(false);
                    textAnimationEnd = true;
                    break;
                }

                timer += Time.deltaTime;
                _closeText.color = new Color(1f, 1f, 1f, 1f - timer / 0.75f);
                yield return null;
            }
            _closeText.color = new Color(1f, 1f, 1f, 0f);

            if (textAnimationEnd) break;

            yield return null;
        }
        _closeText.gameObject.SetActive(false);
        BoardManager.SetTouch(true);
        BoardManager.Instance.IsPlayingTutorial = false;
    }

    /// <summary>
    /// 2스테이지부터 사용할 튜토리얼 코루틴
    /// (손가락 이동만)
    /// </summary>
    /// <param name="startPos"></param>
    /// <returns></returns>
    public IEnumerator HandMoveCoroutine(Vector3 startPos)
    {
        BoardManager.Instance.IsPlayingTutorial = true;
        BoardManager.Instance.IsTutorialPlayed = true;
        int width = BoardManager.Instance.Spawner.GameBoardData.Width;
        int height = BoardManager.Instance.Spawner.GameBoardData.Height;
        RectTransform handRect = _handImage.transform.parent.GetComponent<RectTransform>();
        Vector2 localSPos, localEPos;
        Vector3 sPos = startPos - new Vector3(0, 1f, 0);
        Vector3 screenStartPos = Camera.main.WorldToScreenPoint(sPos);
        Vector3 ePos;
        if (Manager.User.GetStage() == 4) // 4스테이지는 손가락 이동 방향이 다름
            ePos = sPos + new Vector3(0, -1.5f, 0); // 4스테이지는 세로
        else
            ePos = sPos + new Vector3(-1.5f, 0, 0); // 손가락이 이동할 끝 위치 설정
        Vector3 screenEndPos = Camera.main.WorldToScreenPoint(ePos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(handRect, screenStartPos, null, out localSPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(handRect, screenEndPos, null, out localEPos);

        bool animationEnd = false;

        SwappableBlocks.Clear();
        Vector2Int block1GridPos = BoardManager.Instance.BlockMover.WorldToGrid(sPos + new Vector3(-0.5f, 2f, 0), width, height);
        Vector2Int block2GridPos;
        if (Manager.User.GetStage() == 4)
        {
            block1GridPos = BoardManager.Instance.BlockMover.WorldToGrid(sPos + new Vector3(-1f, 1, 0), width, height);
            block2GridPos = block1GridPos + new Vector2Int(0, -1);
        }
        else
            block2GridPos = block1GridPos + new Vector2Int(-1, 0);
        SwappableBlocks.Add(block1GridPos);
        SwappableBlocks.Add(block2GridPos);

        Debug.Log($"이동 가능한 블록 좌표 {block1GridPos}, {block2GridPos}");

        // 튜토리얼 재생 전 해상도 보정
        // ResolutionCorrect();
        SetMatchValue();

        yield return new WaitForSeconds(1f);

        _maskImages[Manager.User.GetStage()].SetActive(true);
        _handImage.enabled = true;

        _handImage.rectTransform.rotation = Quaternion.Euler(0, 0, 30);

        // 사용자의 입력으로 상태가 변경될때까지 반복
        while (true)
        {
            float elapsedTime = 0f;
            _handImage.color = new Color(1f, 1f, 1f, 1f); // 손가락 이미지 초기화
            // 튜토리얼 손가락 이동
            while (elapsedTime < _handMoveDuration)
            {
                if (BoardManager.Instance.CurrentState is ReadyState == false) { animationEnd = true; break; }
                _handImage.rectTransform.anchoredPosition = Vector3.Lerp(localSPos, localEPos, (elapsedTime / _handMoveDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 서서히 사라지는 효과
            elapsedTime = 0f;
            while (elapsedTime < 0.3f)
            {
                if (BoardManager.Instance.CurrentState is ReadyState == false) { animationEnd = true; break; }
                _handImage.color = new Color(1f, 1f, 1f, 1f - (elapsedTime / 0.3f));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _handImage.color = new Color(1f, 1f, 1f, 0f); // 완전히 투명하게 설정

            if (animationEnd) break;

            yield return new WaitForSeconds(0.5f); // 잠시 대기

            yield return null;
        }

        // 튜토리얼 종료 시 손가락 이미지와 마스크 이미지 비활성화
        _handImage.enabled = false;
        _maskImages[Manager.User.GetStage()].SetActive(false);

        yield return new WaitWhile(() => BoardManager.Instance.IsBoardBusy);
        BoardManager.Instance.IsPlayingTutorial = false;
    }

    /// <summary>
    /// 현재 사용안함.
    /// </summary>
    public void ResolutionCorrect()
    {
        // 해상도에 따라 ui 보정
        float targetRatio = 9f / 16f;
        float currentRatio = (float)Screen.width / Screen.height;
        float logDiff = Mathf.Log(currentRatio / targetRatio, 2f);
        float multiplier;

        multiplier = _resolutionCorrectMultiplier;
        float multipliedLogDiff = Mathf.Abs(logDiff * multiplier);
        float match = Mathf.Clamp01(multipliedLogDiff);

        Debug.Log("현재 해상도: " + currentRatio + ", 비율 차이: " + logDiff + ", match 값: " + match);

        _canvas.matchWidthOrHeight = match;
    }

    public void SetMatchValue()
    {
        float targetRatio = 9f / 16f;
        float currentRatio = (float)Screen.width / Screen.height;
        float logDiff = Mathf.Log(currentRatio / targetRatio, 2f);
        float match;
        if (logDiff > 0)
        {
            match = 0.25f;
        }
        else
        {
            match = 0.2f;
        }

        Debug.Log("현재 해상도: " + currentRatio + ", 비율 차이: " + logDiff + ", match 값: " + match);

        _canvas.matchWidthOrHeight = match;
    }

    /// <summary>
    /// matchWidthOrHeight를 해상도에 맞게 계산하여 설정
    /// </summary>
    public void SetMatchValueForResolution()
    {
        float targetRatio = 9f / 16f;
        float currentRatio = (float)Screen.width / Screen.height;
        float logDiff = Mathf.Log(currentRatio / targetRatio, 2f);
        float match;

        if (logDiff > 0)
        {
            match = 1f;
        }
        else
        {
            match = 0f;
        }

        _canvas.matchWidthOrHeight = match;
    }

    /// <summary>
    /// 외부에서 matchWidthOrHeight를 지정하여 설정
    /// </summary>
    /// <param name="matchValue"></param>
    public void SetMatchValue(float matchValue)
    {
        float targetRatio = 9f / 16f;
        float currentRatio = (float)Screen.width / Screen.height;
        float logDiff = Mathf.Log(currentRatio / targetRatio, 2f);

        _canvas.matchWidthOrHeight = matchValue;
    }

}

