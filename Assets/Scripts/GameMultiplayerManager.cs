using System;
using System.Collections;
using System.Collections.Generic;
using Mono.CSharp;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameMultiplayerManager : NetworkBehaviour
{
    public static GameMultiplayerManager Instance;

    private const int MAX_PLAYERS = 2; 

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;
    // public event EventHandler OnHostConnectionLost;

    private bool isLocalGamePaused = false;
    private bool isInitialized = false;
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<float> lastHeartbeatTime = new NetworkVariable<float>(0f);

    private Dictionary<ulong, bool> playerPausedDictionary;
    public NetworkVariable<ulong> pausedByPlayer = new NetworkVariable<ulong>(0);
    public List<ulong> connectedPlayers = new List<ulong>();
    private bool autoTestGamePausedState;

    [Header("Event bus")]
    [SerializeField] private GamePauseEvent _pauseEvent;

    [Header("Pause Timeout")]
    [SerializeField] private float pauseTimeoutDuration = 60f;
    private float pauseTimeoutTimer;
    private bool isTimeoutActive;

    [Header("Connection Settings")]
    [SerializeField] private float heartbeatInterval = 1f;
    [SerializeField] private float hostDisconnectCheckInterval = 2f;
    [SerializeField] private float maxWaitTimeForHostResponse = 5f;
    private Coroutine hostDisconnectCheckCoroutine;
    private float lastPingTime;
    private bool isWaitingForPingResponse;


    private enum State
    {
        WaitingToStart,
        GamePlaying,
        GameOver,
    }

    void Awake()
    {
        Instance = this;
        playerPausedDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        _pauseEvent.OnPause += GameInput_OnPauseAction;
     
    }

    void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;
        _pauseEvent.OnPause -= GameInput_OnPauseAction;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    
         if (hostDisconnectCheckCoroutine != null)
        {
            StopCoroutine(hostDisconnectCheckCoroutine);
        }
    }


    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;
        lastHeartbeatTime.OnValueChanged += OnHeartbeatUpdated;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            playerPausedDictionary[NetworkManager.Singleton.LocalClientId] = false;
            StartCoroutine(ServerHeartbeatCoroutine());
        }

        isInitialized = true;
        InitializePlayerPauseStates();
        
        if (!IsServer && IsClient)
        {
            StartHostDisconnectChecker();
        }
    }
    private void InitializePlayerPauseStates()
    {
        if (!isInitialized || !IsSpawned) return;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerPausedDictionary.ContainsKey(clientId))
            {
                playerPausedDictionary[clientId] = false;
            }
        }
    }

    private void OnHeartbeatUpdated(float oldValue, float newValue)
    {
        if (!IsServer && newValue > lastPingTime)
        {
            isWaitingForPingResponse = false;
        }
    }

    private IEnumerator ServerHeartbeatCoroutine()
    {
        while (IsServer)
        {
            lastHeartbeatTime.Value = Time.unscaledTime;
            yield return new WaitForSeconds(heartbeatInterval);
        }
    }

 
    private void OnClientConnected(ulong clientId)
    {
        if (state.Value == State.GamePlaying)
        {
            NetworkManager.Singleton.DisconnectClient(clientId);
            Debug.Log($"Player {clientId} was rejected because the game has already started.");
        }
        else
        {
            connectedPlayers.Add(clientId);
            CheckPlayerLimit();
            
    
            if (!playerPausedDictionary.ContainsKey(clientId))
            {
                playerPausedDictionary[clientId] = false;
            }
        }
    }

     private void StartHostDisconnectChecker()
    {
        if (hostDisconnectCheckCoroutine != null)
        {
            StopCoroutine(hostDisconnectCheckCoroutine);
        }
        hostDisconnectCheckCoroutine = StartCoroutine(HostDisconnectCheckRoutine());
    }

    private IEnumerator HostDisconnectCheckRoutine()
    {
        var waitTime = new WaitForSecondsRealtime(hostDisconnectCheckInterval);
        float lastResponseTime = Time.unscaledTime;

        while (true)
        {
      
            lastPingTime = Time.unscaledTime;
            isWaitingForPingResponse = true;
            PingHostServerRpc();

            yield return waitTime;

            
            if (isWaitingForPingResponse)
            {
                if (Time.unscaledTime - lastResponseTime >= maxWaitTimeForHostResponse)
                {
                    Debug.LogError("Host not responding - assuming host has crashed");
                    HandleHostDisconnected();
                    yield break;
                }
            }
            else
            {
                lastResponseTime = Time.unscaledTime;
            }

            CheckNetworkStability();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PingHostServerRpc(ServerRpcParams rpcParams = default)
    {
        RespondToPingClientRpc(rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void ShowConnectionWarningClientRpc()
    {
   
        Debug.Log("Warning: Connection to host is unstable");
    }

    [ClientRpc]
    private void RespondToPingClientRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            isWaitingForPingResponse = false;
        }
    }

    private void HandleHostDisconnected()
    {
        Debug.Log("Host disconnected - Forcing immediate shutdown");
     
        if (hostDisconnectCheckCoroutine != null)
        {
            StopCoroutine(hostDisconnectCheckCoroutine);
        }
        

        ForceImmediateShutdown();
    }


    private void ForceImmediateShutdown()
    {

        Time.timeScale = 1f;
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        Loader.Load(Loader.Scene.LobbyScene);
    }




    private void OnClientDisconnected(ulong clientId)
    {
    if (!IsServer && clientId == NetworkManager.ServerClientId)
    {
        Debug.Log($"Host {clientId} disconnected - Immediate shutdown");
        ForceImmediateShutdown();
    }
     }


    

   

    private void CheckPlayerLimit()
    {
        if (connectedPlayers.Count > MAX_PLAYERS)
        {
            ulong excessPlayer = connectedPlayers[2];
            NetworkManager.Singleton.DisconnectClient(excessPlayer);
            connectedPlayers.Remove(excessPlayer);
            Debug.Log($"Player {excessPlayer} has been disconnected because the player limit is 2.");
        }
    }



    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        autoTestGamePausedState = true;
        
    }


    [ServerRpc(RequireOwnership = false)]
    public void BackToMainMenuServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("BackToMainMenuServerRpc called.");
        BackToMainMenuClientRpc();
    }


    [ClientRpc]
    public void BackToMainMenuClientRpc()
    {
        Debug.Log("BackToMainMenuClientRpc triggered on client.");
        Time.timeScale = 1f;
        NetworkManager.Singleton.Shutdown();
        Loader.Load(Loader.Scene.LobbyScene);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    [ClientRpc]
    public void BackToMainMenuNetClientRpc()
    {
        NetworkManager.Singleton.Shutdown();
        Loader.LoadNetwork(Loader.Scene.LobbyScene);
    }


    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if(isGamePaused.Value){
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }



    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if (!IsServer) return;

        switch (state.Value)
        {
            case State.WaitingToStart:
                break;
            case State.GamePlaying:
                HandlePauseTimeout();
                break;
            case State.GameOver:
                break;
        }
    }

     private void HandlePauseTimeout()
    {
        if (isGamePaused.Value && isTimeoutActive)
        {
            pauseTimeoutTimer -= Time.unscaledDeltaTime;
            
            if (pauseTimeoutTimer <= 0f)
            {
                ForceUnpauseGame();
            }
        }
    }

    [ServerRpc]
    private void ForceUnpauseGameServerRpc()
    {
        ForceUnpauseGame();
    }

    private void ForceUnpauseGame()
    {
        if (!IsServer) return;
        foreach (var key in playerPausedDictionary.Keys)
        {
            playerPausedDictionary[key] = false;
        }
        pausedByPlayer.Value = 0;
        UpdatePauseRequestClientRpc(0);
        StopPauseTimeout();
        isGamePaused.Value = false;
    }

    private void StartPauseTimeout()
    {
        pauseTimeoutTimer = pauseTimeoutDuration;
        isTimeoutActive = true;
        Debug.Log($"Pause timeout started ({pauseTimeoutDuration}s)");
    }
    private void StopPauseTimeout()
    {
        isTimeoutActive = false;
        Debug.Log("Pause timeout stopped");
    }

    private void LateUpdate()
    {
        if (autoTestGamePausedState)
        {
            autoTestGamePausedState = false;
            TestGamePausedState();
        }      
    }
    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }


    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }

    public bool IsWaitingToStart()
    {
        return state.Value == State.WaitingToStart;
    }


