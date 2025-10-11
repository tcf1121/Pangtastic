using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    int currentScenario;
    int currentStage;
    [SerializeField] DialoguePlayer _dialoguePlayer;

    void Awake()
    {
        currentScenario = Manager.User.GetScenario() + 1;
        currentStage = Manager.Stage.CurrentStageIndex + 1;
    }


    public void PlayScenario(MissionPlace missionPlace = MissionPlace.Donut, float percent = 0f)
    {
        if (currentScenario == 1)
        {
            if (currentStage == 1)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 2)
        {
            if (currentStage == 2)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 3)
        {
            if (currentStage >= 2)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 4)
        {
            if (missionPlace == MissionPlace.Donut && percent > 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 5)
        {
            if (currentStage >= 3)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 6)
        {
            if (missionPlace == MissionPlace.Donut && percent >= 0.5f)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 7)
        {
            if (missionPlace == MissionPlace.MiniCafe && percent == 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 8)
        {
            if (missionPlace == MissionPlace.MiniCafe && percent >= 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 9)
        {
            if (missionPlace == MissionPlace.MiniCafe && percent >= 0.5f)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 10)
        {
            if (missionPlace == MissionPlace.Cafe && percent == 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 11)
        {
            if (missionPlace == MissionPlace.Cafe && percent >= 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 12)
        {
            if (missionPlace == MissionPlace.Cafe && percent >= 0.5f)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 13)
        {
            if (missionPlace == MissionPlace.IceCream && percent == 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 14)
        {
            if (missionPlace == MissionPlace.IceCream && percent >= 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 15)
        {
            if (missionPlace == MissionPlace.IceCream && percent >= 0.5f)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 16)
        {
            if (missionPlace == MissionPlace.Bakery && percent == 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 17)
        {
            if (missionPlace == MissionPlace.Bakery && percent >= 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 18)
        {
            if (missionPlace == MissionPlace.Bakery && percent >= 0.5f)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 19)
        {
            if (missionPlace == MissionPlace.Pizzeria && percent == 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 20)
        {
            if (missionPlace == MissionPlace.Pizzeria && percent >= 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 21)
        {
            if (missionPlace == MissionPlace.Pizzeria && percent >= 0.5f)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 22)
        {
            if (missionPlace == MissionPlace.Bar && percent == 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 23)
        {
            if (missionPlace == MissionPlace.Bar && percent >= 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 24)
        {
            if (missionPlace == MissionPlace.Bar && percent >= 0.5f)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 25)
        {
            if (missionPlace == MissionPlace.Greengrocery && percent == 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 26)
        {
            if (missionPlace == MissionPlace.Greengrocery && percent >= 0)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 27)
        {
            if (missionPlace == MissionPlace.Greengrocery && percent >= 0.5f)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        else if (currentScenario == 28)
        {
            if (missionPlace == MissionPlace.Greengrocery && percent == 1f)
            {
                _dialoguePlayer.ShowById(currentScenario);
                Manager.User.ClearScenario();
            }
        }
        currentScenario = Manager.User.GetScenario() + 1;
        currentStage = Manager.Stage.CurrentStageIndex + 1;

    }
}
