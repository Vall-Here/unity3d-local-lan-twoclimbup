using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetLoadingSceneController : NetworkBehaviour
{
    [SerializeField] private Image loadingBar; 
    [SerializeField] private TMPro.TextMeshProUGUI loadingText;  
    private float loadDuration = 5f;          

    void Start()
    {
        loadingBar.fillAmount = 0f;
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        
        float elapsedTime = 0f;
        while (elapsedTime < loadDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / loadDuration);
            loadingBar.fillAmount = progress;
            loadingText.text = "Loading... " + Mathf.FloorToInt(progress * 100) + "%";  
            yield return null;
        }
        
        Loader.NetworkLoaderCallback();
    }

}
