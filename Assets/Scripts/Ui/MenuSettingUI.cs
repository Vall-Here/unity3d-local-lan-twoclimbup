using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSettingUI : MonoBehaviour
{
    [SerializeField] private Transform settingPanel;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button closeSettingButton;
    [SerializeField] private Button ExitAppButton;

    private void Start()
    {
        settingButton.onClick.AddListener(ShowSettingPanel);
        closeSettingButton.onClick.AddListener(HideSettingPanel);
        ExitAppButton.onClick.AddListener(ExitApp);
        settingPanel.gameObject.SetActive(false);
    }

    private void ShowSettingPanel()
    {
        LobbyAudioManager.Instance.PlayOpenUISfx(); 
        settingPanel.gameObject.SetActive(true);

    }

    private void HideSettingPanel()
    {
        LobbyAudioManager.Instance.PlayCloseUISfx();
        settingPanel.gameObject.SetActive(false);
    }

    private void ExitApp()
    {
        LobbyAudioManager.Instance.PlayButtonClickSfx();
        Application.Quit();
    }
}
