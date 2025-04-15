
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseHandler : NetworkBehaviour
{
    public bool Pause = false;
    private PlayerInput _playerinput;
    [SerializeField] private GamePauseEvent _pauseEvent;    

    private void Awake()
    {
        _playerinput = GetComponent<PlayerInput>();
    }
    private void OnEnable()
    {
        _playerinput.actions["Pause"].performed += OnPauseAction; 
    }

    private void OnDisable()
    {
        _playerinput.actions["Pause"].performed -= OnPauseAction; 
    }

    private void OnPauseAction(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        _pauseEvent.InvokePauseEvent();
    }
}
