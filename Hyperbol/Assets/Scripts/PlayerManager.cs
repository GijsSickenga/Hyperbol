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

            bool joinGamePad = (previousStates[i].Buttons.Start == ButtonState.Pressed && currentStates[i].Buttons.Start == ButtonState.Released);
            bool leaveGamePad = (previousStates[i].Buttons.B == ButtonState.Pressed && currentStates[i].Buttons.B == ButtonState.Released);
            bool changeTeamGamePad = (previousStates[i].Buttons.Y == ButtonState.Pressed && currentStates[i].Buttons.Y == ButtonState.Released);
            int playerId = i;

            bool joinKeyboard = false;
            bool leaveKeyboard = false;
            bool changeTeamKeyboard = false;

#if UNITY_EDITOR
            joinKeyboard = Input.GetKeyDown(KeyCode.Space);
            leaveKeyboard = Input.GetKeyDown(KeyCode.Escape);
            changeTeamKeyboard = Input.GetKeyDown(KeyCode.T);

            if (joinKeyboard || leaveKeyboard || changeTeamKeyboard)
            {
                playerId = 0;
            }
#endif

            // Handle "Start" button input.
            if (joinGamePad || joinKeyboard)
            {
                if (!playerBlocks[playerId].activeSelf)
                {
                    playerBlocks[playerId].SetActive(true);
                    playerBlocks[playerId].GetComponent<PlayerBlock>().ChangeTeam(Teams.Red);
                }
            }

            // Handle "B" button input.
            if (leaveGamePad || leaveKeyboard)
            {
                if (playerBlocks[playerId].activeSelf)
                {
                    playerBlocks[playerId].GetComponent<PlayerBlock>().ChangeTeam(Teams.NotJoined);
                    playerBlocks[playerId].SetActive(false);
                }
            }

            // Handle "Y" button input.
            if (changeTeamGamePad || changeTeamKeyboard)
            {
                if (playerBlocks[playerId].activeSelf)
                {
                    ChangeTeam(playerBlocks[playerId]);
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