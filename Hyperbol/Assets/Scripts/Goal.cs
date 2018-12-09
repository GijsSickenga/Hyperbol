using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Teams team;
    public GameManager gameManager;

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(Tags.HYPERBOL))
        {
            if (team == Teams.Red)
            {
                gameManager.GoalForBlue();
            }
            else if (team == Teams.Blue)
            {
                gameManager.GoalForRed();
            }

            other.gameObject.GetComponent<Hyperbol>().ResetBal();
        }
    }

}
