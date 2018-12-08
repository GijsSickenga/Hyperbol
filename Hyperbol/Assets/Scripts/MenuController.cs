using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{

    public void StartGame()
    {
        bool teamRedHasPlayers = false;
        bool teamBlueHasPlayers = false;

        for (int i = 0; i < PlayerTracker.trackedPlayers.Length; i++)
        {
            Teams chosenTeam = PlayerTracker.trackedPlayers[i];

            if(chosenTeam != Teams.NotJoined)
            {
                if(chosenTeam == Teams.Red)
                    teamRedHasPlayers = true;
                else if (chosenTeam == Teams.Blue)
                    teamBlueHasPlayers = true;
            }
        }

        if(teamRedHasPlayers && teamBlueHasPlayers)
        {
            Debug.Log("Start game boi");
        }
        else
        {
            Debug.Log("Not enough players in teams");
        }
    }
	
}
