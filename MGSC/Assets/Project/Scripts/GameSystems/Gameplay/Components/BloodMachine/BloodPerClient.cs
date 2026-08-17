using System;
using Clients;

namespace Project.Scripts.BloodMachine
{
	[Serializable]
	public class BloodPerClient
	{
		public float       bloodGained;
		public EClientType clientType;
	}
}