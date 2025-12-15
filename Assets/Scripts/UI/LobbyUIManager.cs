using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;
using TMPro;

namespace SteamLobbyUI      
{
        public class LobbyUIManager : NetworkBehaviour
        {
            public  static LobbyUIManager Instance;
            public Transform playerListParent;  
            public List<TextMeshProUGUI> playerNameList = new List<TextMeshProUGUI>();     
            public List<PlayerNetworkHandler> playerLobbyHandlers = new List<PlayerNetworkHandler>();
            public Button PlayGameButton;

            private void Awake()
            {
                if(Instance == null)       
                {
                    Instance = this;     
                }
                else
                {
                    Destroy(gameObject);     
                }
            }

            private void Start()
            {
                PlayGameButton.interactable = false;
            }

            public void UpdatePlayerLobbyUI()
            {
                playerNameList.Clear();
                playerLobbyHandlers.Clear();

                var lobby = new CSteamID(SteamLobby.Instance.LobbyID);
                int playerCount = SteamMatchmaking.GetNumLobbyMembers(lobby);
                
                CSteamID hostId = new CSteamID(ulong.Parse(SteamMatchmaking.GetLobbyData(lobby, "HostAddress")));
                List<CSteamID> orderedPlayers = new List<CSteamID>();

                if (playerCount == 0)
                {
                    Debug.LogWarning("No Players In lobby");
                    StartCoroutine(RetryUpdate());
                    return;
                }
                
                orderedPlayers.Add(hostId);

                for (int i = 0; i < playerCount; i++)
                {
                    CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(lobby, i);
                    if (memberId != hostId)
                    {
                        orderedPlayers.Add(memberId);
                    }
                }
                
                int j = 0;
                foreach (var player in orderedPlayers)
                {
                    TextMeshProUGUI txtMesh = playerListParent.GetChild(j).GetChild(0).GetComponent<TextMeshProUGUI>();
                    PlayerNetworkHandler handler = playerListParent.GetChild(j).GetComponent<PlayerNetworkHandler>();
                    
                    playerLobbyHandlers.Add(handler);
                    playerNameList.Add(txtMesh);
                    
                    string playerName = SteamFriends.GetFriendPersonaName(player);
                    playerNameList[j].text = playerName;
                    j++;
                }
            }

            public void OnPlayButtonClicked()
            {
                if (NetworkServer.active)
                {
                    //TODO: Change "GameplayScene" to the actual gameplay scene name
                    NewNetworkManager.singleton.ServerChangeScene("Map1");
                }
            }
            
            public void RegisterPlayer(PlayerNetworkHandler playerHandler)
            { 
                playerHandler.transform.SetParent(playerListParent);
                UpdatePlayerLobbyUI();  
            }
            
            [Server]
            public void CheckAllPlayersReady()
            {
                foreach (PlayerNetworkHandler playerHandler in playerLobbyHandlers)
                {
                    if (!playerHandler.isReady)
                    {
                        RpcSetPlayButtonInteractable(false);
                        return; 
                    }
                }
                RpcSetPlayButtonInteractable(true);
            }

            [ClientRpc]
            void RpcSetPlayButtonInteractable(bool truthStatus)
            {
                PlayGameButton.interactable = truthStatus;
            }

            private IEnumerator RetryUpdate()
            {
                yield return new WaitForSeconds(1f);
                UpdatePlayerLobbyUI();
            }
        }
        
}
