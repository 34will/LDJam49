using Photon.Pun;
using UnityEngine;

namespace Unstable
{
    public class NonOwnerColliderDisable : MonoBehaviour
    {
        public void Start()
        {
            if (PhotonNetwork.IsMasterClient)
                return;

            Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rigidBody in rigidBodies)
                rigidBody.isKinematic = true;
        }
    }
}
