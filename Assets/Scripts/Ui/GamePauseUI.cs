using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : NetworkBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Transform optionPanel;
    [SerializeField] private TextMeshProUGUI pauseRequestText;

    [Header("Audio Settings")]
    [SerializeField] private AudioSettingsSO audioSettings;
    [SerializeField] private Slider volumeBacksoundSlider;
    [SerializeField] private Slider volumeSFXSlider;
    private bool isReturningToMainMenu = false;

    private void Start()
    {
        resumeButton.onClick.AddListener(() =>
        {
            if (!isReturningToMainMenu) 
            {
                // Debug.Log("Resume button clicked.");
                MainSceneAudioManager.Instance.PlayButtonClickSfx();
                GameMultiplayerManager.Instance.TogglePauseGame();
            }
        });

        mainMenuButton.onClick.AddListener(() =>
        {
            MainSceneAudioManager.Instance.PlayButtonClickSfx();
            // Debug.Log("Main Menu button clicked.");
            ReturnToMainMenu();
        });

        volumeBacksoundSlider.onValueChanged.AddListener(ChangeVolumeBacksound);
        volumeSFXSlider.onValueChanged.AddListener(ChangeVolumeSFX);


         if(MainSceneAudioManager.Instance != null) {
            MainSceneAudioManager.Instance.AudioSettingsChanged += UpdateAudioSettings;
        }

    }

    void OnDisable()
    {
        if(MainSceneAudioManager.Instance != null) {
            MainSceneAudioManager.Instance.AudioSettingsChanged -= UpdateAudioSettings;
        }
    }


    private void UpdateAudioSettings(float volumeBacksound, float volumeSFX)
    {
        volumeBacksoundSlider.value = volumeBacksound;
        volumeSFXSlider.value = volumeSFX;
    }


    public override void OnNetworkSpawn()
    {
        if (GameMultiplayerManager.Instance != null)
        {
            GameMultiplayerManager.Instance.OnMultiplayerGamePaused += GameMultiplayerManager_OnMultiplayerGamePaused;
            GameMultiplayerManager.Instance.OnMultiplayerGameUnpaused += GameMultiplayerManager_OnMultiplayerGameUnpaused;
            Hide();
        }
        else
        {
            Debug.LogError("GameMultiplayerManager instance is not available.");
        }
        UpdateAudioSettings(audioSettings.volumeBacksound, audioSettings.volumeSFX);
    }

    private void GameMultiplayerManager_OnMultiplayerGamePaused(object sender, System.EventArgs e)
    {
        Show();
        
        resumeButton.Select();

    }

    private void GameMultiplayerManager_OnMultiplayerGameUnpaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        MainSceneAudioManager.Instance.PlayOpenUISfx();
        Debug.Log("Showing pause menu.");
        optionPanel.gameObject.SetActive(true);
        UpdatePauseRequestText();
        resumeButton.Select();
    }

    private void Hide()
    {
        optionPanel.gameObject.SetActive(false);
        MainSceneAudioManager.Instance.PlayCloseUISfx();
    }
    private void UpdatePauseRequestText()
    {
        pauseRequestText.text = "Player Has requested to pause the game.";
    }

    private void ReturnToMainMenu()
    {
        Debug.Log("Attempting to return to main menu.");
        isReturningToMainMenu = true;
        GameMultiplayerManager.Instance.BackToMainMenuServerRpc();
    }



    // ==================== Audio Settings ====================

     private void ChangeVolumeBacksound(float value)
    {
        audioSettings.volumeBacksound = value;
        MainSceneAudioManager.Instance.UpdateBacksoundVolume(value);
        MainSceneAudioManager.Instance.SaveAudioSettings();
    }

    private void ChangeVolumeSFX(float value)
    {
        audioSettings.volumeSFX = value;
        MainSceneAudioManager.Instance.UpdateSFXVolume(value);
        MainSceneAudioManager.Instance.SaveAudioSettings();
    }


}
