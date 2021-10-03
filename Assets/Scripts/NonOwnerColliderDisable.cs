using System.Linq;
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

            Rigidbody[] rigidBodies = GetComponents<Rigidbody>()
                .Concat(GetComponentsInChildren<Rigidbody>())
                .ToArray();
            foreach (Rigidbody rigidBody in rigidBodies)
                rigidBody.isKinematic = true;
        }
    }
}
