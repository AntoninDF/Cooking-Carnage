using System;
using Clients;
using UnityEngine;

namespace Clients
{
	[Serializable]
	public class ClientHeightByType
	{
		[Tooltip("Le type du client")]
		public EClientType ClientType;
		[Tooltip("La différence Y du siège par rapport à sa hauteur par défaut")]
		public float       SeatHeight;
	}
}