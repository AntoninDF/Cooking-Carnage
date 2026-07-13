using System.Collections.Generic;
using UnityEngine.Splines;

namespace Features.Path
{
	public class SplineTrafficManager
	{
		private readonly Dictionary<SplineContainer, List<ClientMovement>> _occupants = new();

		public void Register(ClientMovement movement)
		{
			if (movement.SplineComponent == null) return;
			if (!_occupants.TryGetValue(movement.SplineComponent, out var list))
			{
				list                                 = new List<ClientMovement>();
				_occupants[movement.SplineComponent] = list;
			}
			if (!list.Contains(movement)) list.Add(movement);
		}

		public void Unregister(ClientMovement movement)
		{
			if (movement.SplineComponent == null) return;
			if (_occupants.TryGetValue(movement.SplineComponent, out var list))
				list.Remove(movement);
		}

	
		public bool TryGetNearestAhead(ClientMovement self, out ClientMovement nearest, out float tGap)
		{
			nearest = null;
			tGap    = float.MaxValue;

			if (!_occupants.TryGetValue(self.SplineComponent, out var list)) return false;

			foreach (var other in list)
			{
				if (other == self || !other.IsOnSpline) continue;

				float diff = other.SplinePosition - self.SplinePosition;
				if (diff <= 0f) continue; 

				if (diff < tGap)
				{
					tGap    = diff;
					nearest = other;
				}
			}

			return nearest != null;
		}
	}
}