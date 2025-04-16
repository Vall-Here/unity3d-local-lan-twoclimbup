using System;
using System.Collections;
using System.Collections.Generic;
using DiasGames.Components;
using DiasGames.Controller;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


public class CharacterSpawner : NetworkBehaviour
{
    public GameObject characterPrefab;
    public Transform[] spawnPoint;

    [SerializeField] private ObiSpawner _obiSpawner;
    [SerializeField] private ChainPhysics _chainPhysics;

    private NetworkObject player1;
    private NetworkObject player2;
     private bool cutsceneActive = true;

    [Header("Cutscene Settings")]
    [SerializeField] private GameObject slateCutscene;
    [SerializeField] private GameObject cutsceneCamera;
    [SerializeField] private GameObject _mainCamera; 
    // public UnityEvent OnCutsceneFinished; 

    public override void OnNetworkSpawn()
    {    
        if (IsServer)
        {
            _obiSpawner = GetComponent<ObiSpawner>();   
            _chainPhysics = GetComponent<ChainPhysics>();
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsServer)
        {
            // Jalankan cutscene di semua client
            // PlayCutsceneClientRpc();
            
            // Spawn player tapi nonaktifkan kontrol dulu
            foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                GameObject playerObject = Instantiate(characterPrefab, spawnPoint[clientId].position, spawnPoint[clientId].rotation);
                playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
                
                if (playerObject.TryGetComponent<CSPlayerController>(out var controller))
                {
                    controller.enabled = false;
                }

                if (clientId == 0)
                {
                    player1 = playerObject.GetComponent<NetworkObject>();
                }
                else if (clientId == 1)
                {
                    player2 = playerObject.GetComponent<NetworkObject>();
                }
            }

            if (player1 != null && player2 != null && player1.GetComponent<RigidbodyMover>().Grounded && player2.GetComponent<RigidbodyMover>().Grounded)
            {
                SpawnObiRopeClientRpc(player1, player2);
                SetChainObjectClientRpc(player1, player2);
            }
        }
    }

    // [ClientRpc]
    // private void PlayCutsceneClientRpc()
    // {
    //     Debug.Log("Playing cutscene on client");
    //     cutsceneActive = true;
    //     slateCutscene.SetActive(true);
    //     // OnCutsceneFinished.Invoke();
    // }

    [ServerRpc(RequireOwnership = false)]
    public void FinishCutsceneServerRpc()
    {
        
        if(IsServer) {
            SetupCameraClientRpc();
        }

    }

    [ClientRpc]
    public void SetupCameraClientRpc(){
        cutsceneActive = false;
        slateCutscene.SetActive(false);
        if(player1 != null) {

            player1.GetComponent<CameraRefference>().InitiateAfterObjectNetSpawn();
            if (player1.TryGetComponent<CSPlayerController>(out var controller))
            {
                controller.enabled = true;
            }
        }

        if(player2 != null) {

            player2.GetComponent<CameraRefference>().InitiateAfterObjectNetSpawn();
            if (player2.TryGetComponent<CSPlayerController>(out var controller))
            {
                controller.enabled = true;
            }
        
        }
            cutsceneCamera.SetActive(false);
            _mainCamera.SetActive(true);
      
    }


    [ClientRpc]
    private void SpawnObiRopeClientRpc(NetworkObjectReference objec1, NetworkObjectReference objec2)
    {
        Debug.Log("Spawning Obi Rope");

        if (objec1.TryGet(out NetworkObject player1Obj) && objec2.TryGet(out NetworkObject player2Obj))
        {
            _obiSpawner.SpawnObiRope(player1Obj.transform, player2Obj.transform);
        }
    }

    [ClientRpc]
    private void SetChainObjectClientRpc(NetworkObjectReference objec1, NetworkObjectReference objec2){
        _chainPhysics.SetChainObject(objec1, objec2);
    }
}
