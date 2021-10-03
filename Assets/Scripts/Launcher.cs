using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Unstable.UI;

namespace Unstable
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        private const string playerNamePrefKey = "PlayerName";

        private const string playerReadyKey = "r";

        private readonly string gameVersion = "0.1.0";

        private Player masterClient;

        public byte MaxPlayersPerRoom = 4;

        public InputField PlayerName;

        public GameObject NamePanel;

        public GameObject RoomButtonsPanel;

        public GameObject RoomPanel;

        public Button ReadyButton;

        public Button KickButton;

        public Button PromoteButton;

        public List<GameObject> RoomPanelServerItems;

        public List<GameObject> RoomPanelClientItems;

        public ListView PlayerList;

        public GameObject ProgressLabel;

        public InputField DisplayRoomName;

        public InputField JoinRoomName;

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.EnableCloseConnection = true;
        }

        private void ResetUI()
        {
            ProgressLabel.SetActive(false);
            RoomButtonsPanel.SetActive(false);
            RoomPanel.SetActive(false);
            NamePanel.SetActive(true);
        }

        public void Start()
        {
            PlayerList.ItemSelectedEvent += PlayerListItemSelected;

            ResetUI();
        }

        public void Connect()
        {
            string name = PlayerName.text;
            if (string.IsNullOrWhiteSpace(name))
                return;

            PhotonNetwork.NickName = name;
            PlayerPrefs.SetString(playerNamePrefKey, name);

            ProgressLabel.SetActive(true);
            NamePanel.SetActive(false);

            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }

        public override void OnConnectedToMaster()
        {
            ProgressLabel.SetActive(false);
            RoomPanel.SetActive(false);
            RoomButtonsPanel.SetActive(true);

            PhotonNetwork.SetPlayerCustomProperties(new Hashtable
            {
                { playerReadyKey, false }
            });
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);

            ResetUI();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        { }

        public void JoinRoom()
        {
            string roomName = JoinRoomName.text;
            if (string.IsNullOrWhiteSpace(roomName))
                return;

            PhotonNetwork.JoinRoom(roomName);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"{returnCode} => {message}");
        }

        public void JoinRandomRoom()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        public void CreateRoom()
        {
            string roomName = GenerateRandomRoomName();

            PhotonNetwork.CreateRoom(roomName, new RoomOptions
            {
                MaxPlayers = MaxPlayersPerRoom,
                PublishUserId = true,
                CustomRoomProperties = new Hashtable
                {
                    // { "m", "5x5MS-S-S| | |S-Z-S| | |S-S-S" }
                    { "m", "5x5MS-S-S| | |S-D-S| | |S-S-S" }
                }
            });
        }

        private void UpdateRoomButtons()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                SetActive(RoomPanelServerItems, false);
                SetActive(RoomPanelClientItems, true);
            }
            else
            {
                SetActive(RoomPanelServerItems, true);
                SetActive(RoomPanelClientItems, false);
                KickButton.interactable = false;
                PromoteButton.interactable = false;
            }
        }

        public override void OnJoinedRoom()
        {
            RoomButtonsPanel.SetActive(false);
            RoomPanel.SetActive(true);
            PlayerList.Clear();
            UpdateRoomButtons();

            DisplayRoomName.text = PhotonNetwork.CurrentRoom.Name;

            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                AddPlayer(player);

                if (player.IsMasterClient)
                    masterClient = player;
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            AddPlayer(newPlayer);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"Player left {otherPlayer} - {otherPlayer.UserId}");
            PlayerList.RemoveItem(otherPlayer.UserId);
        }

        private string GetPlayerDisplayName(Player player)
        {
            string result = player.NickName;
            if (player.IsMasterClient)
                result += " (Host)";
            else if (player.CustomProperties[playerReadyKey] is bool playerReady && playerReady)
                result += " - Ready";
            return result;
        }

        private void AddPlayer(Player player)
        {
            PlayerList.AddItem(player.UserId, GetPlayerDisplayName(player));
            Debug.Log($"Player joined {player} - {player.UserId}");
        }

        public void LeaveGame()
        {
            PhotonNetwork.LeaveRoom();

            RoomButtonsPanel.SetActive(true);
            RoomPanel.SetActive(false);
        }

        public void ReadyToggle()
        {
            if (PhotonNetwork.IsMasterClient || !(PhotonNetwork.LocalPlayer.CustomProperties[playerReadyKey] is bool playerReady))
                return;

            ReadyButton.GetComponentInChildren<Text>().text = playerReady ? "Ready" : "Unready";

            PhotonNetwork.SetPlayerCustomProperties(new Hashtable
            {
                { playerReadyKey, !playerReady }
            });
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            PlayerList.UpdateItem(targetPlayer.UserId, GetPlayerDisplayName(targetPlayer));
        }

        public Player GetSelectedPlayer()
        {
            if (!PhotonNetwork.IsMasterClient)
                return null;

            string selectedId = PlayerList.SelectedItem?.Value;
            if (string.IsNullOrWhiteSpace(selectedId))
                return null;

            return PhotonNetwork.PlayerListOthers.FirstOrDefault(x => x.UserId == selectedId);
        }

        public void KickSelectedPlayer()
        {
            Player player = GetSelectedPlayer();
            if (player == null)
                return;

            PhotonNetwork.CloseConnection(player);
        }

        public void PromoteSelectedPlayer()
        {
            Player player = GetSelectedPlayer();
            if (player == null)
                return;

            PhotonNetwork.SetMasterClient(player);

            ReadyButton.GetComponentInChildren<Text>().text = "Ready";

            PhotonNetwork.SetPlayerCustomProperties(new Hashtable
            {
                { playerReadyKey, false }
            });
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.Log($"OnMasterClientSwitched [{masterClient.NickName}] => [{newMasterClient.NickName}]");

            PlayerList.UpdateItem(masterClient.UserId, GetPlayerDisplayName(masterClient));
            PlayerList.UpdateItem(newMasterClient.UserId, GetPlayerDisplayName(newMasterClient));

            masterClient = newMasterClient;

            UpdateRoomButtons();
        }

        private void PlayerListItemSelected(object sender, ListItem item)
        {
            KickButton.interactable = false;
            PromoteButton.interactable = false;
            if (item == null)
                return;

            Player player = PhotonNetwork.PlayerListOthers.FirstOrDefault(x => x.UserId == item.Value);
            if (player == null)
                return;

            KickButton.interactable = true;
            PromoteButton.interactable = true;
        }

        public void StartRoom()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            PhotonNetwork.CurrentRoom.IsOpen = false;

            Debug.Log("We load 'Arena'.");
            PhotonNetwork.LoadLevel("Arena");
        }

        private static void SetActive(List<GameObject> objects, bool value)
        {
            foreach (GameObject gameObject in objects)
                gameObject.SetActive(value);
        }

        private static string GenerateRandomRoomName()
        {
            string result = "";
            for (int i = 0; i < 6; i++)
            {
                bool useLetter = Random.Range(0, 2) == 0;
                if (useLetter)
                    result += (char)(65 + Random.Range(0, 26));
                else
                    result += Random.Range(0, 10).ToString();
            }
            return result;
        }
    }
}
