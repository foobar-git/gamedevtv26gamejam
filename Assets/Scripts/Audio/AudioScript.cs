using UnityEngine;

// Audio channel each sound routes through — Sfx and Player play simultaneously.
public enum AudioChannel { Sfx, Player }

// Single global audio manager — all game sounds route through this singleton.
// Three independent channels, each with its own AudioSource on the GameManager:
//   Sfx    — gameplay sounds (stomp, coins, jump, bricks, etc.)
//   Player — player state sounds (die, save point); overlaps freely with Sfx
//   Music  — looping background track; use PlayMusic/StopMusic, never interrupted by SFX
// TODO: [Phase X] - Extend by adding more AudioChannel values and AudioSources (e.g. Ambient
// for environment loops). Add a priority parameter to PlayAudio / PlayAudioWaitToFinishClip
// if higher-priority sounds should interrupt lower-priority ones on the same channel.
public class AudioScript : MonoBehaviour
{
    public static AudioScript Instance { get; private set; }

    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioSource musicAudioSource;

    void Awake()
    {
        Instance = this;
    }

    public void PlayAudio(AudioClip sound, AudioChannel channel = AudioChannel.Sfx)
    {
        if (sound == null)
        {
            return;
        }
        AudioSource source = GetSource(channel);
        source.clip = sound;
        source.Play();
    }

    public void PlayAudioWaitToFinishClip(AudioClip sound, AudioChannel channel = AudioChannel.Sfx)
    {
        // skip if this exact clip is already playing on this channel — prevents re-triggering on rapid repeated calls
        AudioSource source = GetSource(channel);
        if (sound == null || (source.clip == sound && source.isPlaying))
        {
            return;
        }
        source.clip = sound;
        source.Play();
    }

    public void PlayMusic(AudioClip music)
    {
        if (music == null)
        {
            return;
        }
        musicAudioSource.clip = music;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    public void StopMusic()
    {
        musicAudioSource.Stop();
    }

    AudioSource GetSource(AudioChannel channel) => channel == AudioChannel.Player ? playerAudioSource : sfxAudioSource;

} // end of class
