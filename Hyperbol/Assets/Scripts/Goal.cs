using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Teams team;
    public GameManager gameManager;

    public void Score()
    {
        if (team == Teams.Red)
        {
            gameManager.GoalForBlue();
        }
        else if (team == Teams.Blue)
        {
            gameManager.GoalForRed();
        }
    }
}
