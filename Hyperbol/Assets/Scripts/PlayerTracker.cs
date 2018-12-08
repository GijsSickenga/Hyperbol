using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker
{
    public static Teams[] trackedPlayers = new Teams[4];

    public static Color GetColorFromPlayer(int playerIndex)
    {
        if (trackedPlayers[playerIndex] == Teams.Red)
            return Color.red;
        else if (trackedPlayers[playerIndex] == Teams.Blue)
            return Color.blue;

        Debug.LogError("Invalid team at requesting color");
        return Color.black;
    }
}
