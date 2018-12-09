using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class Vibration : MonoBehaviour
{
    private static Vibration instance;

    void Awake()
    {
        instance = this;
    }

    private void OnDisable()
    {
        for (int i = 0; i < 4; i++)
        {
            GamePad.SetVibration((PlayerIndex)(i), 0, 0);
        }
    }

    public static void VibrateForSeconds(float seconds, float amount, int playerIndex)
    {
        instance.StartCoroutine(Vibrate(seconds, amount, playerIndex));
    }

    public static void VibrateForSeconds(float seconds, float amount, PlayerIndex playerIndex)
    {
        instance.StartCoroutine(Vibrate(seconds, amount, (int)playerIndex));
    }

    private static IEnumerator Vibrate(float seconds, float amount, int playerIndex)
    {
        GamePad.SetVibration((PlayerIndex)(playerIndex), amount, amount);
        yield return new WaitForSeconds(seconds);
        GamePad.SetVibration((PlayerIndex)(playerIndex), 0, 0);
    }
}
