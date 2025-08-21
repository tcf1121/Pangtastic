using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerFlowController : MonoBehaviour
{
    [SerializeField] private StageSO _curStage; // 현재 스테이지 SO
    [SerializeField] private CustomerOrderController _customerOrder; // 주문 오케스트레이터
    [SerializeField] private Image customerImage; // 손님 이미지
    [SerializeField] private float _nextCustomerSpawnDelayTime = 1.5f; // 손님 딜레이

    private List<CustomerSO> _customerPool = new List<CustomerSO>();
    List<CustomerSO> _normals = new List<CustomerSO>(); //노멀 손님 담기용

    private Queue<CustomerSO> _customerQueue = new Queue<CustomerSO>(); // 이번 스테이지 손님 순서 큐
    private CustomerSO _curCustomer;

    private int _successCount;
    private int _failureCount;

    [SerializeField] private Button spawnButton; //테스트용 버튼

    private void Awake()
    {
        if (_customerOrder == null)
        {
            _customerOrder = FindObjectOfType<CustomerOrderController>();
        }
        _customerOrder.OnCustomerSuccess += OnCustomerSuccess;
        _customerOrder.OnCustomerFail += OnCustomerFail;
        _customerOrder.OnSpecialCustomerSuccess += OnSpecialCustomerSuccess;
        _customerOrder.OnSpecialCustomerFail += OnSpecialCustomerFail;

        spawnButton.onClick.AddListener(StartCustomerCycle); //테스트용 버튼
    }

    private void OnDestroy()
    {
        _customerOrder.OnCustomerSuccess -= OnCustomerSuccess;
        _customerOrder.OnCustomerFail -= OnCustomerFail;
        _customerOrder.OnSpecialCustomerSuccess -= OnSpecialCustomerSuccess;
        _customerOrder.OnSpecialCustomerFail -= OnSpecialCustomerFail;
    }

    public void StartCustomerCycle()
    {
        _successCount = 0;
        _failureCount = 0;

        BuildCustomerQueue();
        SpawnNextCustomer();
    }

    private void BuildCustomerQueue()
    {
        _customerQueue.Clear();
        _normals.Clear();

        _customerPool = new List<CustomerSO>(_curStage.CustomerList); //스테이지 손님 목록을 리스트에 담음
        //Debug.Log($"총 손님 수 : {_customerPool.Count}");

        foreach (CustomerSO customer in _customerPool) //스테이지 손님 리스트 순회
        {
            if (customer.Type == CustomerType.Normal) //손님 타입이 노멀이면 리스트에 넣음
            {
                _normals.Add(customer);
            }
        }
        //Debug.Log($"노멀 타입 수 : {_normals.Count}");

        if (_normals.Count > 0) //노멀 타입이 리스트에 있을경우 
        {
            int randIndex = Random.Range(0, _normals.Count);
            CustomerSO firstCustomer = _normals[randIndex];
            //Debug.Log($"첫 손님 : {firstCustomer}");

            _customerQueue.Enqueue(firstCustomer); // 큐에 처음으로 넣어서 첫손님으로 지정
            _customerPool.Remove(firstCustomer); // 풀에서 제거
        }

        while (_customerPool.Count > 0) //풀이 0이될때까지 랜덤으로 뽑아서 큐에 넣고 리스트에서는 삭제
        {
            int rand = Random.Range(0, _customerPool.Count);
            _customerQueue.Enqueue(_customerPool[rand]);
            _customerPool.RemoveAt(rand);
        }

        //손님 리스트 로그
        //CustomerSO[] arr = _customerQueue.ToArray();
        //
        //string log = "손님 순서: ";
        //
        //for (int i = 0; i < arr.Length; i++)
        //{
        //    log += arr[i].Name;
        //    if (i < arr.Length - 1)
        //        log += ", ";
        //}
        //
        //Debug.Log(log);
    }

    private void SpawnNextCustomer()
    {
        //Debug.Log("손님 호출");
        if (_customerQueue.Count <= 0) //모든 손님을 처리했으면
        {
            CheckStageEnd();
            return;
        }

        _curCustomer = _customerQueue.Dequeue(); //큐에서 손님 하나 뺌

        //Debug.Log($"현재 손님 : {_curCustomer}");
        customerImage.sprite = _curCustomer.CustomerPic;

        _customerOrder.StartCustomerOrder(_curCustomer, _curStage); //손님 주문
    }

    private void OnCustomerSuccess(float percentage)
    {
        _successCount++;
        Debug.Log($"손님 성공 남은 인내심 {percentage}%. 성공 카운트 : {_successCount}");

        CheckStageEnd();
    }

    private void OnSpecialCustomerSuccess(CustomerSO customer, float averagePercent)
    {
        _successCount++;
        //스페셜 손님 보상로직 
        Debug.Log($"스페셜 손님 클리어. 평균 인내심: {averagePercent}. 보상 제공 짠");
        CheckStageEnd();
    }

    private void OnCustomerFail()
    {
        _failureCount++;
        Debug.Log($"손님 실패. 실패 카운트 : {_failureCount}");
        CheckStageEnd();
    }

    private void OnSpecialCustomerFail(CustomerSO customer)
    {
        _failureCount++;
        Debug.Log($"손님 실패. 실패 카운트 : {_failureCount}");
        CheckStageEnd();
    }

    private void CheckStageEnd() //스테이지 끝났는지 확인
    {
        int cleared = _successCount;
        int left = _customerQueue.Count;

        
        if (left + cleared < _curStage.StageClearCustomerCount)
        {
            Debug.Log("스테이지 실패");
            //스테이지 실패 로직 여기에

            Debug.Log($"남은 손님 수 : {left}, 클리어 손님 수 : {cleared}, 실패손님 수 : {_failureCount}, 클리어 조건 : {_curStage.StageClearCustomerCount}");
            return;
        }
        else if (left > 0)
        {
            StartCoroutine(NextCustomerRoutine());
        }
        else
        {
            Debug.Log("스테이지 클리어");
            //스테이지 클리어 로직 여기에 
            Debug.Log($"남은 손님 수 : {left}, 클리어 손님 수 : {cleared}, 실패손님 수 : {_failureCount}, 클리어 조건 : {_curStage.StageClearCustomerCount}");

            //Debug.Log($"남은 손님 수 : {left}");
        }

        
    }

    private IEnumerator NextCustomerRoutine()
    {
        yield return new WaitForSeconds(_nextCustomerSpawnDelayTime);
        SpawnNextCustomer();
    }

}
