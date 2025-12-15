using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

namespace SteamLobbyUI
{
        public class PlayerNetworkHandler : NetworkBehaviour
        {
            [SyncVar(hook = nameof(OnReadyStatusChanged))]  
            public bool isReady = false;    
            public Button readyButton;
            public TextMeshProUGUI NameText;

            private void Start()
            {
                readyButton.interactable = isLocalPlayer;
            }

            public override void OnStartLocalPlayer()
            {
                base.OnStartLocalPlayer();
                readyButton.interactable = true;
                isReady = false;
            }

            public override void OnStartClient()
            {           
                base.OnStartClient();   
                LobbyUIManager.Instance.RegisterPlayer(this);
                
            }

            [Command]
            void CmdSetReady()
            {
                isReady = !isReady;     
                OnReadyStatusChanged(!isReady, isReady);        
            }
            
            public void OnReadyButtonPressed()
            {
                CmdSetReady();
            }   
            
            void SetSelectedButtonColor(Color color)
            {
                ColorBlock cb = readyButton.colors;
                cb.normalColor = color;
                cb.selectedColor = color;
                cb.disabledColor = color;   
                readyButton.colors = cb;
            }   
            

            void OnReadyStatusChanged(bool oldValue, bool newValue) 
            {
                if (NetworkServer.active)
                {
                    LobbyUIManager.Instance.CheckAllPlayersReady(); 
                }

                if (isReady)
                {
                    SetSelectedButtonColor(Color.forestGreen);
                }
                else
                {
                    SetSelectedButtonColor(Color.white);
                }
            }
        }
    
}
