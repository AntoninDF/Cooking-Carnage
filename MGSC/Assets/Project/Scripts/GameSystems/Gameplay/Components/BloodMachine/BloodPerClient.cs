using System;
using Clients;

namespace Project.Scripts.BloodMachine
{
	[Serializable]
	public class BloodPerClient
	{
		public UnityEngine.Vector2      bloodGainedMinMax;
		public EClientType clientType;
	}
}