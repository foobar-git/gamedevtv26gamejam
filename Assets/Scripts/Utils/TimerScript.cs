using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerScript : MonoBehaviour {

    private int _width, _height;
    private Rect _rectRight, _rectLeft, _rectInfo;
    private GUIStyle _labelStyleUpperLeft, _labelStyleUpperRight;
    private string _currentTimeString, _realTimeSinceStart, _timeInfoString, _debugLogString, _timeString, _timeSnapshotString;
    private bool _isDisplayingTimerOnScreen, _isTimeActive, _isCountingDown, _isTimePaused;

    [SerializeField] private float playTime, currentTime, startTime, stopTime, continueTime, fromStartTime, fromLoadTime;
    [SerializeField] private float temp, currentPauseGameTime, cumulativePauseGameTime, addToCountDownTime, addToCountDownTimeValue;
    [SerializeField] private float seconds, minutes, hours, addToPlayTime, addToPlayTimeValue, delayTime, delayTimeValue;
    [SerializeField] private float countDownValue, countDownTime, countDownAmount, countDownDelay;
    [SerializeField] private float defaultTimeScaleValue, newTimeScaleValue;
    //[SerializeField] private float fractions, days;

    void Awake () {
        _width = Screen.width;
        _height = Screen.height;
        _rectRight = new Rect (10, 10, _width - 20, _height - 20);
        _rectLeft = new Rect (10, 10, _width - 20, _height - 20);
        _rectInfo = new Rect (20, 60, 400, 600);
        _isTimeActive = false;
        _isCountingDown = false;
        _isTimePaused = false;
        countDownValue = 10f;
        addToCountDownTimeValue = 10f;
        addToPlayTimeValue = 10f;
        defaultTimeScaleValue = 1f;
        newTimeScaleValue = 5f;
        _isDisplayingTimerOnScreen = false;
    }

	void OnGUI () {
        if ( _isDisplayingTimerOnScreen ) {
            // Display the label in the window.
            _labelStyleUpperRight = new GUIStyle ( GUI.skin.GetStyle ("label") );
            _labelStyleUpperRight.alignment = TextAnchor.UpperRight;
            _labelStyleUpperLeft = new GUIStyle ( GUI.skin.GetStyle ("label") );
            _labelStyleUpperLeft.alignment = TextAnchor.UpperLeft;

            // Modify the size of the font based on the window.
            _labelStyleUpperRight.fontSize = 6 * (_width / 200);
            _labelStyleUpperLeft.fontSize = 6 * (_width / 200);

            ShowTime (Time.realtimeSinceStartup, "Real time since start  ", _rectRight, _labelStyleUpperRight);
            ShowTime (Time.time, "TimerClass ", _rectLeft, _labelStyleUpperLeft);

            _timeInfoString =    "Play Time:" + "\t\t\t" + playTime.ToString ("f1") + "\n" +
                                "Count Down Time:" + "\t\t" + countDownTime.ToString ("f1") + "\n" +
                                "Start PlayTime:" + "\t\t" + _timeSnapshotString + "\n" +
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
            GUI.Label (_rectInfo, _timeInfoString);
        }
    }

    void Update () {
        Timer (Time.time);
        GetUserInput ();
    }

    void Timer (float t) {
        fromStartTime = Time.time;

        if (_isTimeActive) {
            playTime = (t - continueTime) + addToPlayTime;
            /*if (playTime < 0f) {
                _isTimeActive = false;
                playTime = 0f;
            }*/
            
            if (_isCountingDown) {
                countDownTime = countDownAmount + (countDownDelay - Time.time) + addToCountDownTime;
                if (countDownTime < 0f) {
                    _isCountingDown = false;
                    countDownTime = 0f;
                }
            }

            temp = cumulativePauseGameTime;

        }
        
        if (_isTimePaused) {
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
            _isDisplayingTimerOnScreen = true;
		} else if ( Input.GetKeyUp (KeyCode.PageDown) ) {
            _isDisplayingTimerOnScreen = false;
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
        _timeString = string.Format ("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        _timeString = debugLog + _timeString;
        GUI.Label (r, _timeString, s);
    }

    string TakeTimeSnapshot (float h, float m, float s) {
        _timeSnapshotString = string.Format ("{0:00}:{1:00}:{2:00}", h, m, s);
        return _timeString;
	}

    void StartTimer (float t) {
        Debug.Log ("Start playTime.");
        _isTimeActive = true;
        _isTimePaused = false;
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
        _isTimeActive = false;
        _isCountingDown = false;
        _isTimePaused = true;
	}

    void ContinuePlayTime (float t) {
        Debug.Log ("Continue playTime.");
        continueTime = t - playTime + addToPlayTime;
        _isTimeActive = true;
        _isTimePaused = false;
	}

    void ResetPlayTime () {
        Debug.Log ("Reset playTime.");
        _isTimeActive = false;
        playTime = 0f;
        stopTime = 0f;
        addToPlayTime = 0f;
	}

    void CountDown (float cV) {
        Debug.Log ("Count down " + cV);
        _isTimeActive = true;
        _isCountingDown = true;
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
