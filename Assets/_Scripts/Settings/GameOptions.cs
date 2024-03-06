using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOptions : Singleton<GameOptions>
{
    public GameObject uiElement;
    #region AudioSettings
    [Header("Audio")]
    public static AudioSettings defaultSettings = new AudioSettings
    {
        masterVol = 1f,
        sfxVol = 1f,
        musicVol = 1f,
        dialogueVol = 1f,
    };
    [SerializeField] private static AudioSettings _audioSettings = defaultSettings;
    
    [Serializable]
    public struct AudioSettings
    {
        public float masterVol;
        public float sfxVol;
        public float musicVol;
        public float dialogueVol;
    }
    
    public static event Action<AudioSettings> OnAudioSettingsUpdated;

    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider musicSlider;
    public Slider dialogueSlider;

    #endregion

    #region Gameplay Settings
    
    [Header("Gameplay")]
    [SerializeField] private static GameSettings _gameSettings;
    public static event Action<GameSettings> OnGameSettingsUpdated;
    public enum ControlTypes { Drag, Click}
    [Serializable]
    public struct GameSettings
    {
        public ControlTypes controlType;
    }
    public TMP_Dropdown controlTypeDropdown;

    #endregion
    
    #region  Methods
    
    public static AudioSettings GetAudioSettings()
    {
        return _audioSettings;
    }
    
    public static GameSettings GetGameSettings()
    {
        return _gameSettings;
    }

    protected void Start()
    {
        
        //Get saved values if present in PlayerPrefs
        if (PlayerPrefs.HasKey("Settings_AudioMaster"))
            _audioSettings.masterVol = PlayerPrefs.GetFloat("Settings_AudioMaster");
        if(PlayerPrefs.HasKey("Settings_AudioSfx"))
            _audioSettings.sfxVol = PlayerPrefs.GetFloat("Settings_AudioSfx");
        if(PlayerPrefs.HasKey("Settings_AudioMusic"))
            _audioSettings.musicVol = PlayerPrefs.GetFloat("Settings_AudioMusic");
        if(PlayerPrefs.HasKey("Settings_AudioDialogueSfx"))
            _audioSettings.dialogueVol = PlayerPrefs.GetFloat("Settings_AudioDialogueSfx");
        if (PlayerPrefs.HasKey("Settings_ControlType"))
            _gameSettings.controlType = (ControlTypes)PlayerPrefs.GetInt("Settings_ControlType");
        
        //Update UI elements to match settings.
        masterSlider.value = _audioSettings.masterVol;
        sfxSlider.value = _audioSettings.sfxVol;
        musicSlider.value = _audioSettings.musicVol;
        dialogueSlider.value = _audioSettings.dialogueVol;
        controlTypeDropdown.value = (int)_gameSettings.controlType;

        GameSettingsUpdated();
        AudioSettingsUpdated();
        
        uiElement.SetActive(true);
        gameObject.SetActive(false);

    }

    public void SetMasterVolume(float vol)
    {
        _audioSettings.masterVol = vol;
        AudioSettingsUpdated();
    }
    
    public void SetSfxVolume(float vol)
    {
        _audioSettings.sfxVol = vol;
        AudioSettingsUpdated();
    }
    
    public void SetMusicVolume(float vol)
    {
        _audioSettings.musicVol = vol;
        AudioSettingsUpdated();
    }
    
    public void SetDialogueVolume(float vol)
    {
        _audioSettings.dialogueVol = vol;
        AudioSettingsUpdated();
    }

    public void SetControlType(int controlType)
    {
        _gameSettings.controlType = (ControlTypes)controlType;
        GameSettingsUpdated();
    }

    void GameSettingsUpdated()
    {
        PlayerPrefs.SetInt($"Settings_ControlType", (int)_gameSettings.controlType);
        OnGameSettingsUpdated?.Invoke(_gameSettings);
    }
    
    void AudioSettingsUpdated()
    {
        PlayerPrefs.SetFloat($"Settings_AudioMaster", _audioSettings.masterVol);
        PlayerPrefs.SetFloat($"Settings_AudioSfx", _audioSettings.sfxVol);
        PlayerPrefs.SetFloat($"Settings_AudioMusic", _audioSettings.musicVol);
        PlayerPrefs.SetFloat($"Settings_AudioDialogueSfx", _audioSettings.dialogueVol);

        OnAudioSettingsUpdated?.Invoke(_audioSettings);
    }

    public void ResetPlayerProgress()
    {
        PlayerPrefs.DeleteAll();
    }
    

    #endregion


}
