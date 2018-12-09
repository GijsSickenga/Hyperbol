using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject playerPrefab;

    public Transform redSpawnSingle;
    public Transform blueSpawnSingle;

    public Transform[] redSpawnDuo;
    public Transform[] blueSpawnDuo;

    void Start()
    {
        List<int> redPlayerIDs = new List<int>();
        List<int> bluePlayerIDs = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            if (PlayerTracker.trackedPlayers[i] != Teams.NotJoined)
            {
                // Joined player, instantiate
                if (PlayerTracker.trackedPlayers[i] == Teams.Red)
                {
                    redPlayerIDs.Add(i);
                }
                else
                {
                    bluePlayerIDs.Add(i);
                }
            }
        }

        if (redPlayerIDs.Count == 1)
        {
            SpawnPlayer(redPlayerIDs[0], Teams.Red, redSpawnSingle);
        }
        else if (redPlayerIDs.Count == 2)
        {
            SpawnPlayer(redPlayerIDs[0], Teams.Red, redSpawnDuo[0]);

            SpawnPlayer(redPlayerIDs[1], Teams.Red, redSpawnDuo[1]);
        }

        if (bluePlayerIDs.Count == 1)
        {
            SpawnPlayer(bluePlayerIDs[0], Teams.Blue, blueSpawnSingle);
        }
        else if (bluePlayerIDs.Count == 2)
        {
            SpawnPlayer(bluePlayerIDs[0], Teams.Blue, blueSpawnDuo[0]);

            SpawnPlayer(bluePlayerIDs[1], Teams.Blue, blueSpawnDuo[1]);
        }
    }

    private void SpawnPlayer(int playerIndex, Teams team, Transform spawnPoint)
    {
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<VehicleControls>().Initialize(playerIndex, team);

        Vibration.VibrateForSeconds(0.5f, 0.5f, playerIndex);
    }
}
