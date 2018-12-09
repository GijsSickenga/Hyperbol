using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject loadingImage;

    public void StartGame()
    {
        int amountOfRedPlayers = 0;
        int amountOfBluePlayers = 0;

        for (int i = 0; i < PlayerTracker.trackedPlayers.Length; i++)
        {
            Teams chosenTeam = PlayerTracker.trackedPlayers[i];

            if(chosenTeam != Teams.NotJoined)
            {
                if (chosenTeam == Teams.Red)
                    amountOfRedPlayers++;
                else if (chosenTeam == Teams.Blue)
                    amountOfBluePlayers++;
            }
        }

        if (amountOfRedPlayers > 0 && amountOfRedPlayers < 3 && amountOfBluePlayers > 0 && amountOfBluePlayers < 3)
        {
            Debug.Log("Start game");
            loadingImage.SetActive(true);
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.Log("Not enough players in teams");
        }
    }
	
}
