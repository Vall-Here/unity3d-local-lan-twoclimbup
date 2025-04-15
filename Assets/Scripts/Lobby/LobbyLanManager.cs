using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using QFSW.QC;
using System;
using System.Collections;

public class LobbyLanManager : NetworkBehaviour
{
    private static LobbyLanManager _instance;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;

    private const int MaxClients = 2;  // Set a configurable client limit

    [SerializeField] string ipAddress;

    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button cancelClientButton;

    [Space(10)]
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private TextMeshProUGUI ipAddressText;
    [SerializeField] private TextMeshProUGUI connectedClientText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Space(10)]
    [SerializeField] private Transform connectedPanelTransform;
    [SerializeField] private UnityTransport transport;
    [SerializeField] private GameObject loadingSpinner;

    private void Awake()
    {
        _instance = this;
    }
    public static LobbyLanManager Instance => _instance;

    void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        joinButton.onClick.AddListener(StartClient);
        stopButton.onClick.AddListener(StopHost);
        cancelClientButton.onClick.AddListener(StopClient);
        startButton.onClick.AddListener(StartGame);

        stopButton.gameObject.SetActive(false);
        cancelClientButton.gameObject.SetActive(false);
        connectedPanelTransform.gameObject.SetActive(false);
        startButton.interactable = false;
        loadingSpinner.SetActive(false);
    }

    [Command]
    public void StartHost()
    {
        LobbyAudioManager.Instance.PlayButtonClickSfx();
        
        NetworkManager.Singleton.StartHost();
        GetLocalIPAddress();

        stopButton.gameObject.SetActive(true);
        hostButton.interactable = false;
        joinButton.interactable = false;
        ipInput.interactable = false;

        ShowStatus("Hosting...");
        loadingSpinner.SetActive(true);
         StartCoroutine(HideConnectionAfterTimeout(5f));
    }

    public void StopHost()
    {
        LobbyAudioManager.Instance.PlayButtonClickSfx();
        NetworkManager.Singleton.Shutdown();
        ClearAllUI();
    }

    public void StopClient()
    {
        LobbyAudioManager.Instance.PlayButtonClickSfx();
        NetworkManager.Singleton.Shutdown();
        ClearAllUI();
    }

    [Command]
    public void StartClient()
    {
        LobbyAudioManager.Instance.PlayButtonClickSfx();
        ipAddress = ipInput.text;
        startButton.interactable = false;
        SetIpAddress();
        

        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.StartClient();

        hostButton.interactable = false;
        cancelClientButton.gameObject.SetActive(true);

        ShowStatus("Connecting...");
        loadingSpinner.SetActive(true);
         StartCoroutine(HideConnectionAfterTimeout(5f));
    }

    void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        LobbyAudioManager.Instance.PlaySuccessUISfx();
        if (NetworkManager.Singleton.IsHost)
        {
            connectedPanelTransform.gameObject.SetActive(true);
            connectedClientText.text = "Player: " + NetworkManager.Singleton.ConnectedClients.Count;

            if (NetworkManager.Singleton.ConnectedClients.Count == MaxClients)
            {
            }
                startButton.interactable = true;

            if (NetworkManager.Singleton.ConnectedClients.Count > MaxClients)
            {
                NetworkManager.Singleton.DisconnectClient(clientId);
                ShowError("Too many players connected!");
            }
        }
        StopAllCoroutines();
        loadingSpinner.SetActive(false);
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);  
    }

    private void OnClientDisconnected(ulong clientId)
    {
        LobbyAudioManager.Instance.PlayFailUISfx();
        if (!NetworkManager.Singleton.IsHost)
        {
            ClearAllUI();
            NetworkManager.Singleton.Shutdown();
        }
        else
        {
            connectedClientText.text = "Player: " + NetworkManager.Singleton.ConnectedClients.Count;
            if (NetworkManager.Singleton.ConnectedClients.Count < MaxClients)
            {
                startButton.interactable = false;
            }
        }

        StopAllCoroutines(); 
        loadingSpinner.SetActive(false);
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddressText.text = ip.ToString();
                    ipAddress = ip.ToString();
                    return ip.ToString();
                }
            }
            ShowError("No valid IPv4 address found!");
            return string.Empty;
        }
        catch (Exception ex)
        {
            ShowError($"Error retrieving local IP: {ex.Message}");
            return string.Empty;
        }
    }

    public void SetIpAddress()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipAddress;
    }

    private void ClearAllUI()
    {
        ipAddressText.text = "";
        stopButton.gameObject.SetActive(false);
        hostButton.interactable = true;
        joinButton.interactable = true;
        ipInput.interactable = true;
        connectedPanelTransform.gameObject.SetActive(false);
        cancelClientButton.gameObject.SetActive(false);
        startButton.interactable = false;

        loadingSpinner.SetActive(false);
    }

    public void StartGame()
    {
        LobbyAudioManager.Instance.PlayButtonClickSfx();
        StartGameClientRpc();
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        Loader.LoadNetwork(Loader.Scene.MainScene);
    }

    private void ShowStatus(string message)
    {
        statusText.text = message;
    }

    private void ShowError(string message)
    {
        statusText.text = "Error: " + message;
        statusText.color = Color.red; 
    }

 

     private IEnumerator HideConnectionAfterTimeout(float timeout)
    {
        yield return new WaitForSeconds(timeout);
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsConnectedClient)
        {
            loadingSpinner.SetActive(false);
            ShowError("Connection timed out.");
            OnFailedToJoinGame?.Invoke(this, EventArgs.Empty); 
        }
    }
}
