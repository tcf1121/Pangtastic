using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomerOrder : MonoBehaviour
{
    [SerializeField] private StageSetting _stageSetting;

    public List<RecipeSO> CurRecipes = new List<RecipeSO>();

    [SerializeField] private UnityEvent onAllOrdersComplete;

    private List<Dictionary<IngredientSO, int>> _requiredPerRecipe = new List<Dictionary<IngredientSO, int>>(); // �����Ǻ� ���� �䱸 ��� ��ųʸ�
    private List<Dictionary<IngredientSO, int>> _collectedPerRecipe = new List<Dictionary<IngredientSO, int>>(); // �����Ǻ� ���� ��� ��ųʸ�
    private List<bool> isRecipeCompleted = new List<bool>(); // �����Ǻ� �Ϸ� ����

    private void Awake()
    {
        if (_stageSetting == null)
        {
            _stageSetting = FindObjectOfType<StageSetting>();
        }
    }
    public void AddIngredient(IngredientSO ingredient) // ����� ���� �� ������ ���
    {
        if (_requiredPerRecipe == null || _requiredPerRecipe.Count == 0) // �ֹ��� ������
        {
            Debug.LogWarning("�ֹ��� �����ϴ�.");
            return;
        }

        for (int i = 0; i < _requiredPerRecipe.Count; i++) // �� �����Ǹ� ������� �˻�
        {
            if (isRecipeCompleted[i]) // �̹� �Ϸ�� �����Ǹ�
            {
                continue; // �ǳʶ�
            }

            Dictionary<IngredientSO, int> reqIng = _requiredPerRecipe[i]; // �ʿ��� ��� ��ųʸ�
            Dictionary<IngredientSO, int> colIng = _collectedPerRecipe[i]; // ���� ��� ��ųʸ�

            if (reqIng.ContainsKey(ingredient) == false) // �� �����ǿ� ���� ��ᰡ �ʿ������
            {
                continue; // ���� �����Ƿ� �Ѿ
            }

            int need = reqIng[ingredient]; // ��� �䱸 ����
            int cur = 0; // ���� ���� ����

            if (colIng.TryGetValue(ingredient, out cur) == false) // ���� ��ųʸ��� Ű�� ������
            {
                cur = 0;
            }

            if (cur >= need) // �̹� �䱸ġ�� �����ߴٸ�
            {
                continue; // ���� �����ǿ� �ʿ����� �˻�
            }

            colIng[ingredient] = cur + 1; // ��� 1�� ����

            Debug.Log($"{CurRecipes[i].Name} ��� {ingredient.Name} {colIng[ingredient].ToString()} / {need.ToString()}"); 
            // ���� ��Ȳ

            if (IsRecipeComplete(i)) // �ش� �����ǰ� �ϼ��Ǿ����� �˻�
            {
                isRecipeCompleted[i] = true; // �Ϸ� �÷��� ����
                Debug.Log("[�Ϸ�] ������ Ŭ����: " + CurRecipes[i].Name); // �Ϸ� �α�

                if (IsAllComplete()) // ��� �����ǰ� �Ϸ�Ǿ����� �˻�
                {
                    Debug.Log("��� �ֹ� �Ϸ�!"); 
                    if (onAllOrdersComplete != null)
                    {
                        onAllOrdersComplete.Invoke(); // �մ� Ŭ���� �̺�Ʈ �߻�
                    }
                }
            }
            break; // ��ᰡ ���� �ݺ��� ����
        }
    }

    public void OnOrderReceived() // �ֹ��� ������ �� ȣ���� �Լ�
    {
        _requiredPerRecipe.Clear(); // ���� �ֹ�����Ʈ �ʱ�ȭ
        _collectedPerRecipe.Clear(); // ���� �ֹ�����Ʈ �ʱ�ȭ
        isRecipeCompleted.Clear(); // ���� �ֹ�����Ʈ �ʱ�ȭ

        for (int i = 0; i < CurRecipes.Count; i++) // ������ ����Ʈ�� ��ȸ
        {
            RecipeSO recipe = CurRecipes[i]; // i��° ������ ��������

            Dictionary<IngredientSO, int> required; // �ʿ��� ��� ����� ��ųʸ�
            _stageSetting.CurRecipe = recipe; // ���� �����Ǹ� StageSetting�� ����
            _stageSetting.InitStageRecipe(); // �������� ���� ��� ����
            required = _stageSetting.GetRequiredIngs(); // ���� �䱸 ��� ��ųʸ� ��������

            Dictionary<IngredientSO, int> reqCopy = new Dictionary<IngredientSO, int>(); // StageRecipeSet ���� ��ųʸ��� ������ ���ο� ��ųʸ�
            foreach (KeyValuePair<IngredientSO, int> ing in required) // ����
            {
                reqCopy[ing.Key] = ing.Value; // Ű�� �� ����
            }
            _requiredPerRecipe.Add(reqCopy); // �����Ǻ� �䱸 ��Ͽ� �߰�

            Dictionary<IngredientSO, int> colInit = new Dictionary<IngredientSO, int>(); // ���� ��ųʸ� �ʱ�ȭ��
            foreach (KeyValuePair<IngredientSO, int> kv2 in reqCopy) // �䱸 Ű���� ��������
            {
                colInit[kv2.Key] = 0; // ���� ������ 0���� ����
            }
            _collectedPerRecipe.Add(colInit); // �����Ǻ� ���� ��Ͽ� �߰�

            isRecipeCompleted.Add(false); // ���� �Ϸ���� �ʾҴٰ� ǥ��

            Debug.Log("�ֹ��޴�: " + recipe.Name); // ������ �̸�

            foreach (KeyValuePair<IngredientSO, int> _ing in reqCopy) // �䱸 ��� ��ȸ
            {
                IngredientSO ingredient = _ing.Key; // ��� SO
                int amount = _ing.Value; // �ʿ� ����
                string ingName = (ingredient != null) ? ingredient.Name : "Unknown"; // �̸� Ȯ��
                Debug.Log(" - " + ingName + " x" + amount.ToString()); // �α� ���
            }
        }
    }

    private bool IsRecipeComplete(int index) // Ư�� �����ǰ� �Ϸ�Ǿ����� �˻��ϴ� �Լ�
    {

        Dictionary<IngredientSO, int> req = _requiredPerRecipe[index]; // �䱸 ��ųʸ� ����
        Dictionary<IngredientSO, int> col = _collectedPerRecipe[index]; // ���� ��ųʸ� ����

        foreach (KeyValuePair<IngredientSO, int> kv in req) // ��� �䱸 �׸��� ��ȸ
        {
            IngredientSO ing = kv.Key; // ���� �˻� ���� ���
            int need = kv.Value; // �䱸 ����
            int have = 0; // ���� ���� ����
            if (col.TryGetValue(ing, out have) == false) // ������ Ű�� ������
            {
                return false; // ���� �������� ���� ������ ����
            }
            if (have < need) // �䱸 ������ �̴��̸�
            {
                return false; // �̿Ϸ�
            }
        }
        return true; // ��� �䱸 �׸��� �����Ǹ� �Ϸ�
    }

    private bool IsAllComplete() // ��� �����ǰ� �Ϸ�Ǿ����� �˻��ϴ� �Լ�
    {
        for (int i = 0; i < isRecipeCompleted.Count; i++) // �Ϸ� �÷��� �迭�� ��ȸ
        {
            if (isRecipeCompleted[i] == false) // �ϳ��� �̿Ϸᰡ ������
            {
                return false; // ��ü �̿Ϸ�
            }
        }
        return true; // ���� �Ϸ�� true
    }
}
