using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public struct Sound
{
    public string name;
    public AudioClip[] clips;
    public float volumePercent;
}

public class AudioManager : Singleton<AudioManager>
{
    public int maxSounds;
    public bool masterMute = false;
    public float masterVolume;
    public AudioMixer audioMixer;

    [Header("Music")]
    private bool isPaused = false;
    public bool musicMute;
    public float musicVolume;
    public AudioMixerGroup musicMix;
    private AudioSource musicSource;
    public bool playOnStart;
    private Sound currentMusic;
    public string startingMusic;
    public List<Sound> music = new List<Sound>();
    public static event Action<string> OnTrackChanged;

    [Header("Sound Effects")]
    public bool sfxMute = false;
    public float sfxVolume;
    public AudioMixerGroup sfxMix;
    public List<Sound> sounds = new List<Sound>();
    private List<AudioSource> activeSources = new List<AudioSource>();

    [Header("Dialogue")]
    public bool dialogueMute;
    public float dialogueVolume;
    public AudioMixerGroup dialogueMix;
    private AudioSource dialogueSource;
    public List<Sound> dialogue = new List<Sound>();

    [Header("Ambiance")]
    public bool ambianceMute;
    public float ambianceVolume;
    public AudioMixerGroup ambianceMix;
    private AudioSource ambianceSource;
    public List<Sound> ambiance = new List<Sound>();


    private void OnEnable()
    {
        GameOptions.OnAudioSettingsUpdated += AudioSettingsUpdated;
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    
    private void OnDisable()
    {
        GameOptions.OnAudioSettingsUpdated -= AudioSettingsUpdated;
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    void OnSceneLoad(Scene scene, LoadSceneMode loadMode)
    {
        if(musicSource)
        {
            //Stop music on title screen
            if (scene.buildIndex == 0)
            {
                musicSource.Stop();
            }
            //If no music is playing, choose a random track.
            else if(!musicSource.isPlaying)
            {
                PlayMusic(PickRandomMusic());
            }
        }


        if (!ambianceSource)
        {
            ambianceSource = gameObject.AddComponent<AudioSource>();
            ambianceSource.outputAudioMixerGroup = ambianceMix;
        }

        PlayAmbiance(ambiance[SceneManager.GetActiveScene().buildIndex], 1);

    }

    private void Start()
    {
        AudioSettingsUpdated(GameOptions.GetAudioSettings());
        if (!musicSource)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.outputAudioMixerGroup = musicMix;
        }

        
    }


    public void PlaySound(string soundToPlay)
    {
        PlaySound(soundToPlay, 1);
    }
    
    public void PlaySound(string soundToPlay, float pitch)
    {
        if (soundToPlay.StartsWith("PieceSwap")) soundToPlay = soundToPlay + Random.Range(1, 5);

        Sound sound = sounds.Find(s => s.name == soundToPlay);

        if (sound.clips == null)
        {
            Debug.Log("Sound not found: " + soundToPlay);
            return;
        }

        PlaySound(sound, pitch);
    }
    
    public void PlaySound(Sound soundToPlay, float pitch)
    {
        if (masterMute || sfxMute)
            return;
        
        if (activeSources.Count >= maxSounds)
            return;

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxMix;

        //Get a random clip to play
        source.clip = soundToPlay.clips[Random.Range(0, soundToPlay.clips.Length)];
        source.volume = soundToPlay.volumePercent * sfxVolume;
        source.pitch = pitch;
        source.Play();

        activeSources.Add(source);
    }

    public void PlayMusic(string musicToPlay)
    {
        Sound sound = music.Find(s => s.name == musicToPlay);

        if (sound.clips == null)
        {
            Debug.Log("Music not found: " + musicToPlay);
            return;
        }

        PlayMusic(sound);
    }
    
    public void PlayMusic(Sound musicToPlay)
    {
        if (masterMute || sfxMute)
            return;
        
        if (activeSources.Count >= 5)
            return;

        if (!musicSource)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.outputAudioMixerGroup = musicMix;
        }
        
        currentMusic = musicToPlay;
        musicSource.clip = currentMusic.clips[Random.Range(0, currentMusic.clips.Length)];
        musicSource.volume = currentMusic.volumePercent * musicVolume;
        
        // musicSource.loop = true;
        musicSource.Play();

        OnTrackChanged?.Invoke(currentMusic.name);
    }

