using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Clients
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Client Seat Config")]

    public class ClientSeatConfig : ScriptableSingleton<ClientSeatConfig>
    {
        public List<ClientHeightByType> ClientHeightByTypeList;
    }
}