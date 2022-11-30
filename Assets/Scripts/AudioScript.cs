using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioScript : MonoBehaviour
{
    [SerializeField] List<AudioClip> _clips;
    [SerializeField] AudioSource _audioSource, _musicSource;
    [SerializeField] Slider _musicSlider, _audioSlider;
    float _audioVolume, _musicVolume;

    void Start() {
        _audioVolume = _audioSource.volume;
        _musicVolume = _musicSource.volume;
        _audioSlider.value = _audioVolume;
        _musicSlider.value = _musicVolume;
    }

    public void PlayAudio(AudioSource source, string clipName){
        AudioClip clip = _clips.Find(audio => audio.name.Equals(clipName));

        if (clip != null) { 
            if (source == null) { source = _audioSource; }

            source.volume = _audioVolume;
            source.PlayOneShot(clip);
        }
    }

    public void PlayMusic(string clipName){
        AudioClip clip = _clips.Find(audio => audio.name.Equals(clipName));

        if (clip != null) { 
            _musicSource.Stop();
            _musicSource.volume = _musicVolume;
            _musicSource.PlayOneShot(clip); 
        }
    }

    public void SetMusicPitch(float pitch) {
        _musicSource.pitch = pitch;
    }

    public void SetMusicVolume() {
        _musicVolume = Mathf.Min(_musicSlider.value, 1);
        _musicSource.volume = _musicVolume;
    }

    public void SetAudioVolume() {
        _audioVolume = Mathf.Min(_audioSlider.value, 1);
        _audioSource.volume = _audioVolume;
    }

    // public void StopMusic() { _musicSource.Stop(); }
}
