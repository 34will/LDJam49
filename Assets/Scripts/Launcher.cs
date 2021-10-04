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

        public byte DefaultMaxPlayersPerRoom = 4;

        public InputField PlayerName;

        public GameObject NamePanel;

        public GameObject RoomButtonsPanel;

        public GameObject RoomPanel;

        public Button ReadyButton;

        public Button KickButton;

        public Button PromoteButton;

        public Button StartGameButton;

        public List<GameObject> RoomPanelServerItems;

        public List<GameObject> RoomPanelClientItems;

        public ListView PlayerList;

        public GameObject ProgressLabel;

        public InputField DisplayRoomName;

        public InputField JoinRoomName;

        public Dropdown RoomTypeDropdown;
        public Text RoomType;

        public InputField RoomWidthInput;
        public Text RoomWidth;

        public InputField RoomHeightInput;
        public Text RoomHeight;
    
        public Text RoomMapStringLabel;
        public Text RoomMapString;
        public InputField RoomMapStringInput;

        public Dropdown RoomGamemodeDropdown;
        public Text RoomGamemode;

        public Dropdown RoomDebrisDropdown;
        public Text RoomDebris;

        public Dropdown RoomDroppersDropdown;
        public Text RoomDroppers;

        public InputField RoomMaxPlayersInput;
        public Text RoomMaxPlayers;

        public Dropdown RoomAllowRandomsDropdown;
        public Text RoomAllowRandoms;

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.EnableCloseConnection = true;
            PhotonPeer.RegisterType(typeof(UnstableRoomOptions), 0, UnstableRoomOptions.Serialize, UnstableRoomOptions.Deserialize);
        }

        private void ResetUI()
        {
            ProgressLabel.SetActive(false);
            RoomPanel.SetActive(false);
            RoomButtonsPanel.SetActive(PhotonNetwork.IsConnected);
            NamePanel.SetActive(!PhotonNetwork.IsConnected);
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

            PhotonNetwork.SetPlayerCustomProperties(new Hashtable { { playerReadyKey, false } });
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
                IsVisible = false,
                MaxPlayers = DefaultMaxPlayersPerRoom,
                PublishUserId = true,
                CustomRoomProperties = new Hashtable { { UnstableRoomOptions.RoomOptionsKey, new UnstableRoomOptions() } }
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
            UpdateRoomUI();

            DisplayRoomName.text = PhotonNetwork.CurrentRoom.Name;

            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                AddPlayer(player);

                if (player.IsMasterClient)
                    masterClient = player;
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (!PhotonNetwork.IsMasterClient)
                UpdateRoomUI();
        }

        private void UpdateRoomUI()
        {
            UnstableRoomOptions options = UnstableRoomOptions.Current;
            if (options == null)
                return;

            if (PhotonNetwork.IsMasterClient)
            {
                switch (options.MapType)
                {
                    case MapType.Map:
                        RoomTypeDropdown.value = 0;
                        break;
                    case MapType.Random:
                        RoomTypeDropdown.value = 1;
                        break;
                }
                RoomWidthInput.text = options.Width.ToString();
                RoomHeightInput.text = options.Height.ToString();
                RoomMapStringInput.text = options.MapString.ToString();
                switch (options.Gamemode)
                {
                    case Gamemode.LastPersonStanding:
                        RoomGamemodeDropdown.value = 0;
                        break;
                    case Gamemode.Tag:
                        RoomGamemodeDropdown.value = 1;
                        break;
                    case Gamemode.CaptureTheFlag:
                        RoomGamemodeDropdown.value = 2;
                        break;
                    case Gamemode.KingOfTheHill:
                        RoomGamemodeDropdown.value = 3;
                        break;
                }
                RoomDebrisDropdown.value = options.Debris ? 0 : 1;
                RoomDroppersDropdown.value = options.Droppers ? 0 : 1;
                RoomMaxPlayersInput.text = PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
                RoomAllowRandomsDropdown.value = PhotonNetwork.CurrentRoom.IsVisible ? 0 : 1;
            }
            else
            {
                switch (options.MapType)
                {
                    case MapType.Map:
                        RoomType.text = "Map";
                        break;
                    case MapType.Random:
                        RoomType.text = "Random";
                        break;
                }
                RoomWidth.text = options.Width.ToString();
                RoomHeight.text = options.Height.ToString();
                RoomMapString.text = options.MapString.ToString();
                switch (options.Gamemode)
                {
                    case Gamemode.LastPersonStanding:
                        RoomGamemode.text = "Last Person Standing";
                        break;
                    case Gamemode.Tag:
                        RoomGamemode.text = "Tag";
                        break;
                    case Gamemode.CaptureTheFlag:
                        RoomGamemode.text = "Capture the Flag";
                        break;
                    case Gamemode.KingOfTheHill:
                        RoomGamemode.text = "King of the Hill";
                        break;
                }
                RoomDebris.text = options.Debris ? "Yes" : "No";
                RoomDroppers.text = options.Droppers ? "Yes" : "No";
                RoomMaxPlayers.text = PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
                RoomAllowRandoms.text = PhotonNetwork.CurrentRoom.IsVisible ? "Yes" : "No";
            }
        }

        public void RoomTypeChanged()
        {
            int value = RoomTypeDropdown.value;
            if (!PhotonNetwork.IsMasterClient || value < 0 || value > 1)
                return;

            UnstableRoomOptions options = UnstableRoomOptions.Current;
            if (options == null)
                return;

            options.MapType = (MapType)value;

            UnstableRoomOptions.Current = options;
        }

        public void RoomWidthChanged()
        {
            string value = RoomWidthInput.text;
            if (!PhotonNetwork.IsMasterClient || string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int intValue))
                return;
            if (intValue < 0)
            {
                RoomWidth.text = "0";
                return;
            }

            UnstableRoomOptions options = UnstableRoomOptions.Current;
            if (options == null)
                return;

            options.Width = intValue;
            ValidifyRoomSize(null, options);

            UnstableRoomOptions.Current = options;
        }

        public void RoomHeightChanged()
        {
            string value = RoomHeightInput.text;
            if (!PhotonNetwork.IsMasterClient || string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int intValue))
                return;
            if (intValue < 0)
            {
                RoomHeight.text = "0";
                return;
            }

            UnstableRoomOptions options = UnstableRoomOptions.Current;
            if (options == null)
                return;

            options.Height = intValue;
            ValidifyRoomSize(null, options);

            UnstableRoomOptions.Current = options;
        }

        private bool ValidifyRoomSize(string value = null, UnstableRoomOptions options = null)
        {
            if (value == null)
                value = RoomMapStringInput.text;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            if (options == null)
                options = UnstableRoomOptions.Current;

            bool areEqual = value.Length == (options.Width * options.Height);
            RoomMapStringLabel.color = areEqual ? Color.white : Color.red;
            StartGameButton.interactable = areEqual;
            return areEqual;
        }

        public void RoomMapStringChanged()
        {
            string value = RoomMapStringInput.text;
            if (!PhotonNetwork.IsMasterClient || string.IsNullOrWhiteSpace(value))
                return;

            UnstableRoomOptions options = UnstableRoomOptions.Current;
            if (options == null)
                return;

            ValidifyRoomSize(value, options);

            options.MapString = value;

            UnstableRoomOptions.Current = options;
        }

        public void RoomGamemodeChanged()
        {
            int value = RoomGamemodeDropdown.value;
            if (!PhotonNetwork.IsMasterClient || value < 0 || value > 3)
                return;

            UnstableRoomOptions options = UnstableRoomOptions.Current;
            if (options == null)
                return;

            options.Gamemode = (Gamemode)value;

            UnstableRoomOptions.Current = options;
        }

        public void RoomDebrisChanged()
        {
            int value = RoomDebrisDropdown.value;
            if (!PhotonNetwork.IsMasterClient || value < 0 || value > 1)
                return;

            UnstableRoomOptions options = UnstableRoomOptions.Current;
            if (options == null)
                return;

            options.Debris = value == 0;

            UnstableRoomOptions.Current = options;
        }

        public void RoomDroppersChanged()
        {
            int value = RoomDroppersDropdown.value;
            if (!PhotonNetwork.IsMasterClient || value < 0 || value > 1)
                return;

            UnstableRoomOptions options = UnstableRoomOptions.Current;
            if (options == null)
                return;

            options.Droppers = value == 0;

            UnstableRoomOptions.Current = options;
        }

        public void RoomMaxPlayersChanged()
        {
            string value = RoomMaxPlayersInput.text;
            if (!PhotonNetwork.IsMasterClient || string.IsNullOrWhiteSpace(value) || !byte.TryParse(value, out byte byteValue))
                return;
            if (byteValue < 0)
            {
                RoomMaxPlayers.text = "0";
                return;
            }

            PhotonNetwork.CurrentRoom.MaxPlayers = byteValue;
        }

        public void RoomAllowRandomsChanged()
        {
            int value = RoomAllowRandomsDropdown.value;
            if (!PhotonNetwork.IsMasterClient || value < 0 || value > 1)
                return;

            PhotonNetwork.CurrentRoom.IsVisible = value == 0;
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
            UpdateRoomUI();
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
                    result += Random.Range(1, 10).ToString();
            }
            return result;
        }
    }
}
