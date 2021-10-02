using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Unstable
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        private readonly string gameVersion = "0.1.0";
        private bool isConnecting = false;

        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        public byte MaxPlayersPerRoom = 4;

        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject ControlPanel;

        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        public GameObject ProgressLabel;

        public void Awake()
        {
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public void Start()
        {
            ProgressLabel.SetActive(false);
            ControlPanel.SetActive(true);
        }

        public void Connect()
        {
            ProgressLabel.SetActive(true);
            ControlPanel.SetActive(false);

            if (PhotonNetwork.IsConnected)
                PhotonNetwork.JoinRandomRoom();
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            if (isConnecting)
            {
                PhotonNetwork.JoinRandomRoom();
                isConnecting = false;
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);

            isConnecting = false;
            ProgressLabel.SetActive(false);
            ControlPanel.SetActive(true);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions
            {
                MaxPlayers = MaxPlayersPerRoom
            });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("We load the 'Arena'.");
                PhotonNetwork.LoadLevel("Arena");
            }
        }
    }
}
