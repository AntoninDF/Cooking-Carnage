using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FeatureScannerByRaycast : FeatureScanner
{
    #region Properties

    public Vector3 Origin { get; set; }
    public Vector3 Direction { get; set; }
    public float Distance { get; set; }
    public bool SortByDistance { get; set; } = true;

    #endregion



    #region Methods

    public FeatureScannerByRaycast() => Set(Vector3.zero, Vector3.up);
    public FeatureScannerByRaycast(Vector3 start, Vector3 end) => Set(start, end);
    public FeatureScannerByRaycast(Vector3 position, Vector3 direction, float distance) => Set(position, direction, distance);

    public void Set(Vector3 start, Vector3 end)
    {
        Origin = start;
        Direction = Vector3.Normalize(end - start);
        Distance = Vector3.Distance(start,  end);
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
        
        results = Physics.RaycastAll(Origin, Direction, Distance);

        if (SortByDistance) results.OrderBy(hit => hit.distance);

        foreach(RaycastHit hit in results)
        {
            var go = hit.transform.gameObject;
            if (go == null) continue;

            foreach (var comp in go.GetComponents<MonoBehaviour>())
            {
                if (comp is IFeatureCollection collection)
                {
                    result = result.Concat(collection.Collection.AllFeatures).ToList();
                }
            }
        }

        return result;
    }
    #endregion
}
