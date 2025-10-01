using UnityEditor;

public class CSVToSOImporter
{

    [MenuItem("PangTastic/Import CSV to SO")] // 메뉴 경로
    public static void StartImportIngredients()
    {
        StringImporter.StartImportString();
        RewardImporter.StartImportReward();
        IngredientImporter.StartImportIngredients();
        RecipeImporter.StartImportRecipes();
        CustomerImporter.StartImportCustomers();
        PuzzleBoardImporter.StartImportBoardMap();
        StageImporter.StartImportStages();

    }

}
