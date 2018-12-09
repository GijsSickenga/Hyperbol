using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerBlock : MonoBehaviour
{
    public Teams team;
    public PlayerManager playerManager;
    public int playerIndex;

    public void ChangeTeam(Teams newTeam)
    {
        team = newTeam;

        PlayerTracker.trackedPlayers[playerIndex] = team;

        if (team != Teams.NotJoined)
        {
            Color theColor = playerManager.availableColors[(int)team];
            this.GetComponent<Image>().color = theColor;
        }
    }
}