using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Unstable
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(UIManager))]
    public class NetworkPlayer : MonoBehaviourPun
    {
        public static GameObject LocalPlayerInstance;

        private Rigidbody rigidBody;

        private UIManager uiManager;

        public GameObject Camera;

        public GameObject Model;

        public bool IsDead = false;

        public float DeathHeight = 2.0f;

        public List<MonoBehaviour> ToDisableOnDeath;

        public void Awake()
        {
            if (photonView.IsMine)
                LocalPlayerInstance = gameObject;

            DontDestroyOnLoad(gameObject);
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

        [PunRPC]
        public void Die()
        {
            rigidBody.isKinematic = true;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

            Destroy(Model);

            IsDead = true;

            uiManager.OnDeath();
        }

        [PunRPC]
        public void ResetPlayerPosition()
        {
            rigidBody.position = new Vector3(0.0f, 30.0f, 0.0f);
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

            foreach (MonoBehaviour behaviour in ToDisableOnDeath)
                behaviour.enabled = false;

            photonView.RPC("Die", RpcTarget.Others);
            Die();
        }
    }
}
