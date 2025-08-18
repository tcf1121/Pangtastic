using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private StageSetting _stageSetting; // �������� ���� ��ũ��Ʈ
    [SerializeField] private CustomerOrder _customerOrder; // �ֹ� ó�� ��ũ��Ʈ
    [SerializeField] private Button spawnButton; //�׽�Ʈ�� ��ư

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
        _customerOrder.CurRecipes.Clear(); // ���� �ֹ� ����

        int customerIndex = _stageSetting.CurStage.CustomerList.Length; //���������� �մ� ����Ʈ ��

        int rand = Random.Range(0, customerIndex); // ���� �ε��� �̱�
        CustomerSO chosen = _stageSetting.CurStage.CustomerList[rand]; // ���� �մ� ����

        Debug.Log($"{chosen.Name} ����"); // �մ� �̸� �α� ���
        
        OrderMenu(chosen);
    }

    private void OrderMenu(CustomerSO customer)
    {
        int orderNum = Random.Range(2, 4);

        _recipeList.Clear(); // ������ ��� �ʱ�ȭ

        for(int i = 0; i < customer.FavoriteRecipes.Length; i++)  //�ߺ��ֹ� ������. ����Ʈ�� �մ� �ֹ���� ����
        {
            _recipeList.Add(i);
        }

        for (int i = 0; i < orderNum && _recipeList.Count > 0; i++) // �������� �ֹ����� ����ŭ �ݺ������� �ֹ� ����� �����ϸ� ����
        {
            int randIndex = Random.Range(0, _recipeList.Count); //������ ��Ͽ��� �������� ����

            int recipeIndex = _recipeList[randIndex];

            _customerOrder.CurRecipes.Add(customer.FavoriteRecipes[recipeIndex]); //�ֹ��� ����Ʈ�� �߰�

            _recipeList.RemoveAt(randIndex); //�ֹ����� ��Ͽ��� �ε��� ����

        }

        _customerOrder.OnOrderReceived(); // �ֹ� ����
    }
}
