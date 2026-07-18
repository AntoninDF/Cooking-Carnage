using System.Collections.Generic;
using UnityEngine;
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

		public bool TryGetNearestAhead(ClientMovement self, out ClientMovement nearest, out float worldGap)
		{
			nearest  = null;
			worldGap = float.MaxValue;

			if (!_occupants.TryGetValue(self.SplineComponent, out var list)) return false;

			bool    isClosed = self.Spline.Closed;
			Vector3 selfPos  = self.GetSplinePos(self.SplinePosition);

			for (int i = list.Count - 1; i >= 0; i--)
			{
				var other = list[i];
				
				if (other == null || other.ControlledTransform == null)
				{
					list.RemoveAt(i);
					continue;
				}

				if (other == self || !other.IsOnSpline) continue;

				float diff = other.SplinePosition - self.SplinePosition;
				
				if (diff <= 0f)
				{
					if (!isClosed) continue;
					diff += 1f;
					if (diff <= 0f) continue;
				}
				
				Vector3 otherPos = other.GetSplinePos(other.SplinePosition);
				float   dist     = Vector3.Distance(selfPos, otherPos);

				if (dist < worldGap)
				{
					worldGap = dist;
					nearest  = other;
				}
			}

			return nearest != null;
		}
		
		public void ClearAll() 
		{
			_occupants.Clear();
		}
	}
}