using Photon.Pun;
using UnityEngine;

namespace Unstable
{
    public class NetworkPlayer : MonoBehaviourPun
    {
        public static GameObject LocalPlayerInstance;

        public GameObject Camera;

        public void Awake()
        {
            if (photonView.IsMine)
                LocalPlayerInstance = gameObject;

            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            Camera.SetActive(photonView.IsMine);
        }
    }
}
