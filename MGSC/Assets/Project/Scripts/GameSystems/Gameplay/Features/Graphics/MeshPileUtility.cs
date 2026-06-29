using System.Diagnostics;
using UnityEngine;

public static class MeshPileUtility
{
    /// <summary>
    /// Any request for a mesh will be aborted and yield no result if it takes longer than this to generate.
    /// </summary>
    public static float MaxGenerationDuration
    {
        get
        {
            if (Application.isPlaying) return 0.05f;
            return 0.3f;
        }
    }
    
    public static int Seed = 0;
    public static bool MakePile_Auto(ref Mesh result, Mesh mesh, int count)
    {
        return MakePile(ref result, mesh, count, mesh.bounds.size.y);
    }
    public static bool MakePile(ref Mesh result, Mesh mesh, int count, float spacing, float randRot01 = 0f, float randScale01 = 0f, float jitter = 0f)
    {
        if (count < 0) return false;
        System.Random _rng = new(Seed);

        var timer = Stopwatch.StartNew();

        Mesh[] toCombine = new Mesh[count];

        // Apply modifications on the mesh for each iteration in the pile, and store these meshes in an array for further combination.
        // They are not combined immediately for performance reasons
        for (int i = 0; i < count; i++)
        {
            toCombine[i] = GetCopy(mesh);

            // rand is systematically computed even if it is not used, so that the random sequence stays consistent anyway
            float rand = (float)_rng.NextDouble();

            // Apply random scale if needed
            if (randScale01 > 0f)
            {
                float scale = 1 + (float)(rand * 2 - 1) * randScale01;
                Scale(ref toCombine[i], scale);
            }

            rand = (float)_rng.NextDouble();

            // Apply random rotation if needed
            if (randRot01 > 0f)
            {
                Quaternion rot = Quaternion.Euler(new Vector3(0, (float)(rand * 2 - 1) * randRot01 * 180, 0));
                Rotate(ref toCombine[i], rot);
            }
            
            // Apply vertical spacing and jitter
            Offset(
                ref toCombine[i],
                new Vector3(0, spacing * i, 0) +
                Quaternion.Euler(new Vector3(0, (float)_rng.NextDouble() * 360, 0)) * Vector3.forward * (float)_rng.NextDouble() * jitter);

            // Make sure that the time budget is not consumed
            if (timer.Elapsed.TotalSeconds > MaxGenerationDuration)
            {
                UnityEngine.Debug.LogWarning("MeshPile generation has been aborted because it was longer than " + MaxGenerationDuration + "s. It generated " + mesh.vertices.Length * (i+1) + " vertices and copied " + (i+1) + " meshes.");
                return false;
            }
        }
        result = Combine(toCombine);
        result.name = mesh.name + " (pile of " + count + ")";
        timer.Stop();
        return true;
    }

