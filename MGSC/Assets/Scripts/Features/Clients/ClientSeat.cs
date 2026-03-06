using GameSystem;
using UnityEngine;

namespace Features
{
    public class ClientSeat : Feature
    {
        public Transform transform;

        public ClientManager ClientManager => Reference.GetManager<ClientManager>();
        public ClientSeat(IFeatureCollection owner, Transform seatTransform) : base(owner)
        {
            if (seatTransform == null)
                Debug.LogWarning("ClientSeat transform is null");
            transform = seatTransform;

            ClientManager.Register(this);
        }
    }
}