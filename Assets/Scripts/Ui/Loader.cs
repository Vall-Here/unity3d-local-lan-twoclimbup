using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader {

    public enum Scene
    {
        LoadingScene, 
        NetLoadingScene, 
        LobbyScene,
        MainScene,
        obstacle,


    }
    [SerializeField] private static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        Debug.Log("Load");
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(Scene.LoadingScene.ToString(), LoadSceneMode.Single); 

    }
    
    public static void LoadNetwork(Scene targetScene)
    {
        Debug.Log("LoadNetwork");
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(Scene.NetLoadingScene.ToString(), LoadSceneMode.Single);
    }
    
    public static void LoaderCallback()
    {
        Debug.Log("LoaderCallback");
        SceneManager.LoadScene(targetScene.ToString(),LoadSceneMode.Single);
    }

    public static void NetworkLoaderCallback()
    {
        Debug.Log("NetworkLoaderCallback");
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

}


