using Clients;
using UnityEngine;

namespace Features
{
    /// <summary>
    /// ClientHistory is a log of all clients past and present
    /// </summary>
    public class ClientHistory : Feature
    {
        public ClientHistory(IFeatureCollection owner) : base(owner)
        {
        }

        public void AddClient(Client client)
        {

        }

        private ClientData MakeClientDataFor(Client client)
        {
            return null;
        }
        public ClientData GetClientDataFor(Client client)
        {
            return null;
        }

    }
}