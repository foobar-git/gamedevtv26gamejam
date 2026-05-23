using System.Collections;
using UnityEngine;

// Wraps an AudioSource with two play modes: fire-and-forget (PlayAudio) and
// wait-to-finish (PlayAudioWaitToFinishClip) which prevents the same clip from overlapping itself.
public class AudioScript : MonoBehaviour
{
    public AudioSource audioSource;
    [SerializeField] private bool clipFinished;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        clipFinished = true;
    }

    public void PlayAudio(AudioClip sound)
    {
        if (sound == null)
        {
            return;
        }
        audioSource.clip = sound;
        audioSource.Play();
    }

    public void PlayAudioWaitToFinishClip(AudioClip sound)
    {
        // clipFinished blocks re-entry so the clip can't interrupt itself if called repeatedly;
        // PlayAudio deliberately bypasses this for fire-and-forget sounds
        if (clipFinished && sound != null)
        {
            clipFinished = false;
            audioSource.clip = sound;
            audioSource.Play();
            StartCoroutine(WaitForClipToFinish());
        }
    }

    public IEnumerator WaitForClipToFinish()
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        clipFinished = true;
    }

} // end of class
