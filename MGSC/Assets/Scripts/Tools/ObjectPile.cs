using Cooking;
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ObjectPile : MonoBehaviour
{
    #region Fields
    public int TargetObjectCount { get => _targetObjectCount; set => _targetObjectCount = value; }
    public List<GameObject> Objects { get; set; } = new();
    public List<GameObject> Prefabs { get => _prefabs; set => _prefabs = value; }
    public float Height => CalculateLocalBounds(this.gameObject).size.y;

    public Action<GameObject> OnObjectAdded { get; set; }
    public Action<GameObject> OnObjectRemoved { get; set; }
    public Action OnObjectDestroyed { get; set; }


    [SerializeField] protected int _targetObjectCount = 3;
    [SerializeField] protected List<GameObject> _prefabs;

    [SerializeField] protected float _offsetFromCenter = 0;
    [SerializeField] protected float _padding = 0;

    [Space]
    [Range(0, 1)]
    [SerializeField] protected float _yawRotationRange;
    [Range(0, 1)]
    [SerializeField] protected float _pitchRotationRange;

    [Space]
    [SerializeField] protected Vector3 _minScale = Vector3.one;
    [SerializeField] protected Vector3 _maxScale = Vector3.one;

    [Header("Other parameters")]
    [SerializeField] protected int _seed;
    [SerializeField] protected bool _allowChildDeletion = true;
    [Tooltip("Check this option to make sure everything is regenerated each time any change is made to the parameters")]
    [SerializeField] protected bool _regenerateEverything = false;
    [SerializeField] protected int _objectCountWarningThreshold = 500;
    [SerializeField] protected int _objectCountErrorThreshold = 1000;

    protected System.Random Rng = new(0);

    #endregion


    #region Methods

    public virtual void OnValueChange()
    {
        TryRegenerate();
    }
    public virtual bool TryRegenerate()
    {
        if (_targetObjectCount < 0) return false;
        if (_prefabs == null || _prefabs.Count == 0) return false;


        Regenerate();
        return true;
    }
    protected virtual void CleanUp()
    {
        CleanNullRefs();
        CleanHierarchy();
    }

    protected virtual void CleanNullRefs()
    {
        Objects.RemoveAll(x => x == null);
    }
    protected virtual void CleanHierarchy()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            if (!Objects.Contains(obj))
            {

#if UNITY_EDITOR
                DestroyImmediate(obj);
#else
            Destroy(go);
#endif
            }
        }
    }

    public virtual bool TryGet(int index, out GameObject go)
    {
        go = null;
        if (index < 0) return false;
        if (index >= Objects.Count) return false;
        go = Objects[index];
        return true;
    }
    public virtual void RemoveAll()
    {
        for (int i = Objects.Count - 1; i >= 0; i--)
        {
            TryRemove(i);
        }
        Objects.Clear();
    }
    public virtual void DisposeAll()
    {
        for (int i = Objects.Count - 1; i >= 0; i--)
        {
            TryDispose(i);
        }
        Objects.Clear();
    }
    public virtual bool TryRemove(int index)
    {
        TryGet(index, out GameObject go);
        return TryRemove(go);
    }

    protected virtual void Remove(GameObject go)
    {
        Objects.Remove(go);
        if (go.transform.parent == this)
            go.transform.parent = null;
    }

    public virtual bool TryRemove(GameObject go)
    {
        if (!CanRemove(go)) return false;
        Remove(go);
        return true;
    }

    public virtual bool TryAdd(Vector3 position = default, Quaternion rotation = default, Vector3 scale = default) => TryAdd(SelectPrefab(), position, rotation, scale);
    public virtual bool TryAdd(GameObject reference, Vector3 position = default, Quaternion rotation = default, Vector3 scale = default)
    {
        if (CanAdd(reference))
        {
            Add(reference, position, rotation, scale);
            return true;
        }
        return false;
    }

    public virtual bool TryDispose(int index)
    {
        TryGet(index, out GameObject go);
        return TryDispose(go);
    }
    public virtual bool TryDispose(GameObject go)
    {
        if (TryRemove(go))
        {
#if UNITY_EDITOR
            DestroyImmediate(go);
#else
            Destroy(go);
#endif
            return true;
        }
        return false;
    }

    protected virtual GameObject SelectPrefab()
    {
        return SelectRandomPrefab();
    }

    protected virtual GameObject SelectRandomPrefab()
    {
        int goIndex = Rng.Next(0, _prefabs.Count);
        return _prefabs[goIndex];
    }

    protected virtual void Add(GameObject reference, Vector3 position=default, Quaternion rotation=default, Vector3 scale=default)
    {
        GameObject go = Instantiate(reference, this.transform);

        if (rotation == default)
            RotateByParameters(go);
        else
            RotateByParameters(go);

        if (scale == default)
            ScaleByParameters(go);
        else
            go.transform.localScale = scale;

        if (position == default)
        {
            OffsetByParameters(go);
            PutOnTop(go);
        }
        else
            go.transform.localPosition = position;
        
        Objects.Add(go);
    }

    protected virtual void PutOnTop(GameObject go)
    {
        var p = go.transform.localPosition;
        go.transform.localPosition = new Vector3(p.x, Height + CalculateLocalBounds(go).size.y/2 + _padding, p.z);
    }
    protected  virtual void OffsetByParameters(GameObject go)
    {
        double offsetOrientation = Rng.NextDouble() * 360;
        Vector3 offset = Quaternion.Euler(0, (float)offsetOrientation, 0) * Vector3.right;
        offset *= (float)Rng.NextDouble() * _offsetFromCenter;
        go.transform.localPosition += offset;
    }
    protected virtual void RotateByParameters(GameObject go)
    {
        float pitch, yaw;
        pitch = (float)Rng.NextDouble() * _pitchRotationRange;
        yaw = (float)Rng.NextDouble() * _yawRotationRange;
        go.transform.rotation = Quaternion.Euler(pitch * 90, yaw * 360, 0);
    }
    protected virtual void ScaleByParameters(GameObject go)
    {
        float x, y, z;
        x = (float)Rng.NextDouble() * (_maxScale.x - _minScale.x) + _minScale.x;
        y = (float)Rng.NextDouble() * (_maxScale.y - _minScale.y) + _minScale.y;
        z = (float)Rng.NextDouble() * (_maxScale.z - _minScale.z) + _minScale.z;
        go.transform.localScale = new Vector3(x, y, z);
    }

    protected virtual bool CanAdd(GameObject reference)
    {
        return
            reference != null &&
            !Objects.Contains(reference) &&
            Objects.Count < _objectCountWarningThreshold;
    }
    protected virtual bool CanRemove(GameObject reference)
    {
        return
            reference != null &&
            Objects.Contains(reference);
    }

    protected virtual void Regenerate()
    {
        TargetObjectCount = Mathf.Max(TargetObjectCount, 0);
        if (TargetObjectCount >= _objectCountErrorThreshold)
        {
            Debug.LogWarning("[ObjectPile] : More than " + _objectCountErrorThreshold + " objects tried to be generated on '"
                + name + "'. This number exceeds the recommended object count. Generation was aborted");
            return;
        }

        this.Rng = new System.Random(_seed);

        if (_regenerateEverything)
        {
            DisposeAll();
        }
        CleanUp();

        int delta = _targetObjectCount - Objects.Count;
        if (delta == 0) return; // We have exactly the requested amount of objects, no need to regenerate;

        if (delta > 0)
        {
            for (int  i = 0; i < delta; i++)
            {
                TryAdd();
            }
        }
        if (delta < 0)
        {
            for (int  i = 0; i > delta; i--)
            {
                if (_allowChildDeletion)
                    TryDispose(Objects.Count-1);
                else
                    TryRemove(Objects.Count-1);
            }
        }

        if (Objects.Count > _objectCountWarningThreshold && Objects.Count < _objectCountErrorThreshold)
        {
            Debug.LogWarning("[ObjectPile] : More than " + _objectCountWarningThreshold + " objects have been generated on '"
                + name + "'. This number exceeds the recommended object count.");
        }
    }

    public void RandomizeSeed()
    {
        _seed = Rng.Next(0, int.MaxValue);
        Rng = new(_seed);
    }

    protected Bounds CalculateLocalBounds(GameObject go)
    {
        if (go == null) return new Bounds();
        var renderers = go.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0) return new Bounds();
        Quaternion currentRotation = go.transform.rotation;
        go.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Bounds bounds = renderers[0].bounds;
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                bounds.Encapsulate(renderer.bounds);
        }
        Vector3 localCenter = bounds.center - go.transform.position;
        bounds.center = localCenter;
        go.transform.rotation = currentRotation;

        return bounds;
    }
    protected Bounds[] GetChildBounds(GameObject go)
    {
        if (go == null) return new Bounds[0];
        var renderers = go.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0) return new Bounds[0];
        List<Bounds> bounds = new List<Bounds>();
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.enabled && renderer.gameObject.activeInHierarchy)
            {
                var bound = renderer.bounds;
                bounds.Add(bound);
            }
        }

        return bounds.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f);
        foreach(var bound in GetChildBounds(gameObject))
        {
            Gizmos.DrawWireCube(bound.center, bound.size);
        }

        Gizmos.color = Color.blue;
        var bounds = CalculateLocalBounds(gameObject);
        Gizmos.DrawWireCube(bounds.center + transform.position, bounds.size);
    }
    #endregion
}


#if UNITY_EDITOR

[CustomEditor(typeof(ObjectPile))]
public class ObjectPileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ObjectPile objectPile = (ObjectPile)target;
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            objectPile.OnValueChange();
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Regenerate"))
        {
            objectPile.DisposeAll();
            objectPile.TryRegenerate();
        }
        if (GUILayout.Button("Random seed"))
        {
            objectPile.RandomizeSeed();
            objectPile.TryRegenerate();
        }
        GUILayout.EndHorizontal();

    }
}
#endif