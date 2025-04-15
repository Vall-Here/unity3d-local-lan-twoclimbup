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
    private bool isReturningToMainMenu = false;

    private void Awake()
    {
        resumeButton.onClick.AddListener(() =>
        {
            if (!isReturningToMainMenu) 
            {
                Debug.Log("Resume button clicked.");
                GameMultiplayerManager.Instance.TogglePauseGame();
            }
        });

        mainMenuButton.onClick.AddListener(() =>
        {
            Debug.Log("Main Menu button clicked.");
            ReturnToMainMenu();
        });
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
        Debug.Log("Showing pause menu.");
        optionPanel.gameObject.SetActive(true);
        UpdatePauseRequestText();
        resumeButton.Select();
    }

    private void Hide()
    {
        optionPanel.gameObject.SetActive(false);
        // UpdatePauseRequestText();
        
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


}
