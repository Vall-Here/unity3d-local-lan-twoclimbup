
using Unity.Netcode;
using UnityEngine;


public class CameraRefference : NetworkBehaviour
{
    [SerializeField] private Transform _playerCameraTrack;
    [SerializeField] private Transform _playerCameraRoll;
    [SerializeField] private Transform _playerCameraCrawl;
    [SerializeField] public Transform _playerRopeTransform;
    [SerializeField] private Animator _playerAnimator;

    [SerializeField] public TMPro.TextMeshProUGUI _nameTag;
    [SerializeField] public TMPro.TextMeshProUGUI _backnameTag;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        InitiateAfterObjectNetSpawn();
        
    }

    public void InitiateAfterObjectNetSpawn(){

        Cinemachine.CinemachineStateDrivenCamera _statedriven = GameObject.FindAnyObjectByType<Cinemachine.CinemachineStateDrivenCamera>();

        Cinemachine.CinemachineVirtualCamera _rollcamera = GameObject.FindGameObjectWithTag("RollCamera").GetComponent<Cinemachine.CinemachineVirtualCamera>(); ;

        Cinemachine.CinemachineVirtualCamera _crawlcamera = GameObject.FindGameObjectWithTag("CrouchCamera").GetComponent<Cinemachine.CinemachineVirtualCamera>(); ;

        if(IsOwner) {   
            if (_statedriven != null){
                _statedriven.Follow = _playerCameraTrack;
                _statedriven.m_AnimatedTarget = _playerAnimator;
            }
            if (_crawlcamera != null)
            {
                _crawlcamera.Follow = _playerCameraCrawl;
            }
            if (_rollcamera != null)
            {
                _rollcamera.Follow = _playerCameraRoll;
            }

            // if (PlayerPrefs.HasKey("PlayerName")){
            // string Playername = PlayerPrefs.GetString("PlayerName");
            // SetNameTag(Playername);
            // }
    
        }
    
    }

    public void SetNameTag(string name){
        _nameTag.text = name;
        _backnameTag.text = name;
    }
}
