using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unstable
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public GameObject PlayerPrefab;

        public void Start()
        {
            if (NetworkPlayer.LocalPlayerInstance != null)
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            else
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(0.0f, 5.0f, 0.0f), Quaternion.identity, 0);
            }
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("Launcher");
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork: Trying to Load a level but we are not the master Client");
                return;
            }
            Debug.Log("PhotonNetwork: Loading Arena Level");
            PhotonNetwork.LoadLevel("Arena");
        }

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);

            if (!PhotonNetwork.IsMasterClient)
                return;

            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
            LoadArena();
        }


        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName);

            if (!PhotonNetwork.IsMasterClient)
                return;

            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
            LoadArena();
        }
    }
}
