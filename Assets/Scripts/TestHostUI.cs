
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestHostUI : MonoBehaviour
{
    public Button _hostButton;
    public Button _joinButton;

    private void Start()
    {
        _hostButton.onClick.AddListener(OnHostButtonClick);
        _joinButton.onClick.AddListener(OnJoinButtonClick);
    }

    public void OnHostButtonClick()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void OnJoinButtonClick()
    {
        NetworkManager.Singleton.StartClient();
    }
}
