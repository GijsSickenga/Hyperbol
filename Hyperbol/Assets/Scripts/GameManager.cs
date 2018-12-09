using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int matchDurationSeconds = 300;

    public TextMeshProUGUI timeText;
    public TextMeshProUGUI redScoreText, blueScoreText;

    public GameObject endScreen;
    public GameObject redWinText, blueWinText, drawText;

    public TextMeshProUGUI redScoreEndText, blueScoreEndText;

    private int redScore, blueScore;

    private float matchTimeLeft;

	void Start ()
    {
        matchTimeLeft = (float)matchDurationSeconds;

        redScore = 0;
        blueScore = 0;

        redScoreText.SetText(redScore.ToString());
        blueScoreText.SetText(blueScore.ToString());
    }
	
	void Update ()
    {
        matchTimeLeft -= Time.deltaTime;

        if (matchTimeLeft > 0f)
        {
            SetTimeToText();
        }
        else
        {
            matchTimeLeft = 0f;
            SetTimeToText();

            StartCoroutine(GameOver());
        }

        if (Input.GetKeyDown(KeyCode.B))
            GoalForBlue();
        else if (Input.GetKeyDown(KeyCode.R))
            GoalForRed();
	}

    private void SetTimeToText()
    {
        int minutes = Mathf.FloorToInt(matchTimeLeft / 60F);
        int seconds = Mathf.FloorToInt(matchTimeLeft - minutes * 60f);
        string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);

        timeText.SetText(niceTime);
    }

    public IEnumerator GameOver()
    {
        endScreen.SetActive(true);

        redScoreEndText.SetText(redScore.ToString());
        blueScoreEndText.SetText(blueScore.ToString());

        if (redScore > blueScore)
        {
            redWinText.SetActive(true);
        }
        else if (blueScore > redScore)
        {
            blueWinText.SetActive(true);
        }
        else
        {
            drawText.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(8f);

        SceneManager.LoadScene(0);
    }

    public void GoalForRed()
    {
        redScore++;

        redScoreText.SetText(redScore.ToString());
    }

    public void GoalForBlue()
    {
        blueScore++;

        blueScoreText.SetText(blueScore.ToString());
    }
}