    public void PauseMusic()
    {
        isPaused = true;

        if (musicSource)
            musicSource.Pause();
    }

    public void ResumeMusic()
    {
        isPaused= false;

        if (musicSource)
            musicSource.UnPause();
    }

    public void PlayDialogue(string dialogueToPlay)
    {
        Sound sound = dialogue.Find(s => s.name == dialogueToPlay);

        if (sound.clips == null)
        {
            Debug.Log("Dialogue not found: " + dialogueToPlay);
            return;
        }

        PlayDialogue(sound);
    }

    public void PlayDialogue(Sound dialogueToPlay)
    {
        if (masterMute || dialogueMute)
            return;

        if (!dialogueSource)
        {
            dialogueSource = gameObject.AddComponent<AudioSource>();
            dialogueSource.outputAudioMixerGroup = dialogueMix;
        }

        dialogueSource.clip = dialogueToPlay.clips[Random.Range(0, dialogueToPlay.clips.Length)];
        dialogueSource.volume = dialogueToPlay.volumePercent * dialogueVolume;
        dialogueSource.Play();

        activeSources.Add(dialogueSource);
    }

    //Audio Clean-up
    public void AudioUpdate()
    {
        if(musicSource)
        {
            if (!musicSource.isPlaying && !isPaused)
            {
                // PlayMusic(PickRandomMusic());
                NextTrack();
            }
        }

        // Clean up finished sounds
        for (int i = activeSources.Count - 1; i >= 0; i--)
        {
            if (activeSources[i] == null)
            {
                activeSources.RemoveAt(i);
            }
            else if (!activeSources[i].isPlaying)
            {
                Destroy(activeSources[i]);
                activeSources.RemoveAt(i);
            }
        }
    }

    public void PlayAmbiance(string ambianceToPlay)
    {
        PlaySound(ambianceToPlay, 1);
    }

    public void PlayAmbiance(string ambianceToPlay, float pitch)
    {
        Sound sound = sounds.Find(s => s.name == ambianceToPlay);

        if (sound.clips == null)
        {
            Debug.Log("Sound not found: " + ambianceToPlay);
            return;
        }

        PlaySound(sound, pitch);
    }

    public void PlayAmbiance(Sound ambianceToPlay, float pitch)
    {
        if (masterMute || ambianceMute)
            return;

        //Get a random clip to play
        ambianceSource.clip = ambianceToPlay.clips[Random.Range(0, ambianceToPlay.clips.Length)];
        ambianceSource.volume = ambianceToPlay.volumePercent * ambianceVolume;
        ambianceSource.pitch = pitch;
        ambianceSource.loop = true;
        ambianceSource.Play();
    }

    public string NextTrack()
    {
        int currentIndex = music.FindIndex(s => s.name == currentMusic.name);
        PlayMusic(music[(currentIndex + 1) % music.Count]);
        return currentMusic.name;
    }

    public string PreviousTrack()
    {
        int currentIndex = music.FindIndex(s => s.name == currentMusic.name);
        currentIndex--;
        if (currentIndex < 0) 
            currentIndex = music.Count - 1;

        PlayMusic(music[currentIndex]);
        return currentMusic.name;
    }

    public void ChangeMasterVol(float sliderValue)
    {
        masterVolume = sliderValue;
        audioMixer.SetFloat("MasterVol", Mathf.Log10(masterVolume) * 20);
    }
    
    public void ChangeSfxVol(float sliderValue)
    {
        sfxVolume = sliderValue; 
        audioMixer.SetFloat("SfxVol", Mathf.Log10(sfxVolume) * 20);
    }
    
    public void ChangeMusicVol(float sliderValue)
    {
        musicVolume = sliderValue; 
        audioMixer.SetFloat("MusicVol", Mathf.Log10(musicVolume) * 20);
    }

    public void ChangeDialogueVol(float sliderValue)
    {
        dialogueVolume = sliderValue;
        audioMixer.SetFloat("DialogueVol", Mathf.Log10(dialogueVolume) * 20);
    }
    
    void AudioSettingsUpdated(GameOptions.AudioSettings settings)
    {
        ChangeMasterVol(settings.masterVol);
        ChangeSfxVol(settings.sfxVol);
        ChangeMusicVol(settings.musicVol);
        ChangeDialogueVol(settings.dialogueVol);
    }

    private string PickRandomMusic()
    {
        return music[Random.Range(0, music.Count)].name;
    }
}
