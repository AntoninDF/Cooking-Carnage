using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class MeshPile : MonoBehaviour
{

    public Mesh mesh;
    [Min(0)]
    public int count = 5;
    [Range(0f, 1f), Tooltip("Applying random rotation takes longer to compute. Don't use it with high mesh counts.")]
    public float randomRotation;
    [Range(0f, 1f), Tooltip("Applying random scale takes longer to compute. Don't use it with high mesh counts.")]
    public float randomScale;
    [Tooltip("Jitter is fine, use it as you wish")]
    public float jitter;

    private MeshFilter mf;
    private MeshCollider mc;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TryGetComponent(out mf);
        TryGetComponent(out mc);
    }
    void Reset()
    {
        TryGetComponent(out MeshRenderer mr);
        mr.material = new Material(Shader.Find("HDRP/Lit"));
        mesh = MeshPileUtility.Cube(Vector3.one * 0.1f);
        Regenerate_Editor();
    }

    void OnValidate() => Regenerate_Editor();

    public void Regenerate()
    {
        if (mesh == null) return;
        Mesh newMesh = mf.sharedMesh;
        MeshPileUtility.MakePile(ref newMesh, mesh, count, mesh.bounds.size.y, randomRotation, randomScale, jitter);
        mf.sharedMesh = newMesh;
        if (mc != null)
            mc.sharedMesh = newMesh;
    }
    void Regenerate_Editor()
    {
        TryGetComponent(out mf);
        TryGetComponent(out mc);
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
}
