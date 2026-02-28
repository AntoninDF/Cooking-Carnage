using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Features
{
    public class FeatureScannerByRaycast : FeatureScanner
    {
        #region Properties

        public Vector3 Origin { get; set; }
        public Vector3 Direction { get; set; }
        public float Distance { get; set; }
        public bool SortByDistance { get; set; } = true;
        public Vector3 LastScanHitPosition { get; set; }
        /// <summary>
        /// If true, the raycast doesn't stop until the distance is reached. Otherwise, it stops after the first object
        /// </summary>
        public bool ScanAll { get; set; }



        #endregion



        #region Methods

        public FeatureScannerByRaycast() => Set(Vector3.zero, Vector3.up);
        public FeatureScannerByRaycast(Vector3 start, Vector3 end) => Set(start, end);
        public FeatureScannerByRaycast(Vector3 position, Vector3 direction, float distance) => Set(position, direction, distance);

        public void Set(Vector3 start, Vector3 end)
        {
            Origin = start;
            Direction = Vector3.Normalize(end - start);
            Distance = Vector3.Distance(start, end);
        }
        public void Set(Vector3 position, Vector3 direction, float distance)
        {
            Origin = position;
            Direction = direction;
            Distance = distance;
        }

        public override List<FeatureComponent> Get()
        {
            List<FeatureComponent> result = new();
            RaycastHit[] results;

            if (ScanAll) results = Physics.RaycastAll(Origin, Direction, Distance);
            else
            {
                if (Physics.Raycast(Origin, Direction, out var hit, Distance))
                {
                    results = new RaycastHit[1] { hit };
                }
                else
                    results = new RaycastHit[0];
            }

            if (SortByDistance)
            {
                results.OrderBy(hit => hit.distance);
            }
            float minDistance = float.MaxValue;

            foreach (RaycastHit hit in results)
            {
                var go = hit.transform.gameObject;
                if (go == null) continue;

                if (hit.distance < minDistance)
                {
                    LastScanHitPosition = hit.point;
                    minDistance = hit.distance;
                }

                foreach (var comp in go.GetComponents<MonoBehaviour>())
                {
                    if (comp is IFeatureCollection collection)
                    {
                        result.AddRange(collection.Collection.AllFeatures);
                    }
                }
            }

            return result;
        }
        #endregion
    }
}