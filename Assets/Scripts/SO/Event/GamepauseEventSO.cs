using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GamePauseEvent", menuName = "Events/GamePauseEvent")]
public class GamePauseEvent : ScriptableObject
{
    public event EventHandler OnPause;   
    public void InvokePauseEvent()
    {
        OnPause?.Invoke(this, EventArgs.Empty);  
    }
}
