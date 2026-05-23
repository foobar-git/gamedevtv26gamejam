using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour {
    
    public AudioSource audioSource;
    [SerializeField] private bool clipFinished;

	void Awake () {
		audioSource = GetComponent<AudioSource> ();
        clipFinished = true;
	}

	public void PlayAudio (AudioClip sound) {
        if (sound == null) return;
        audioSource.clip = sound;
        audioSource.Play ();
	}

    public void PlayAudioWaitToFinishClip (AudioClip sound) {
        if (clipFinished && sound != null) {
            clipFinished = false;
            audioSource.clip = sound;
            audioSource.Play ();
            StartCoroutine ( WaitForClipToFinish () );
        }
	}

    public IEnumerator WaitForClipToFinish () {
        yield return new WaitForSeconds ( audioSource.clip.length );
        clipFinished = true;
	}

} // end of class
