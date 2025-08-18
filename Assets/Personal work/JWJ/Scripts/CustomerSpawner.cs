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

    private List<CustomerSO> _normalCus = new List<CustomerSO>();
    private List<CustomerSO> _uniqueCus = new List<CustomerSO>();
    private List<CustomerSO> _specialCus = new List<CustomerSO>();

    [SerializeField] private Image customerImage; // �մ� �̹���

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

        CustomerSO chosen = PickCustomerByWeight(_stageSetting.CurStage);

        Debug.Log($"{chosen.Name} ����"); // �մ� �̸� �α� ���
        customerImage.sprite = chosen.CustomerPic; //�մ� ���� ����


        OrderMenu(chosen); //�ֹ�
    }

    private CustomerSO PickCustomerByWeight(StageSO stage)
    {
        CustomerSO randomCustomer;

        _normalCus.Clear();
        _uniqueCus.Clear();
        _specialCus.Clear();

        var cusList = stage.CustomerList; //���� �������� �մ� ����Ʈ

        for (int i = 0; i < cusList.Length; i++)
        {
            if (cusList[i] == null) 
            { 
                continue; 
            }

            if (cusList[i].Type == CustomerType.Normal) //�մ� Ÿ���� ����̸�
            {
                _normalCus.Add(cusList[i]);
            }

            else if (cusList[i].Type == CustomerType.Unique) //�մ� Ÿ���� ����ũ��
            {
                _uniqueCus.Add(cusList[i]);
            }

            else if (cusList[i].Type == CustomerType.Special) //�մ� Ÿ���� ������̸�
            {
                _specialCus.Add(cusList[i]);
            }
        }

        Debug.Log($"���Ÿ�� �� : {_normalCus.Count}, ����ũŸ�� �� : {_uniqueCus.Count}, ����Ÿ�� �� : {_specialCus.Count}");

        int weightNormal = stage.WeightNormal;
        int weightUnique = stage.WeightUnique;
        int weightSpecial = stage.WeightSpecial;

        //Ư�� Ÿ���� �մ��� ������� ����ġ�� 0���� �ٲ���
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

        Debug.Log($"[����ġ] ���: {weightNormal}, ����ũ:{weightUnique}, �����: {weightSpecial}");

        int totalNumber = weightNormal + weightUnique + weightSpecial;

        int randomNum = Random.Range(0, totalNumber);
        Debug.Log($"�������� : {randomNum}");
        {
            if(randomNum < weightNormal)
            {
                int rand = Random.Range(0, _normalCus.Count);
                randomCustomer = _normalCus[rand];
                Debug.Log($"��� ���� : {randomCustomer.Name}");
                return randomCustomer;
            }

            else if (randomNum < weightNormal + weightUnique)
            {
                int rand = Random.Range(0, _uniqueCus.Count);
                randomCustomer = _uniqueCus[rand];
                Debug.Log($"����ũ ���� : {randomCustomer.Name}");
                return randomCustomer;
            }

            else
            {
                int rand = Random.Range(0, _specialCus.Count);
                randomCustomer = _specialCus[rand];
                Debug.Log($"����� ���� : {randomCustomer.Name}");
                return randomCustomer;
            }
        }
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