    /// <summary>
    /// Offsets all the vertices of the mesh
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="offset"></param>
    public static void Offset(ref Mesh mesh, Vector3 offset)
    {
        Vector3[] p = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            p[i] = mesh.vertices[i] + offset;
        }
        mesh.SetVertices(p);
        mesh.RecalculateBounds();
    }
    /// <summary>
    /// Scales the mesh
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="scale"></param>
    public static void Scale(ref Mesh mesh, float scale)
    {
        Vector3[] p = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            p[i] = mesh.vertices[i] * scale;
        }
        mesh.SetVertices(p);
        mesh.RecalculateBounds();
    }
    /// <summary>
    /// Rotates the mesh
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="rotation"></param>
    public static void Rotate(ref Mesh mesh, Quaternion rotation)
    {
        Vector3[] p = new Vector3[mesh.vertices.Length];
        Vector3[] n = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            p[i] = rotation * mesh.vertices[i];
            n[i] = rotation * mesh.normals[i];
        }
        mesh.SetVertices(p);
        mesh.SetNormals(n);
        mesh.RecalculateBounds();
    }
    /// <summary>
    /// Returns a new copy identical to the reference
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns></returns>
    public static Mesh GetCopy(Mesh mesh)
    {
        Mesh result = new();
        result.name = mesh.name;
        result.SetVertices(mesh.vertices);
        result.SetNormals(mesh.normals);
        result.SetColors(mesh.colors);
        result.SetUVs(0, mesh.uv);
        result.SetUVs(1, mesh.uv2);
        result.SetUVs(2, mesh.uv3);
        result.SetUVs(3, mesh.uv4);
        result.SetTriangles(mesh.triangles, 0, true);

        return result;
    }
    /// <summary>
    /// Combines all meshes into one. Submeshes are not preserved.
    /// </summary>
    /// <param name="meshes"></param>
    /// <returns></returns>
    public static Mesh Combine(params Mesh[] meshes)
    {
        Mesh result = new();
        if (meshes.Length == 0)
        {
            result.name = "Combine instance (empty)";
        }
        result.name = "Combine instance (" + meshes.Length + " meshes)";

        result.SetVertices(GetCombinedVertices(meshes));
        result.SetNormals(GetCombinedNormals(meshes));
        result.SetUVs(0, GetCombinedUV0(meshes));
        result.SetTriangles(GetCombinedTriangles(meshes), 0);
        return result;
    }

    static Vector3[] GetCombinedVertices(Mesh[] meshes)
    {
        int count = 0;
        foreach(Mesh mesh in meshes)
            count += mesh.vertices.Length;
        Vector3[] result = new Vector3[count];
        int curr = 0;
        foreach (Mesh mesh in meshes)
        {
            mesh.vertices.CopyTo(result, curr);
            curr += mesh.vertices.Length;
        }
        return result;
    }
    static Vector3[] GetCombinedNormals(Mesh[] meshes)
    {
        int count = 0;
        foreach(Mesh mesh in meshes)
            count += mesh.normals.Length;
        Vector3[] result = new Vector3[count];
        int curr = 0;
        foreach (Mesh mesh in meshes)
        {
            mesh.normals.CopyTo(result, curr);
            curr += mesh.normals.Length;
        }
        return result;
    }
    static Vector2[] GetCombinedUV0(Mesh[] meshes)
    {
        int count = 0;
        foreach(Mesh mesh in meshes)
            count += mesh.uv.Length;
        Vector2[] result = new Vector2[count];
        int curr = 0;
        foreach (Mesh mesh in meshes)
        {
            mesh.uv.CopyTo(result, curr);
            curr += mesh.uv.Length;
        }
        return result;
    }
    static int[] GetCombinedTriangles(Mesh[] meshes)
    {
        int count = 0;
        foreach(Mesh mesh in meshes)
            count += mesh.triangles.Length;
        int[] result = new int[count];
        int currIndex = 0;
        int addedVerts = 0;
        foreach (Mesh mesh in meshes)
        {
            for (int i = 0; i < mesh.triangles.Length; i++)
            {
                result[i + currIndex] = mesh.triangles[i] + addedVerts;
            }
            currIndex += mesh.triangles.Length;
            addedVerts += mesh.vertices.Length;
        }
        return result;
    }

    public static Mesh Cube(Vector3 size)
    {
        Mesh mesh = new Mesh();
        float x = size.x/2;
        float y = size.y/2;
        float z = size.z/2;
        mesh.SetVertices(new Vector3[]
        {
            new(-x,-y,-z),
            new(-x,y,-z),
            new(x,y,-z),
            new(x,-y,-z),
            new(-x,-y,z),
            new(-x,y,z),
            new(x,y,z),
            new(x,-y,z),
        });
        mesh.SetTriangles(new int[]
        {
            0,1,2,
            0,2,3,
            4,6,5,
            4,7,6,
            0,4,1,
            1,4,5,
            2,1,5,
            2,5,6,
            3,2,6,
            3,6,7,
            3,7,0,
            0,7,4
        }, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.name = "SmoothCube (" + size.x + ", " + size.y + ", " + size.z + ")";
        return mesh;
    }
}
