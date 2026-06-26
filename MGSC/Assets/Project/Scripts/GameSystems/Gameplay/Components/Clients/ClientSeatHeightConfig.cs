using System.Collections.Generic;
using UnityEngine;

namespace Clients
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Client Seat Height Config")]

    public class ClientSeatHeightConfig : ScriptableObject
    {
        public List<ClientHeightByType> ClientHeightByTypeList;
    }
}