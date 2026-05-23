using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerScript : MonoBehaviour {

    private int width, height;
    private Rect rectRight, rectLeft, rectInfo;
    private GUIStyle labelStyleUpperLeft, labelStyleUpperRight;
    private string currentTimeString, realTimeSinceStart, timeInfoString, debugLogString, timeString, timeSnapshotString;
    private bool displayTimerOnScreen, timeActive, countDown, pauseTime;

    [SerializeField] private float playTime, currentTime, startTime, stopTime, continueTime, fromStartTime, fromLoadTime;
    [SerializeField] private float temp, currentPauseGameTime, cumulativePauseGameTime, addToCountDownTime, addToCountDownTimeValue;
    [SerializeField] private float seconds, minutes, hours, addToPlayTime, addToPlayTimeValue, delayTime, delayTimeValue;
    [SerializeField] private float countDownValue, countDownTime, countDownAmount, countDownDelay;
    [SerializeField] private float defaultTimeScaleValue, newTimeScaleValue;
    //[SerializeField] private float fractions, days;

    void Awake () {
        width = Screen.width;
        height = Screen.height;
        rectRight = new Rect (10, 10, width - 20, height - 20);
        rectLeft = new Rect (10, 10, width - 20, height - 20);
        rectInfo = new Rect (20, 60, 400, 600);
        timeActive = false;
        countDown = false;
        pauseTime = false;
        countDownValue = 10f;
        addToCountDownTimeValue = 10f;
        addToPlayTimeValue = 10f;
        defaultTimeScaleValue = 1f;
        newTimeScaleValue = 5f;
        displayTimerOnScreen = false;
    }

	void OnGUI () {
        if ( displayTimerOnScreen ) {
            // Display the label in the window.
            labelStyleUpperRight = new GUIStyle ( GUI.skin.GetStyle ("label") );
            labelStyleUpperRight.alignment = TextAnchor.UpperRight;
            labelStyleUpperLeft = new GUIStyle ( GUI.skin.GetStyle ("label") );
            labelStyleUpperLeft.alignment = TextAnchor.UpperLeft;

            // Modify the size of the font based on the window.
            labelStyleUpperRight.fontSize = 6 * (width / 200);
            labelStyleUpperLeft.fontSize = 6 * (width / 200);

            ShowTime (Time.realtimeSinceStartup, "Real time since start  ", rectRight, labelStyleUpperRight);
            ShowTime (Time.time, "TimerClass ", rectLeft, labelStyleUpperLeft);

            timeInfoString =    "Play Time:" + "\t\t\t" + playTime.ToString ("f1") + "\n" +
                                "Count Down Time:" + "\t\t" + countDownTime.ToString ("f1") + "\n" +
                                "Start PlayTime:" + "\t\t" + timeSnapshotString + "\n" +
                                "From Start Time:" + "\t\t" + fromStartTime.ToString ("f0") + "\n" +
                                "Stop Time:" + "\t\t" + stopTime.ToString ("f0") + "\n" +
                                "From Load Time:" + "\t\t" + fromLoadTime.ToString ("f0") + "\n" +
                                "Current Pause Game Time:" + "\t" + currentPauseGameTime.ToString ("f0") + "\n" +
                                "Cumulative Pause Game Time:" + "\t" + cumulativePauseGameTime.ToString ("f0") + "\n" +
                                "Continue Game Time:" + "\t\t" + continueTime.ToString ("f0") + "\n" +
                                "==========================" + "\n" +
                                "1 - Start playTime." + "\n" +
                                "2 - Show time since level loaded." + "\n" +
                                "3 - Stop playTime." + "\n" +
                                "4 - Freeze Time.timeScale" + "\n" +
                                "5 - Continue playTime." + "\n" +
                                "6 - Reset playTime." + "\n" +
                                "7 - Count down time " + countDownValue + "\n" +
                                "8 - Add to count down time by " + addToCountDownTimeValue + "\n" +
                                "9 - Add to playTime by " + addToPlayTimeValue + "\n" +
                                "0 - Hold to set Time.timeScale to " + newTimeScaleValue;
            GUI.Label (rectInfo, timeInfoString);
        }
    }

    void Update () {
        Timer (Time.time);
        GetUserInput ();
    }

    void Timer (float t) {
        fromStartTime = Time.time;

        if (timeActive) {
            playTime = (t - continueTime) + addToPlayTime;
            /*if (playTime < 0f) {
                timeActive = false;
                playTime = 0f;
            }*/
            
            if (countDown) {
                countDownTime = countDownAmount + (countDownDelay - Time.time) + addToCountDownTime;
                if (countDownTime < 0f) {
                    countDown = false;
                    countDownTime = 0f;
                }
            }

            temp = cumulativePauseGameTime;

        }
        
        if (pauseTime) {
            currentPauseGameTime = Time.time - stopTime;
            cumulativePauseGameTime = Time.time - stopTime + temp;
		}
    }

    void GetUserInput () {

        if ( Input.GetKeyDown (KeyCode.Alpha1) ) {
            StartTimer (fromStartTime);
		}

        if ( Input.GetKeyDown (KeyCode.Alpha2) ) {
            ShowTimeSinceLastLvlLoad ();
        }

        if ( Input.GetKeyDown (KeyCode.Alpha3) ) {
            StopPlayTime (Time.time);
        }

        if ( Input.GetKeyDown (KeyCode.Alpha4) ) {
            SetNewTimeScale (0f);
        } else if ( Input.GetKeyUp (KeyCode.Alpha4) ) {
            SetNewTimeScale (defaultTimeScaleValue);
        }

        if ( Input.GetKeyDown (KeyCode.Alpha5) ) {
            ContinuePlayTime (Time.time);
		}

        if ( Input.GetKeyDown (KeyCode.Alpha6) ) {
            ResetPlayTime ();
		}

        if ( Input.GetKeyDown (KeyCode.Alpha7) ) {
            CountDown (countDownValue);
		}

        if ( Input.GetKeyDown (KeyCode.Alpha8) ) {
            AddToCountDownTimer (addToCountDownTimeValue);
		}

        if ( Input.GetKeyDown (KeyCode.Alpha9) ) {
            AddToPlayTime (addToPlayTimeValue);
		}

        if ( Input.GetKeyDown (KeyCode.KeypadPlus) || Input.GetKeyDown (KeyCode.Alpha0) ) {
            SetNewTimeScale (newTimeScaleValue);
        } else if ( Input.GetKeyUp (KeyCode.KeypadPlus) || Input.GetKeyUp (KeyCode.Alpha0) ) {
            SetNewTimeScale (defaultTimeScaleValue);
        }

        if ( Input.GetKeyUp (KeyCode.PageUp) ) {
            displayTimerOnScreen = true;
		} else if ( Input.GetKeyUp (KeyCode.PageDown) ) {
            displayTimerOnScreen = false;
		}

    }

    void DefineTime (float t) {
        seconds = (t % 60f);
        minutes = Mathf.Floor (t / 60f) % 60f;
        hours = Mathf.Floor (t / 3600f) % 24f;
        //days = Mathf.Floor (Time.time / 86400f) % 365f;
        //fractions = (Time.time * 10) % 10;
	}

    void ShowTime (float t, string debugLog, Rect r, GUIStyle s) {
        DefineTime (t);
        timeString = string.Format ("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        timeString = debugLog + timeString;
        GUI.Label (r, timeString, s);
    }

    string TakeTimeSnapshot (float h, float m, float s) {
        timeSnapshotString = string.Format ("{0:00}:{1:00}:{2:00}", h, m, s);
        return timeString;
	}

    void StartTimer (float t) {
        Debug.Log ("Start playTime.");
        timeActive = true;
        pauseTime = false;
        continueTime = t;
        playTime = 0f;
        stopTime = 0f;
        addToPlayTime = 0f;
        TakeTimeSnapshot (hours, minutes, seconds);
	}
        
    void ShowTimeSinceLastLvlLoad () {
        Debug.Log ("Show time since level loaded.");
        fromLoadTime = Time.timeSinceLevelLoad;
	}

    void StopPlayTime (float t) {
        Debug.Log ("Stop playTime.");
        stopTime = t;
        timeActive = false;
        countDown = false;
        pauseTime = true;
	}

    void ContinuePlayTime (float t) {
        Debug.Log ("Continue playTime.");
        continueTime = t - playTime + addToPlayTime;
        timeActive = true;
        pauseTime = false;
	}

    void ResetPlayTime () {
        Debug.Log ("Reset playTime.");
        timeActive = false;
        playTime = 0f;
        stopTime = 0f;
        addToPlayTime = 0f;
	}

    void CountDown (float cV) {
        Debug.Log ("Count down " + cV);
        timeActive = true;
        countDown = true;
        //countDownAmount = playTime;       // cunt down from current play time
        countDownAmount = cV;   // count down from given value in seconds
        countDownDelay = Time.time;
        addToCountDownTime = 0f;
	}

    void AddToCountDownTimer (float f) {
        Debug.Log ("Add " + f + " to count down timer.");
        addToCountDownTime += f;
	}

    void AddToPlayTime (float f) {
        Debug.Log ("Add " + f + " to playTime.");
        addToPlayTime += f;
	}

    void SetNewTimeScale (float f) {
        Time.timeScale = f;
        Debug.Log ("Time.timeScale set to " + Time.timeScale);
	}

} // end of class
