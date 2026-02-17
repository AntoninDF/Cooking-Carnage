using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Cooking;

[ExecuteAlways]
public class ObjectPile : MonoBehaviour
{
    #region Fields
    public int ObjectCount { get => _objectCount; set => _objectCount = value; }
    public List<GameObject> Prefabs { get => _prefabs; set => _prefabs = value; }
    public float Height => CalculateLocalBounds(this.gameObject).size.y;

    [SerializeField] private int _objectCount = 3;
    [SerializeField] private List<GameObject> _prefabs;

    [SerializeField] private float _padding = 0;
    [SerializeField] private float _offsetFromCenter = 0;

    [Space]
    [Range(0, 1)]
    [SerializeField] private float _yawRotationRange;
    [Range(0, 1)]
    [SerializeField] private float _pitchRotationRange;

    [Space]
    [SerializeField] private Vector3 _minScale = Vector3.one;
    [SerializeField] private Vector3 _maxScale = Vector3.one;

    [Header("Other parameters")]
    [SerializeField] private int _seed;
    [SerializeField] private bool _allowChildDeletion = true;
    [SerializeField] private bool _regenerateEverything = false;
    [SerializeField] private int _objectCountWarningThreshold = 500;
    [SerializeField] private int _objectCountErrorThreshold = 1000;

    #endregion


    #region Methods

    public void OnValueChange()
    {
        TryRegenerate();
    }

    public bool TryRegenerate()
    {
        if (_objectCount < 0) return false;
        if (_prefabs == null || _prefabs.Count == 0) return false;


        Regenerate();
        return true;
    }


    private void Regenerate()
    {
        int childsToKeep = Mathf.Min(transform.childCount, _objectCount);
        if (_regenerateEverything)
            childsToKeep = 0;

        for (int child = transform.childCount-1;  child >= childsToKeep; child--)
        {
            if (_allowChildDeletion)
            {
                if (Application.isPlaying)
                    Reference.GetManager<ItemManager>().Despawn(transform.GetChild(child).GetComponent<Item>());
                else
                    DestroyImmediate(transform.GetChild(child).gameObject);
            }
            else
                transform.GetChild(child).gameObject.SetActive(false);
        }
     

        if (transform.childCount >= _objectCountErrorThreshold)
        {
            Debug.LogWarning("[ObjectPile] : More than " + _objectCountErrorThreshold + " objects have been generated on '" 
                + name + "'. This number exceeds the recommended object count.");
            return;
        }

        float totalHeight = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            totalHeight += CalculateLocalBounds(transform.GetChild(i).gameObject).size.y;
            totalHeight += _padding;
        }

        // Setup the random number generator
        System.Random rng = new System.Random(_seed);
        for (int i = 0; i++ < transform.childCount;) rng.Next();

        for (int i = childsToKeep; i < _objectCount; i++)
        {
            int goIndex = rng.Next(0, _prefabs.Count);
            GameObject go = _prefabs[goIndex];

            var prefab = PrefabUtility.InstantiatePrefab(go, this.transform);
        }

        for (int child = childsToKeep; child < transform.childCount; child++)
        {
            GameObject go = transform.GetChild(child).gameObject;
            Bounds bounds = CalculateLocalBounds(go);

            // Position
            go.transform.localPosition = Vector3.up * (totalHeight + bounds.size.y/2 + child * _padding);
            //go.transform.localPosition -= bounds.center;
            double offsetOrientation = rng.NextDouble() * 360;
            Vector3 offset = Quaternion.Euler(0, (float)offsetOrientation, 0) * Vector3.right;
            offset *= (float)rng.NextDouble() * _offsetFromCenter;
            go.transform.localPosition += offset;
            totalHeight += bounds.size.y;

            // Rotation
            float pitch, yaw;
            pitch = (float)rng.NextDouble() *_pitchRotationRange;
            yaw = (float)rng.NextDouble() * _yawRotationRange;
            go.transform.rotation = Quaternion.Euler(pitch*90, yaw*360, 0);

            // Scale
            float x, y, z;
            x = (float)rng.NextDouble() * (_maxScale.x - _minScale.x) + _minScale.x;
            y = (float)rng.NextDouble() * (_maxScale.y - _minScale.y) + _minScale.y;
            z = (float)rng.NextDouble() * (_maxScale.z - _minScale.z) + _minScale.z;
            go.transform.localScale = new Vector3(x, y, z);
        }


        if (transform.childCount > _objectCountWarningThreshold && transform.childCount < _objectCountErrorThreshold)
        {
            Debug.LogWarning("[ObjectPile] : More than " + _objectCountWarningThreshold + " objects have been generated on '"
                + name + "'. This number exceeds the recommended object count.");
        }
    }

    public void RandomizeSeed()
    {
        System.Random rng = new(_seed);
        _seed = rng.Next(0, int.MaxValue);
    }

    private Bounds CalculateLocalBounds(GameObject go)
    {
        if (go == null) return new Bounds();
        var renderers = go.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0) return new Bounds();
        Quaternion currentRotation = go.transform.rotation;
        go.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        Vector3 localCenter = bounds.center - go.transform.position;
        bounds.center = localCenter;
        go.transform.rotation = currentRotation;

        return bounds;
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
            objectPile.TryRegenerate();
        if (GUILayout.Button("Random seed"))
        {
            objectPile.RandomizeSeed();
            objectPile.TryRegenerate();
        }
        GUILayout.EndHorizontal();

    }
}
#endif