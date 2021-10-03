using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unstable
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public GameObject PlayerPrefab;
        public GameObject PlatformsPrefab;
        public GameObject DebrisPrefab;

        public void Start()
        {
            if (NetworkPlayer.LocalPlayerInstance != null)
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            else
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(0.0f, 30.0f, 0.0f), Quaternion.identity, 0);
            }

            if (!PhotonNetwork.IsMasterClient)
                return;

            UnstableRoomOptions options = UnstableRoomOptions.Current;

            GameObject platforms = Instantiate(PlatformsPrefab, Vector3.zero, Quaternion.identity);
            PlatformGenerator platformGenerator = platforms.GetComponent<PlatformGenerator>();

            if (options.Debris)
            {
                GameObject debris = Instantiate(DebrisPrefab, Vector3.zero, Quaternion.identity);
                DebrisController debrisController = debris.GetComponent<DebrisController>();
                debrisController.PlatformGenerator = platformGenerator;
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

        // public void LoadArena()
        // {
        //     if (!PhotonNetwork.IsMasterClient)
        //     {
        //         Debug.LogError("PhotonNetwork: Trying to Load a level but we are not the master Client");
        //         return;
        //     }
        //     Debug.Log("PhotonNetwork: Loading Arena Level");
        //     PhotonNetwork.LoadLevel("Arena");
        // }

        // public override void OnPlayerEnteredRoom(Player other)
        // {
        //     Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);

        //     if (!PhotonNetwork.IsMasterClient)
        //         return;

        //     Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
        //     LoadArena();
        // }


        // public override void OnPlayerLeftRoom(Player other)
        // {
        //     Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName);

        //     if (!PhotonNetwork.IsMasterClient)
        //         return;

        //     Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
        //     LoadArena();
        // }
    }
}
