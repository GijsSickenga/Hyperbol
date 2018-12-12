using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Teams team;
    public GameManager gameManager;
    public GameObject blueGoalPS, redGoalPS;

    public void Score()
    {
        if (team == Teams.Red)
        {
            Instantiate(redGoalPS);
            gameManager.GoalForBlue();
        }
        else if (team == Teams.Blue)
        {
            Instantiate(blueGoalPS);
            gameManager.GoalForRed();
        }
    }
}
