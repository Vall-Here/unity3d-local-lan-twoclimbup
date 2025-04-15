using System;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private void Start()
    {
        LobbyLanManager.Instance.OnTryingToJoinGame += LobbyLanManager_OnTryingToJoinGame;
        LobbyLanManager.Instance.OnFailedToJoinGame += LobbyLanManager_OnFailedToJoinGame;
        Hide();
    }

    private void LobbyLanManager_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Hide();
    }

    private void LobbyLanManager_OnTryingToJoinGame(object sender, EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        LobbyLanManager.Instance.OnTryingToJoinGame -= LobbyLanManager_OnTryingToJoinGame;
        LobbyLanManager.Instance.OnFailedToJoinGame -= LobbyLanManager_OnFailedToJoinGame;
    }
}
