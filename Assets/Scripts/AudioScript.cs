using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    [SerializeField] List<AudioClip> _clips;
    [SerializeField] AudioSource _audioSource, _musicSource;
    float _audioVolume, _musicVolume;

    void Start() {
        // if (_musicSource.clip.name.Equals("loop_1")) { _musicSource.volume = .3f; }
    }

    public void PlayAudio(AudioSource source, string clipName){
        AudioClip clip = _clips.Find(audio => audio.name.Equals(clipName));

        if (clip != null) { 
            if (source == null) { _audioSource.PlayOneShot(clip); }
            else { source.PlayOneShot(clip); }
        }
    }

    public void PlayMusic(string clipName){
        AudioClip clip = _clips.Find(audio => audio.name.Equals(clipName));

        if (clip != null) { 
            _musicSource.Stop();
            _musicSource.PlayOneShot(clip); 
        }
    }

    public void SetMusicPitch(float pitch) {
        _musicSource.pitch = pitch;
    }

    // public void StopMusic() { _musicSource.Stop(); }
}