// PAUSE CONTROL =================================================================================================
    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        
        TogglePauseGame();
    }

    public void TogglePauseGame()
    {
        isLocalGamePaused = !isLocalGamePaused;
        if (isLocalGamePaused)
        {
            PauseGameServerRpc();
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            UnpauseGameServerRpc();
            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

  [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        
        if (playerPausedDictionary.ContainsKey(clientId))
        {
            playerPausedDictionary[clientId] = true;
            pausedByPlayer.Value = clientId;
            UpdatePauseRequestClientRpc(clientId);

            if (!isGamePaused.Value)
            {
                StartPauseTimeout();
            }
            
            TestGamePausedState();
        }
    }

   [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        
        if (playerPausedDictionary.ContainsKey(clientId))
        {
            playerPausedDictionary[clientId] = false;
            
            if (pausedByPlayer.Value == clientId)
            {
                pausedByPlayer.Value = 0;
                UpdatePauseRequestClientRpc(0);
            }
            
            StopPauseTimeout();
            
            TestGamePausedState();
        }
    }



    [ClientRpc]
    private void UpdatePauseRequestClientRpc(ulong playerId)
    {
        Debug.Log($"Player {playerId} triggered the pause.");
        pausedByPlayer.Value = playerId; 
    }


   private void TestGamePausedState()
    {
        bool anyPlayerPaused = false;
        int pausedPlayersCount = 0;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDictionary.TryGetValue(clientId, out bool isPaused) && isPaused)
            {
                anyPlayerPaused = true;
                pausedPlayersCount++;
            }
        }

        if (pausedPlayersCount == NetworkManager.Singleton.ConnectedClients.Count)
        {
            Debug.Log("All players have paused the game");
        }

        isGamePaused.Value = anyPlayerPaused;
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Server: Client {clientId} disconnected, handling disconnect.");

        playerPausedDictionary.Remove(clientId);

        if (pausedByPlayer.Value == clientId)
        {
            pausedByPlayer.Value = 0;
            UpdatePauseRequestClientRpc(0);
            bool anyOtherPlayerPaused = false;
            foreach (var kvp in playerPausedDictionary)
            {
                if (kvp.Value) anyOtherPlayerPaused = true;
            }
            
            if (!anyOtherPlayerPaused)
            {
                isGamePaused.Value = false;
                StopPauseTimeout();
            }
        }
        
        TestGamePausedState();
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Host disconnected, shutting down");
            NetworkManager.Singleton.Shutdown();
            return;
        }
        if (NetworkManager.Singleton.ConnectedClients.Count <= 1)
        {
            HandleSinglePlayerLeft();
        }
    }

    private void HandleSinglePlayerLeft()
    {
        Debug.Log("Only one player left, pausing game");
        isGamePaused.Value = true;
        pausedByPlayer.Value = NetworkManager.ServerClientId;
        UpdatePauseRequestClientRpc(NetworkManager.ServerClientId);
        pauseTimeoutDuration = 10f;
        StartPauseTimeout();
        NotifySinglePlayerLeftClientRpc();
    }

    [ClientRpc]
    private void NotifySinglePlayerLeftClientRpc()
    {
        Debug.Log("Other players have left the game. Returning to lobby soon...");
    }



//======================================= Heartbeat =============================================

// Tambahkan di GameMultiplayerManager.cs
    [ServerRpc(RequireOwnership = false)]
    private void HeartbeatServerRpc(ServerRpcParams rpcParams = default)
    {
    }

    private IEnumerator HostHeartbeatCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            if (!IsServer) continue;
            
            // Kirim heartbeat ke semua client
            HeartbeatClientRpc();
        }
    }

    [ClientRpc]
    private void HeartbeatClientRpc()
    {
        // Reset timer ketika menerima heartbeat dari host
        if (hostDisconnectCheckCoroutine != null)
        {
            StopCoroutine(hostDisconnectCheckCoroutine);
            hostDisconnectCheckCoroutine = StartCoroutine(HostDisconnectCheckRoutine());
        }
    }



    private void CheckNetworkStability()
    {
        if (!IsClient || !NetworkManager.Singleton.IsConnectedClient) return;
        
        var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        var rtt = transport.GetCurrentRtt(NetworkManager.ServerClientId); 
        
        if (rtt > 500) 
        {
            Debug.LogWarning($"High latency detected with host: {rtt}ms");
            ShowConnectionWarningClientRpc();
        }
    }
}
