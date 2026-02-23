

using System.Linq;
using UnityEngine;

public class BoundsFinder : FeatureComponent
{
    public Transform transform {  get; set; }
    public Bounds LastComputedBounds { get; set; }
    public BoundsFinder(IFeatureCollection owner, Transform transform) : base(owner)
    {
        this.transform = transform;
    }

    public Bounds Get()
    {
        var renderers = transform.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0)
        {
            LastComputedBounds = new Bounds();
            return LastComputedBounds;
        }

        Bounds bounds = renderers[0].bounds;

        foreach (var renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        LastComputedBounds = bounds;
        return bounds;
    }

    public Bounds Get(out Bounds bounds)
    {
        bounds = Get();
        return bounds;
    }

    public float FindWorldLowestPoint()
    {
        var bounds = Get();
        float result = bounds.min.y;
        return result;
    }
    public float FindWorldHighestPoint()
    {
        var bounds = Get();
        float result = bounds.max.y;
        return result;
    }
    public float FindLocalLowestPoint()
    {
        var bounds = Get();
        float result = bounds.min.y - transform.position.y;
        return result;
    }
    public float FindLocalHighestPoint()
    {
        var bounds = Get();
        float result = bounds.max.y - transform.position.y;
        return result;
    }
}