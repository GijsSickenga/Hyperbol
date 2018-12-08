using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using XInputDotNetPure;

public class PlayerManager : MonoBehaviour
{
    public GameObject[] playerBlocks;
    public Color[] availableColors;

    private GamePadState[] currentStates;
    private GamePadState[] previousStates;

    void Start()
    {
        for (int i = 0; i < playerBlocks.Length; i++)
        {
            playerBlocks[i].GetComponent<PlayerBlock>().ChangeTeam(Teams.NotJoined);
        }

        currentStates = new GamePadState[4];
        previousStates = new GamePadState[4];
    }

    void Update()
    {
        for (int i = 0; i < currentStates.Length; i++)
        {
            currentStates[i] = GamePad.GetState((PlayerIndex)i);

            //Handle "Start" button input
            if (previousStates[i].Buttons.Start == ButtonState.Pressed && currentStates[i].Buttons.Start == ButtonState.Released)
            {
                if (!playerBlocks[i].activeSelf)
                {
                    playerBlocks[i].SetActive(true);
                    playerBlocks[i].GetComponent<PlayerBlock>().ChangeTeam(Teams.Red);
                }
            }

            //Handle "B" button input
            if (previousStates[i].Buttons.B == ButtonState.Pressed && currentStates[i].Buttons.B == ButtonState.Released)
            {
                if (playerBlocks[i].activeSelf)
                {
                    playerBlocks[i].GetComponent<PlayerBlock>().ChangeTeam(Teams.NotJoined);
                    playerBlocks[i].SetActive(false);
                }
            }

            //Handle "Y" button input
            if (previousStates[i].Buttons.Y == ButtonState.Pressed && currentStates[i].Buttons.Y == ButtonState.Released)
            {
                if (playerBlocks[i].activeSelf)
                {
                    ChangeTeam(playerBlocks[i]);
                }
            }

            previousStates[i] = currentStates[i];
        }
    }

    private void ChangeTeam(GameObject block)
    {
        Teams currentTeam = block.GetComponent<PlayerBlock>().team;

        Teams newTeam = currentTeam == Teams.Red ? Teams.Blue : Teams.Red;

        block.GetComponent<PlayerBlock>().ChangeTeam(newTeam);
    }
}

public enum Teams
{
    Red,
    Blue,
    NotJoined
}