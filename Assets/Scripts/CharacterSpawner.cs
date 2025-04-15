using System;
using System.Collections;
using System.Collections.Generic;
using DiasGames.Components;
using DiasGames.Controller;
using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    public GameObject characterPrefab;
    public Transform[] spawnPoint;

    [SerializeField] private ObiSpawner _obiSpawner;
    [SerializeField] private ChainPhysics _chainPhysics;

    private NetworkObject player1;
    private NetworkObject player2;

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
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject playerObject = Instantiate(characterPrefab, spawnPoint[clientId].position, spawnPoint[clientId].rotation);
            playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

            if (clientId == 0)
            {
                player1 = playerObject.GetComponent<NetworkObject>();
            }
            else if (clientId == 1)
            {
                player2 = playerObject.GetComponent<NetworkObject>();
            }

            // Set Nama Player
            CameraRefference _playerData = playerObject.GetComponent<CameraRefference>();
            _playerData._nameTag.text = "Player " + clientId;
            _playerData._backnameTag.text = "Player " + clientId;
        }

        if (IsServer && player1 != null && player2 != null & player1.GetComponent<RigidbodyMover>().Grounded && player2.GetComponent<RigidbodyMover>().Grounded)
        {
            SpawnObiRopeClientRpc(player1, player2);
            SetChainObjectClientRpc(player1, player2);
        }
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
