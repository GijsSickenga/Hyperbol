using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int matchDurationSeconds = 300;

    public TextMeshProUGUI timeText;

    private float matchTimeLeft;

	void Start ()
    {
        matchTimeLeft = (float)matchDurationSeconds;
	}
	
	void Update ()
    {
        matchTimeLeft -= Time.deltaTime;

        SetTimeToText();
	}

    private void SetTimeToText()
    {
        int minutes = Mathf.FloorToInt(matchTimeLeft / 60F);
        int seconds = Mathf.FloorToInt(matchTimeLeft - minutes * 60);
        string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);

        timeText.SetText(niceTime);
    }
}
