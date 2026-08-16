#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using UnityEngine;
using Unity.Profiling;
using NaughtyAttributes;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
[ExecuteAlways]
public class MeshPile : MonoBehaviour
{

    public Mesh mesh;
    public Material material;
    [Min(0)]
    public int count = 5;
    [Range(0f, 1f), Tooltip("Applying random rotation takes longer to compute. Don't use it with high mesh counts.")]
    public float randomRotation;
    [Range(0f, 1f), Tooltip("Applying random scale takes longer to compute. Don't use it with high mesh counts.")]
    public float randomScale;
    [Tooltip("Jitter is fine, use it as you wish")]
    public float jitter;

    /// <summary>Mesh Filter</summary>
    private MeshFilter mf;
    /// <summary>Mesh Collider</summary>
    private MeshCollider mc;
    /// <summary>Mesh Renderer</summary>
    private MeshRenderer mr;
    private int _defaultLayer;
    
    public Collider colliderPile;
    public Action OnPlateCountChanged;

    // Profiling
    ProfilerMarker MeshPile_Regenerate = new("MeshPile/Regenerate()");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TryGetComponent(out mf);
        TryGetComponent(out mc);
        TryGetComponent(out mr);
        TryAssignDefaultMaterial();
        TryAssignDefaultMesh();
        RegenerateIfNull();
        
        _defaultLayer = gameObject.layer;
        OnPlateCountChanged += CheckCollider;

    }
    void Reset()
    {
        TryAssignDefaultMaterial();
        TryAssignDefaultMesh();
        Regenerate_Editor();
    }

    void CheckCollider()
    {
        if (colliderPile != null)
            colliderPile.enabled = count != 0;

    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        AppDomain.CurrentDomain.AssemblyLoad += RegenerateIfNull_Editor;
        EditorApplication.playModeStateChanged += RegenerateIfNull_Editor;
    }
    private void OnDisable()
    {
        AppDomain.CurrentDomain.AssemblyLoad -= RegenerateIfNull_Editor;
        EditorApplication.playModeStateChanged -= RegenerateIfNull_Editor;
    }

    void RegenerateIfNull_Editor(PlayModeStateChange temp) => RegenerateIfNull();
    void RegenerateIfNull_Editor(object temp, object temp2) => RegenerateIfNull();
#endif
    public void RegenerateIfNull()
    {
        TryGetComponent(out mf);
        if (mf.sharedMesh == null)
            Regenerate();
    }
    private void TryAssignDefaultMesh()
    {
        if (mesh != null) return;
        mesh = MeshPileUtility.Cube(Vector3.one * 0.1f);
    }

    private void TryAssignDefaultMaterial()
    {
        TryGetComponent(out mr);
        if (mr.sharedMaterial != null) return;
        mr.sharedMaterial = new Material(Shader.Find("HDRP/Lit"));
    }

    // void OnValidate() => Regenerate_Editor();

    public void Regenerate(bool applyRandomPresets=true)
    {
        MeshPile_Regenerate.Begin();
        if (mr == null) TryGetComponent(out mr);
        if (mesh == null) return;
        Mesh newMesh = mf.sharedMesh;
        if (applyRandomPresets)
            MeshPileUtility.MakePile(ref newMesh, mesh, count, mesh.bounds.size.y, randomRotation, randomScale, jitter);
        else
            MeshPileUtility.MakePile(ref newMesh, mesh, count, mesh.bounds.size.y, 0, 0, 0);
        mf.sharedMesh = newMesh;
        if (mc != null)
            mc.sharedMesh = newMesh;
        if (material != null)
            mr.sharedMaterial = material;
        MeshPile_Regenerate.End();

        OnPlateCountChanged?.Invoke();
    }
    [ContextMenu("Regenerate mesh")]
    [Button("Regenerate mesh")]
    internal void Regenerate_Editor()
    {
        TryGetComponent(out mf);
        TryGetComponent(out mc);
        TryGetComponent(out mr);
        Regenerate();
    }

    public void RemoveOneElement()
    {
        count = count == 0 ? 0 : count-1;
        Regenerate();
    }
    public void AddOneElement()
    {
        count++;
        Regenerate();
    }
    public void Clear()
    {
        count = 0;
        Regenerate();
    }
    
    public void SetOutline(bool active)
    {
        gameObject.layer = active ? LayerMask.NameToLayer("Outlined") : _defaultLayer;
    }

    public void OnDestroy()
    {
        OnPlateCountChanged -= CheckCollider;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(MeshPile))]
public class MeshPileEditor : Editor
{
    void OnSceneGUI()
    {
        Handles.color = Color.cyan;
        MeshPile myObj = (MeshPile)target;
        //myObj.RegenerateIfNull();
        if (myObj.GetComponent<MeshFilter>().sharedMesh == null)
        {
            Debug.Log("Mesh for " + myObj.gameObject + " is null");
            return;
        }
        var bounds = myObj.GetComponent<MeshFilter>().sharedMesh.bounds;
        Handles.DrawWireCube(myObj.transform.position + bounds.center, bounds.size);
        Undo.RecordObject(myObj, "Modify pile object count");
        int prevCount = myObj.count;
        myObj.count = (int)Handles.ScaleValueHandle(
            myObj.count,
            myObj.transform.position + bounds.center + myObj.transform.up * bounds.size.y/2f,
            myObj.transform.rotation * Quaternion.Euler(-90,0,0),
            0.5f,
            Handles.ConeHandleCap,
            1
            );
        if (myObj.count != prevCount)
            myObj.Regenerate_Editor();
    }
}
#endif