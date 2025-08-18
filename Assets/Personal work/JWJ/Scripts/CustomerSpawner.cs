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

        int customerIndex = _stageSetting.CurStage.CustomerList.Length; //스테이지의 손님 리스트 수

        int rand = Random.Range(0, customerIndex); // 랜덤 인덱스 뽑기
        CustomerSO chosen = _stageSetting.CurStage.CustomerList[rand]; // 랜덤 손님 선택

        Debug.Log($"{chosen.Name} 등장"); // 손님 이름 로그 출력
        
        OrderMenu(chosen);
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
