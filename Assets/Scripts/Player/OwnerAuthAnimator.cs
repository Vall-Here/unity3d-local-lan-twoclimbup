using Unity.Netcode;
using Unity.Netcode.Components;

public class OwnerAuthAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
