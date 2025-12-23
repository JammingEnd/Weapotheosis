using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerVisualHandler : NetworkBehaviour
{
    [SerializeField] private Camera _camera;
    
    public override void OnStartLocalPlayer()
    {
        int bodyLayer = LayerMask.NameToLayer("PlayerBody");
        int handsLayer = LayerMask.NameToLayer("PlayerHands");

        // Hide own body
        _camera.cullingMask &= ~(1 << bodyLayer);

        // Show hands (ensure layer is included)
        _camera.cullingMask |= (1 << handsLayer);
    }
    
}
