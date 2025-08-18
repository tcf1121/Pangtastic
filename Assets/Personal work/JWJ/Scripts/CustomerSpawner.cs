using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private StageSetting _stageSetting; // 스테이지 세팅 스크립트
    [SerializeField] private CustomerOrder _customerOrder; // 주문 처리 스크립트
    [SerializeField] private Button spawnButton; //테스트용 버튼

    private List<int> _recipeList = new List<int>();

    private List<CustomerSO> _normalCus = new List<CustomerSO>();
    private List<CustomerSO> _uniqueCus = new List<CustomerSO>();
    private List<CustomerSO> _specialCus = new List<CustomerSO>();

    [SerializeField] private Image customerImage; // 손님 이미지

    private void Awake()
    {
        spawnButton.onClick.AddListener(SpawnRandomCustomer);

        if(_stageSetting == null)
        {
            _stageSetting = FindObjectOfType<StageSetting>();
        }
    }

    public void SpawnRandomCustomer()
    {
        _customerOrder.CurRecipes.Clear(); // 기존 주문 비우기

        CustomerSO chosen = PickCustomerByWeight(_stageSetting.CurStage);

        Debug.Log($"{chosen.Name} 등장"); // 손님 이름 로그 출력
        customerImage.sprite = chosen.CustomerPic; //손님 사진 설정


        OrderMenu(chosen); //주문
    }

    private CustomerSO PickCustomerByWeight(StageSO stage)
    {
        CustomerSO randomCustomer;

        _normalCus.Clear();
        _uniqueCus.Clear();
        _specialCus.Clear();

        var cusList = stage.CustomerList; //현재 스테이지 손님 리스트

        for (int i = 0; i < cusList.Length; i++)
        {
            if (cusList[i] == null) 
            { 
                continue; 
            }

            if (cusList[i].Type == CustomerType.Normal) //손님 타입이 노멀이면
            {
                _normalCus.Add(cusList[i]);
            }

            else if (cusList[i].Type == CustomerType.Unique) //손님 타입이 유니크면
            {
                _uniqueCus.Add(cusList[i]);
            }

            else if (cusList[i].Type == CustomerType.Special) //손님 타입이 스페셜이면
            {
                _specialCus.Add(cusList[i]);
            }
        }

        Debug.Log($"노멀타입 수 : {_normalCus.Count}, 유니크타입 수 : {_uniqueCus.Count}, 스페타입 수 : {_specialCus.Count}");

        int weightNormal = stage.WeightNormal;
        int weightUnique = stage.WeightUnique;
        int weightSpecial = stage.WeightSpecial;

        //특정 타입의 손님이 없을경우 가중치는 0으로 바꿔줌
        if (_normalCus.Count == 0)
        { 
            weightNormal = 0; 
        }
        if (_uniqueCus.Count == 0) 
        { 
            weightUnique = 0; 
        }
        if (_specialCus.Count == 0) 
        { 
            weightSpecial = 0; 
        }

        Debug.Log($"[가중치] 노멀: {weightNormal}, 유니크:{weightUnique}, 스페셜: {weightSpecial}");

        int totalNumber = weightNormal + weightUnique + weightSpecial;

        int randomNum = Random.Range(0, totalNumber);
        Debug.Log($"랜덤숫자 : {randomNum}");
        {
            if(randomNum < weightNormal)
            {
                int rand = Random.Range(0, _normalCus.Count);
                randomCustomer = _normalCus[rand];
                Debug.Log($"노멀 뽑음 : {randomCustomer.Name}");
                return randomCustomer;
            }

            else if (randomNum < weightNormal + weightUnique)
            {
                int rand = Random.Range(0, _uniqueCus.Count);
                randomCustomer = _uniqueCus[rand];
                Debug.Log($"유니크 뽑음 : {randomCustomer.Name}");
                return randomCustomer;
            }

            else
            {
                int rand = Random.Range(0, _specialCus.Count);
                randomCustomer = _specialCus[rand];
                Debug.Log($"스페셜 뽑음 : {randomCustomer.Name}");
                return randomCustomer;
            }
        }
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
}
