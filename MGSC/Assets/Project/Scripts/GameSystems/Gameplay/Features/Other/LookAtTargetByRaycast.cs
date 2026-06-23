using UnityEngine;

namespace Features
{
    public class LookAtTargetByRaycast : F_LookAtTarget
    {

        public Collider Collider { get; set; }
        /// <summary>
        /// If not passthrough, it will be considered as looked at only when it is directly looked at. Otherwise, it is looked at if it is in line of sight, even if obstruded.
        /// </summary>
        public bool Passthrough { get; set; }

        public LookAtTargetByRaycast(IFeatureCollection owner, Transform transform) : base(owner, transform)
        {
            Collider = transform.GetComponent<Collider>();
        }
        public LookAtTargetByRaycast(IFeatureCollection owner, Transform transform, Collider collider) : base(owner, transform)
        {
            Collider = collider;
        }

        public override bool CheckIfLookedAt(Vector3 position, Vector3 direction, float distance)
        {
            LookPosition = transform.position;
            if (Collider == null)
                return false;


            if (Passthrough)
            {
                RaycastHit[] hits = Physics.RaycastAll(position, direction, distance);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider == Collider)
                    {
                        LookPosition = hit.point;
                        return true;
                    }
                }
                return false;
            }
            else
            {
                if (Physics.Raycast(position, direction, out RaycastHit hit, distance))
                {
                    LookPosition = hit.point;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}