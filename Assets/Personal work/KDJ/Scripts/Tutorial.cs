using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using KDJ;
using KDJ.States;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private List<GameObject> _maskImages;
    [SerializeField] private GameObject _donutInfo;
    [SerializeField] private Image _handImage;
    [SerializeField] private float _handMoveDuration = 0.5f;
    public int Count = 0;

    public void PlayTutorial(Vector3 startPos)
    {
        _maskImages[Manager.User.GetStage()].SetActive(true);
        _handImage.enabled = true;
        StartCoroutine(HandMoveCoroutine(startPos));
    }

    public IEnumerator HandMoveCoroutine(Vector3 startPos)
    {
        BoardManager.Instance.IsTutorialPlayed = true;
        Vector3 SPos = startPos - new Vector3(0, 1f, 0);
        Vector3 screenStartPos = Camera.main.WorldToScreenPoint(SPos);
        Vector3 endPos = SPos + new Vector3(-1.5f, 0, 0); // 손가락이 이동할 끝 위치 설정
        Vector3 screenEndPos = Camera.main.WorldToScreenPoint(endPos);
        // 사용자의 입력으로 상태가 변경될때까지 반복
        while (true)
        {
            float elapsedTime = 0f;
            _handImage.color = new Color(1f, 1f, 1f, 1f); // 손가락 이미지 초기화
            // 튜토리얼 손가락 이동
            while (elapsedTime < _handMoveDuration)
            {
                if (BoardManager.Instance.CurrentState is ReadyState == false) goto EndAnimation;
                _handImage.transform.position = Vector3.Lerp(screenStartPos, screenEndPos, (elapsedTime / _handMoveDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 서서히 사라지는 효과
            elapsedTime = 0f;
            while (elapsedTime < 0.3f)
            {
                if (BoardManager.Instance.CurrentState is ReadyState == false) goto EndAnimation;
                _handImage.color = new Color(1f, 1f, 1f, 1f - (elapsedTime / 0.3f));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _handImage.color = new Color(1f, 1f, 1f, 0f); // 완전히 투명하게 설정

            yield return new WaitForSeconds(0.5f); // 잠시 대기

            yield return null;
        }

        // 매칭이 이루어지면 애니메이션 종료
        EndAnimation:

        // 튜토리얼 종료 시 손가락 이미지와 마스크 이미지 비활성화
        _handImage.enabled = false;
        _maskImages[Manager.User.GetStage()].SetActive(false);

        yield return new WaitWhile(() => BoardManager.Instance.IsBoardBusy);

        yield return new WaitForSeconds(0.5f);

        // 목표 도넛 정보창 튜토리얼 활성화
        _donutInfo.SetActive(true);

        // 사용자의 첫 번째 입력을 기다림
        while (true)
        {
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                _donutInfo.SetActive(false);
                break;
            }
            yield return null;
        }
    }
}

