using System;
using System.Collections.Generic;
using CMF;
using Photon.Pun;
using UnityEngine;

namespace Unstable
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(UIManager))]
    public class NetworkPlayer : MonoBehaviourPun
    {
        public static NetworkPlayer Instance;

        public Rigidbody rigidBody;

        private UIManager uiManager;

        public AdvancedWalkerController WalkerController;

        public GameObject Camera;

        public GameObject Model;

        public bool IsDead = false;

        public float DeathHeight = 2.0f;

        public List<MonoBehaviour> ToDisableOnDeath;

        public Vector3 MapCenter;

        public void Awake()
        {
            if (photonView.IsMine)
                Instance = this;

            DontDestroyOnLoad(gameObject);

            if (!PhotonNetwork.IsMasterClient)
                return;

            GameObject.Find("/GameManager").GetComponent<NetworkManager>().RegisterPlayer(this);
        }

        public void Start()
        {
            Camera.SetActive(photonView.IsMine);
            rigidBody = GetComponent<Rigidbody>();

            if (!photonView.IsMine)
                return;

            uiManager = GetComponent<UIManager>();
            uiManager.UI.DeathMessage.OnMessageShown += OnPlayerDeathMessageShown;
        }

        public void Destroy()
        {
            uiManager.UI.DeathMessage.OnMessageShown -= OnPlayerDeathMessageShown;
        }

        private void SetState(bool dead)
        {
            if (IsDead == dead)
                return;

            foreach (MonoBehaviour behaviour in ToDisableOnDeath)
                behaviour.enabled = !dead;

            rigidBody.isKinematic = dead;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            WalkerController.SetMomentum(Vector3.zero);

            Model.SetActive(!dead);

            IsDead = dead;
        }

        [PunRPC]
        private void DoRevive()
        {
            SetState(false);

            if (!photonView.IsMine)
                return;
            uiManager.OnRevive();
        }

        public void Revive()
        {
            if (!IsDead)
                return;

            photonView.RPC("DoRevive", RpcTarget.Others);
            DoRevive();
        }

        [PunRPC]
        private void Die()
        {
            SetState(true);

            if (!photonView.IsMine)
                return;
            uiManager.OnDeath();
        }

        [PunRPC]
        private void DoWin()
        {
            SetState(true);
            ResetPlayerPosition();

            if (!photonView.IsMine)
                return;
            uiManager.Spectate();
        }

        [PunRPC]
        public void Win()
        {
            photonView.RPC("DoWin", RpcTarget.Others);
            DoWin();
        }

        [PunRPC]
        private void ResetPlayerPosition()
        {
            rigidBody.position = MapCenter;
        }

        private void OnPlayerDeathMessageShown(object sender, EventArgs e)
        {
            photonView.RPC("ResetPlayerPosition", RpcTarget.Others);
            ResetPlayerPosition();
        }

        public void Update()
        {
            if (!photonView.IsMine)
                return;

            if (transform.position.y > DeathHeight || IsDead)
                return;

            photonView.RPC("Die", RpcTarget.Others);
            Die();
        }
    }
}
