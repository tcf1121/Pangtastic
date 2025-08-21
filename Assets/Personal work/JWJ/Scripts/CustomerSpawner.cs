using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private StageSetting _stageSetting; // 스테이지 세팅 스크립트
    [SerializeField] private CustomerOrder _customerOrder; // 주문 처리 스크립트
    [SerializeField] private Button spawnButton; //테스트용 버튼
    [SerializeField] private float _nextCustomerSpawnDelayTime; //다음손님 등장까지 지연시간

    private List<int> _recipeList = new List<int>();

    private List<CustomerSO> _normalCus = new List<CustomerSO>();
    private List<CustomerSO> _uniqueCus = new List<CustomerSO>();
    private List<CustomerSO> _specialCus = new List<CustomerSO>();

    private int _successCount;
    private int _failureCount;

    [SerializeField] private Image customerImage; // 손님 이미지

    private CustomerSO _curCustomer;
    

    private void Awake()
    {
        spawnButton.onClick.AddListener(SpawnRandomCustomer); //테스트용 버튼
        _customerOrder.OnOrderSucceeded += OnOrderSucceeded;
        _customerOrder.OnOrderFailed += OnOrderFailed;

        if(_stageSetting == null)
        {
            _stageSetting = FindObjectOfType<StageSetting>();
        }
    }

    public void SpawnRandomCustomer()
    {
        _customerOrder.CurRecipes.Clear(); // 기존 주문 비우기

        //_curCustomer = PickCustomerByWeight(_stageSetting.CurStage);

        Debug.Log($"{_curCustomer.Name} 등장"); // 손님 이름 로그 출력
        customerImage.sprite = _curCustomer.CustomerPic; //손님 사진 설정

        _customerOrder.SetCurrentCustomer(_curCustomer);
        OrderMenu(_curCustomer); //주문
    }


    private void OrderMenu(CustomerSO customer)
    {
        int orderNum = Random.Range(2, 4);

        _recipeList.Clear(); // 레시피 목록 초기화

        for(int i = 0; i < customer.FavoriteRecipes.Length; i++)  //중복주문 방지용. 리스트에 손님 주문목록 넣음
        {
            _recipeList.Add(i);
        }

        for (int i = 0; i < orderNum && _recipeList.Count > 0; i++) // 스테이지 주문가능 수만큼 반복하지만 주문 목록이 부족하면 멈춤
        {
            int randIndex = Random.Range(0, _recipeList.Count); //레시피 목록에서 랜덤으로 뽑음

            int recipeIndex = _recipeList[randIndex];

            _customerOrder.CurRecipes.Add(customer.FavoriteRecipes[recipeIndex]); //주문할 리스트에 추가

            _recipeList.RemoveAt(randIndex); //주문가능 목록에서 인데스 제거

        }

        _customerOrder.OnOrderReceived(); // 주문 접수
    }

    public void OnOrderSucceeded(float persent) //주문 성공시 이벤트 호출
    {
        _successCount = _successCount + 1;
        Debug.Log($"{persent.ToString()}%  성공 횟수: {_successCount}");

        //if(_curCustomer.HasReward)
        //{
        //    Debug.Log("보상 제공");
        //}

        StageEndCheck();
    }


    public void OnOrderFailed() //주문 실패시 이벤트 호출
    {
        _failureCount = _failureCount + 1;
        Debug.Log($"주문 실패(시간초과). 실패 횟수: {_failureCount.ToString()}");

        StageEndCheck();
    }

    private void StageEndCheck()
    {
        int leftCustomerCount = _stageSetting.TotalCustomerCount - _successCount - _failureCount;

        if (leftCustomerCount + _successCount < _stageSetting.StageClearCustomerCount)
        {
            Debug.Log("스테이지 실패");
        }
        if (leftCustomerCount > 0)
        {
            StartCoroutine(NextCustomerRoutine());
        }
        else
        {
            Debug.Log("스테이지 클리어");
        }
    }

    private IEnumerator NextCustomerRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        SpawnRandomCustomer();
    }
}
