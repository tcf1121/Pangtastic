using KDJ;
using SCR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomerFlowController : MonoBehaviour
{
    [SerializeField] private CustomerOrderController _customerOrder;
    //[SerializeField] private BoardManager _boardManager;
    [SerializeField] private Image _customerImage; // 손님 이미지

    private CustomerSO _curCustomer;

    public event Action<CustomerSO> OnCustomerSpawn;
    public event Action OnStageCleared;
    public event Action OnStageFailed;

    [SerializeField] private Button spawnButton; //테스트용 버튼

    private void Awake()
    {
        if (_customerOrder == null)
        {
            _customerOrder = FindObjectOfType<CustomerOrderController>();
        }
        //if (_boardManager == null)
        //{
        //    _boardManager = FindObjectOfType<BoardManager>();
        //}
        _customerOrder.OnStageClear += OnStageClear;
        _customerOrder.OnCustomerFail += OnCustomerFail;
        //_customerOrder.OnSpecialCustomerSuccess += OnSpecialCustomerSuccess;
        //_customerOrder.OnSpecialCustomerFail += OnSpecialCustomerFail;
        _customerOrder.OnOrderSegmentCleared += OnOrderSegmentCleared;

        //spawnButton.onClick.AddListener(SpawnCustomer); //테스트용 버튼
    }

    private void OnDestroy()
    {
        _customerOrder.OnStageClear -= OnStageClear;
        _customerOrder.OnCustomerFail -= OnCustomerFail;
        //_customerOrder.OnSpecialCustomerSuccess -= OnSpecialCustomerSuccess;
        //_customerOrder.OnSpecialCustomerFail -= OnSpecialCustomerFail;
        _customerOrder.OnOrderSegmentCleared -= OnOrderSegmentCleared;
    }

    public void SpawnCustomer()
    {
        StageSO curStage = Manager.Stage.CurrentStage;

        _curCustomer = curStage.Customer;

        _customerImage.sprite = _curCustomer.CustomerPic;

        OnCustomerSpawn?.Invoke(_curCustomer);

        _customerOrder.StartCustomerOrder(_curCustomer, curStage); //손님 주문

        Debug.Log($"현재 스테이지 {curStage.StageID}");
    }

    // 250919 김동진 수정
    // async/await 패턴 적용 그외에 수정 없음
    private async void OnOrderSegmentCleared(CustomerSO curCustomer, float percentage) //스페셜 손님 전용. 주문 하나 완료 할때마다 호출
    {
        if (curCustomer.Type != CustomerType.Special) //스페셜손님 아니면 리턴
        {
            return;
        }

        int percent = Mathf.FloorToInt(percentage);
        Debug.Log($"스페셜 중간 성공 인내심 {percent}%. 보상제공");
        List<GemType> rewardGive = new();
        for (int i = 0; i < 2; i++) //보상 개수 두개
        {
            int rand = UnityEngine.Random.Range(0, 100);
            Debug.Log($"랜덤 숫자 {rand}");
            if (rand >= 90)
            {
                Debug.Log("도넛상자 제공");
                rewardGive.Add(GemType.DonutBox);
            }
            else if (rand >= 68)
            {
                Debug.Log("오븐 제공");
                rewardGive.Add(GemType.DonutBox);
            }
            else if (rand >= 46)
            {
                Debug.Log("우유 제공");
                rewardGive.Add(GemType.Milk);
            }
            else if (rand >= 23)
            {
                Debug.Log("밀대 세로 제공");
                rewardGive.Add(GemType.Milk);
            }
            else if (rand >= 0)
            {
                Debug.Log("밀대 가로 제공");
                rewardGive.Add(GemType.Roller_h);
            }
        }
        await InGameManager.RewardGem(rewardGive);  //보상 제공
    }

    private void OnCustomerFail()
    {
        StageFail();
    }

    // 250919 김동진 수정
    // async/await 패턴 적용 그외에 수정 없음
    private async void OnStageClear(CustomerSO customer, float percentage)  // 스테이지 클리어시 호출
    {
        int percent = Mathf.FloorToInt(percentage); //int 로 변경

        Debug.Log($"스테이지 클리어. 보상 기준 인내심{percent}");

        List<GemType> rewardGive = new();
        int num = 0;
        //특수블록 보상 도넛상자 제외
        if (percent >= 70)
        {
            //_boardManager.Spawner.SpawnRandomBlock
            Debug.Log("블록 4개 제공");
            num = 4;
        }
        else if (percent >= 40)
        {
            Debug.Log("블록 3개 제공");
            num = 3;
        }
        else if (percent >= 17)
        {
            Debug.Log("블록 2개 제공");
            num = 2;
        }
        else if (percent >= 0)
        {
            Debug.Log("블록 1개 제공");
            num = 1;
        }

        for (int i = 0; i < num; i++)
        {
            rewardGive.Add((GemType)UnityEngine.Random.Range(6, 10));
        }

        await InGameManager.RewardGem(rewardGive);
        InGameManager.AddScore(percent * 10); //인내심기준 점수 전송

        OnStageCleared?.Invoke(); //스테이지 클리어 이벤트
        //StartCoroutine(TmpChangeSceneRoutine()); //임시 씬 넘기기 코루틴
    }

    private void StageFail()
    {
        Debug.Log("스테이지 실패");
        OnStageFailed?.Invoke();

        //StartCoroutine(TmpChangeSceneRoutine()); //임시 씬 넘기기 코루틴
    }

    private IEnumerator TmpChangeSceneRoutine() //임시 씬 넘기기 코루틴
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(2/*로비씬*/);
    }
}