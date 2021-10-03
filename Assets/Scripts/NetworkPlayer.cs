using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Unstable
{
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkPlayer : MonoBehaviourPun
    {
        public static GameObject LocalPlayerInstance;

        private Rigidbody rigidBody;

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
        }

        public void Update()
        {
            if (!photonView.IsMine)
                return;

            if (transform.position.y > DeathHeight || IsDead)
                return;

            rigidBody.position = new Vector3(0.0f, 30.0f, 0.0f);
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

            Destroy(Model);

            foreach (MonoBehaviour behaviour in ToDisableOnDeath)
                behaviour.enabled = false;
            
            IsDead = true;
        }
    }
}
