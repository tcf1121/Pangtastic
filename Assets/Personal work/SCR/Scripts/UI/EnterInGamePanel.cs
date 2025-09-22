using System;
using System.Collections.Generic;
using System.Linq;
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
        enterBtn.onClick.AddListener(EnterGame);
        // 여기서 레시피 만들어서 Stage에 넣기
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
        _level.text = $"{Manager.Stage.CurrentStageIndex + 1}{stringSO.GetText(Manager.Language.GetLanguage())}";
        GetRecipe();
        _rollerNum.text = $"{Manager.User.GetItem().Roller}";
        _donutBoxNum.text = $"{Manager.User.GetItem().DonutBox}";
        _ovenNum.text = $"{Manager.User.GetItem().Oven}";
        Manager.Stage.ResetUseItem();
        SetToggles();
    }

    void SetToggles()
    {
        if (Manager.User.GetItem().Roller <= 0)
        {
            rollerToggle.isOn = false;
            rollerToggle.interactable = false;
        }
        else if (Manager.User.GetItem().Roller > 0)
        {
            rollerToggle.isOn = false;
            rollerToggle.interactable = true;
        }
        if (Manager.User.GetItem().DonutBox <= 0)
        {
            donutBoxToggle.isOn = false;
            donutBoxToggle.interactable = false;
        }
        else if (Manager.User.GetItem().DonutBox > 0)
        {
            donutBoxToggle.isOn = false;
            donutBoxToggle.interactable = true;
        }
        if (Manager.User.GetItem().Oven <= 0)
        {
            ovenToggle.isOn = false;
            ovenToggle.interactable = false;
        }
        else if (Manager.User.GetItem().Oven > 0)
        {
            ovenToggle.isOn = false;
            ovenToggle.interactable = true;
        }
    }

    void EnterGame()
    {
        Debug.Log("게임 시작");
        if (Manager.User.CheckHeart())
        {

            if (rollerToggle.isOn)
            {
                Manager.User.UseItem(ItemType.Roller);
                Manager.Stage.SetUseItem((int)ItemType.Roller);
            }
            if (donutBoxToggle.isOn)
            {
                Manager.User.UseItem(ItemType.DonutBox);
                Manager.Stage.SetUseItem((int)ItemType.DonutBox);
            }
            if (ovenToggle.isOn)
            {
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


}
