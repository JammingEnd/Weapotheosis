using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerVisualHandler : NetworkBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject ClientHands;
    
    public override void OnStartLocalPlayer()
    {
        _camera.cullingMask &= ~(1 << LayerMask.NameToLayer("LocalPlayer"));
    }

    public override void OnStartClient()
    {
        if(!isLocalPlayer)
        {
            ClientHands.SetActive(false);
        }
    }
}
