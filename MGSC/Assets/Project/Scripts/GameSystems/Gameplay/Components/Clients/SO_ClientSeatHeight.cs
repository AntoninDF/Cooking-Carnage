using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Components.Clients
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Client Seat Heights")]
	public class SO_ClientSeatHeight : ScriptableSingleton<SO_ClientSeatHeight>
	{
		public List<ClientHeightByType> ClientHeightByTypes;
	}
}