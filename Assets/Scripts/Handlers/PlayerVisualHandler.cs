using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerVisualHandler : NetworkBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject LocalPlayerModel;
    [SerializeField] private GameObject ClientHands;
    
    public override void OnStartLocalPlayer()
    {
        if(!isLocalPlayer)
        {
            LocalPlayerModel.SetActive(true);
            ClientHands.SetActive(false);
            return;
        }
        LocalPlayerModel.SetActive(false);
        ClientHands.SetActive(true);
    }
}
