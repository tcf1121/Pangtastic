using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnterInGamePanel : MonoBehaviour
{
    [SerializeField] TMP_Text _level;
    [SerializeField] TMP_Text _rollerNum;
    [SerializeField] TMP_Text _donutBoxNum;
    [SerializeField] TMP_Text _ovenNum;
    [SerializeField] Toggle rollerToggle;
    [SerializeField] Toggle donutBoxToggle;
    [SerializeField] Toggle ovenToggle;
    [SerializeField] Button enterBtn;
    [SerializeField] List<Image> recipeImages;
    [SerializeField] Sprite questionMarkImage;
    [SerializeField] StringSO stringSO;
    [SerializeField] GameObject _fallHeartPopup;
    [SerializeField] List<Button> _emptyButton;
    [SerializeField] List<GameObject> _buyButton;

    private StageSO _curStage;
    private CustomerSO _curCustomer;

    List<RecipeSO> _stageRecipes = new List<RecipeSO>();
    void Awake()
    {
        _curStage = Manager.Stage.CurrentStage;
        _curCustomer = _curStage.Customer;

        _stageRecipes.Clear();
        _stageRecipes = RecipeRule.BuildOrder(_curCustomer, _curStage);
        Manager.Stage.SetStageRecipe(_stageRecipes);
        Manager.Language.ChangedLanguage += () =>
        _level.text = $"{Manager.Stage.CurrentStageIndex + 1}{stringSO.GetText(Manager.Language.GetLanguage())}";

        enterBtn.onClick.AddListener(EnterGame);
        SetBuyButton();
        // 여기서 레시피 만들어서 Stage에 넣기
    }

    void OnDestroy()
    {
        Manager.Language.ChangedLanguage -= () =>
        _level.text = $"{Manager.Stage.CurrentStageIndex + 1}{stringSO.GetText(Manager.Language.GetLanguage())}";
    }
    void GetRecipe()
    {
        //List<RecipeSO> recipeSOs = _stageRecipes;
        //List<RecipeSO> recipeSOs = Manager.Stage.CurrentStage.StageRecipes.ToList();
        if (_curCustomer.Type != CustomerType.Special)
        {
            for (int i = 0; i < recipeImages.Count; i++)
            {
                if (i < _stageRecipes.Count)
                {
                    recipeImages[i].sprite = _stageRecipes[i].FoodPic;
                    recipeImages[i].gameObject.SetActive(true);
                }
                else
                    recipeImages[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < recipeImages.Count; i++)
            {
                if (i == 0)
                {
                    recipeImages[0].sprite = questionMarkImage;
                    recipeImages[0].gameObject.SetActive(true);
                }
                else
                    recipeImages[i].gameObject.SetActive(false);
            }
        }
    }

    void OnEnable()
    {
        foreach (var go in _buyButton)
        {
            go.SetActive(false);
        }
        GetRecipe();
        _level.text = $"{Manager.Stage.CurrentStageIndex + 1}{stringSO.GetText(Manager.Language.GetLanguage())}";
        _rollerNum.text = $"{Manager.User.GetItem().Roller}";
        _donutBoxNum.text = $"{Manager.User.GetItem().DonutBox}";
        _ovenNum.text = $"{Manager.User.GetItem().Oven}";
        Manager.Stage.ResetUseItem();
        CheckClearing();
    }

    void CheckClearing()
    {
        if (Manager.User.GetClearing() > 2)
        {
            rollerToggle.isOn = true;
            rollerToggle.interactable = false;
            _emptyButton[0].gameObject.SetActive(false);
            donutBoxToggle.isOn = true;
            donutBoxToggle.interactable = false;
            _emptyButton[0].gameObject.SetActive(false);
            ovenToggle.isOn = true;
            ovenToggle.interactable = false;
            _emptyButton[0].gameObject.SetActive(false);
        }
        else
        {
            SetToggles();
        }
    }

    void SetToggles()
    {
        if (Manager.User.GetItem().Roller <= 0)
        {
            rollerToggle.isOn = false;
            rollerToggle.interactable = false;
            _emptyButton[0].gameObject.SetActive(true);
        }
        else if (Manager.User.GetItem().Roller > 0)
        {
            rollerToggle.isOn = false;
            rollerToggle.interactable = true;
            _emptyButton[0].gameObject.SetActive(false);
        }
        if (Manager.User.GetItem().DonutBox <= 0)
        {
            donutBoxToggle.isOn = false;
            donutBoxToggle.interactable = false;
            _emptyButton[1].gameObject.SetActive(true);
        }
        else if (Manager.User.GetItem().DonutBox > 0)
        {
            donutBoxToggle.isOn = false;
            donutBoxToggle.interactable = true;
            _emptyButton[1].gameObject.SetActive(false);
        }
        if (Manager.User.GetItem().Oven <= 0)
        {
            ovenToggle.isOn = false;
            ovenToggle.interactable = false;
            _emptyButton[2].gameObject.SetActive(true);
        }
        else if (Manager.User.GetItem().Oven > 0)
        {
            ovenToggle.isOn = false;
            ovenToggle.interactable = true;
            _emptyButton[2].gameObject.SetActive(false);
        }
    }

    void EnterGame()
    {
        Debug.Log("게임 시작");
        if (Manager.User.CheckHeart())
        {

            if (rollerToggle.isOn)
            {
                if (Manager.User.GetClearing() < 3)
                    Manager.User.UseItem(ItemType.Roller);
                Manager.Stage.SetUseItem((int)ItemType.Roller);
            }
            if (donutBoxToggle.isOn)
            {
                if (Manager.User.GetClearing() < 3)
                    Manager.User.UseItem(ItemType.DonutBox);
                Manager.Stage.SetUseItem((int)ItemType.DonutBox);
            }
            if (ovenToggle.isOn)
            {
                if (Manager.User.GetClearing() < 3)
                    Manager.User.UseItem(ItemType.Oven);
                Manager.Stage.SetUseItem((int)ItemType.Oven);
            }

            LoadingManager.LoadScene(3);
        }
        else
        {
            _fallHeartPopup.SetActive(true);
        }
    }

    void SetBuyButton()
    {
        for (int i = 0; i < _emptyButton.Count; i++)
        {
            int index = i;
            _emptyButton[index].onClick.AddListener(() => _buyButton[index].SetActive(!_buyButton[index].activeSelf));
        }
    }

}
