using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MenuSettingUI : MonoBehaviour
{
    [SerializeField] private Transform settingPanel;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button closeSettingButton;
    [SerializeField] private Button exitAppButton;

    private void Start()
    {
        settingButton.onClick.AddListener(ShowSettingPanel);
        closeSettingButton.onClick.AddListener(HideSettingPanel);
        exitAppButton.onClick.AddListener(ExitApp);
        settingPanel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        settingButton.onClick.RemoveAllListeners();
        closeSettingButton.onClick.RemoveAllListeners();
        exitAppButton.onClick.RemoveAllListeners();
    }

    private void ShowSettingPanel()
    {
        if (LobbyAudioManager.Instance != null)
            LobbyAudioManager.Instance.PlayOpenUISfx();
        settingPanel.gameObject.SetActive(true);
    }

    private void HideSettingPanel()
    {
        if (LobbyAudioManager.Instance != null)
            LobbyAudioManager.Instance.PlayCloseUISfx();
        settingPanel.gameObject.SetActive(false);
    }

    private void ExitApp()
    {
        if (LobbyAudioManager.Instance != null)
            LobbyAudioManager.Instance.PlayButtonClickSfx();

        // Proper shutdown sequence
        StartCoroutine(SafeExitCoroutine());
    }

    private IEnumerator SafeExitCoroutine()
    {
        // Handle network shutdown if exists
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            LobbyLanManager.Instance.CleanupNetworkManager();
            NetworkManager.Singleton.Shutdown();
            yield return new WaitUntil(() => !NetworkManager.Singleton.IsListening);
        }

        // Handle other cleanup
        System.GC.Collect();
        yield return null;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}