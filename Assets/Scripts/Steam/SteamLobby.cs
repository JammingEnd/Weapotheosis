using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using Mirror.FizzySteam;

namespace SteamLobbyUI  
{
    public class SteamLobby : NetworkBehaviour
    {
        public static SteamLobby Instance;
        public GameObject hostButton = null;
        public ulong LobbyID;
        public NetworkManager NetworkManager;
        public PanelSwapper PanelSwapper;
        
        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
        protected Callback<LobbyEnter_t> lobbyEntered;
        protected Callback<LobbyChatUpdate_t> lobbyChatUpdate;
        
        private const string HostAddressKey = "HostAddress";
        
        
        private void Awake()
        {
            if (Instance == null)       
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
            NetworkManager = GetComponent<NetworkManager>();
            if (!SteamManager.Initialized)
            {
                Debug.LogWarning("Steam lobby not initialized");
                return;
            }
            PanelSwapper.gameObject.SetActive(true);
            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        }
        private void OnLobbyCreated(LobbyCreated_t param)
        {
            if(param.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError("Lobby creation failed");
                return;
            }
            Debug.Log("Lobby created. Lobby ID: " + param.m_ulSteamIDLobby);
            NetworkManager.StartHost();
            
            SteamMatchmaking.SetLobbyData(new  CSteamID(param.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
            LobbyID = param.m_ulSteamIDLobby;
        }
        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t param)
        {
            Debug.Log("Join request received for lobby: " + param.m_steamIDLobby);
            if (NetworkClient.active || NetworkClient.isConnected)
            {
                Debug.Log("Already in a lobby, cannot join another.");
                NetworkManager.singleton.StopHost();
                NetworkClient.Shutdown();
                return;
            }
            
            SteamMatchmaking.JoinLobby(param.m_steamIDLobby);
        }
        private void OnLobbyEntered(LobbyEnter_t param)
        {
            // Only join if we’re not the host
            if (NetworkServer.active)
            {
                Debug.Log("Already hosting a lobby.");
                return;
            }

            LobbyID = param.m_ulSteamIDLobby;
            StartCoroutine(ConnectToHost());
        }

        private IEnumerator ConnectToHost()
        {
            var lobby = new CSteamID(LobbyID);
            string hostAddressStr = SteamMatchmaking.GetLobbyData(lobby, HostAddressKey);

            // Wait until lobby data contains the host SteamID
            float timeout = 5f; // optional max wait
            float timer = 0f;
            while (string.IsNullOrEmpty(hostAddressStr) && timer < timeout)
            {
                yield return null; // wait a frame
                timer += Time.deltaTime;
                hostAddressStr = SteamMatchmaking.GetLobbyData(lobby, HostAddressKey);
            }

            if (string.IsNullOrEmpty(hostAddressStr))
            {
                Debug.LogError("Failed to retrieve host SteamID from lobby data!");
                yield break;
            }

            if (!ulong.TryParse(hostAddressStr, out ulong hostSteamId))
            {
                Debug.LogError($"Invalid SteamID format: {hostAddressStr}");
                yield break;
            }

            Debug.Log("Connecting to host: " + hostSteamId);
            NetworkManager.GetComponent<FizzySteamworks>().ClientConnect(hostSteamId.ToString());

            // Optional: swap to lobby panel once connection is initiated
            PanelSwapper.SwapPanel("Lobby");
        }


        private void OnLobbyChatUpdate(LobbyChatUpdate_t param)
        {
            if(param.m_ulSteamIDLobby != LobbyID) return;   
            
            EChatMemberStateChange stateChange = (EChatMemberStateChange)param.m_rgfChatMemberStateChange;
            Debug.Log($"LobbyChatUpdate: {stateChange}");
            
            bool shouldUpdate = stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeLeft) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeDisconnected) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeKicked) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeBanned);

            if (shouldUpdate)
            {
                StartCoroutine(DelayedNameUpdate(0.5f));
                LobbyUIManager.Instance.CheckAllPlayersReady();
            }
        }

        private IEnumerator DelayedNameUpdate(float delay)
        {
            if (LobbyUIManager.Instance == null)
            {
                yield break;
            }
            yield return new WaitForSeconds(delay);
            LobbyUIManager.Instance?.UpdatePlayerLobbyUI(); // stack trace
        }

        public void LeaveLobby()
        {
            CSteamID currentOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(LobbyID));
            CSteamID me = SteamUser.GetSteamID();
            var lobby = new CSteamID(LobbyID);
            List<CSteamID> players = new List<CSteamID>();
            
            int count = SteamMatchmaking.GetNumLobbyMembers(lobby);
            for (int i = 0; i < count; i++)
            {
                players.Add(SteamMatchmaking.GetLobbyMemberByIndex(lobby, i));
            }

            if (LobbyID != 0)
            {
                SteamMatchmaking.LeaveLobby(new CSteamID(LobbyID));
            }
            
            if(NetworkServer.active && me == currentOwner)
            {
                NetworkManager.singleton.StopHost();
            }
            else if(NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
            }
            
            PanelSwapper.gameObject.SetActive(true);
            this.gameObject.SetActive(true);
            PanelSwapper.SwapPanel("MainMenu");
          
        }




        public void HostLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, NetworkManager.maxConnections);
        }
        
        
    }

}

