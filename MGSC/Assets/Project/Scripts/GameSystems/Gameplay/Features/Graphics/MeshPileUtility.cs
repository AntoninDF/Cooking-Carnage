using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
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
    public static bool AllowCaching = true;
    [Serializable]
    private struct PilePreset
    {
        public Mesh Mesh;
        public int Count;
        public float Spacing;
        public float RandomRotation;
        public float RandomScale;
        public float Jitter;
        
        public PilePreset(Mesh mesh,  int count, float spacing, float randomRotation, float randomScale, float jitter)
        {
            Mesh = mesh;
            Count = count;
            Spacing = spacing;
            RandomRotation = randomRotation;
            RandomScale = randomScale;
            Jitter = jitter;
        }
    }
    private static Dictionary<PilePreset, Mesh> _meshCache = new();

    static double totalGenerationTime = 0;

    public static bool MakePile_Auto(ref Mesh result, Mesh mesh, int count)
    {
        return MakePile(ref result, mesh, count, mesh.bounds.size.y);
    }
    /// <summary>
    /// Will return a newly generated pile, or a deep copy (via <see cref="UnityEngine.Object.Instantiate"/>) of a cached one if parameters match the ones from a previously cached version
    /// </summary>
    /// <param name="result"></param>
    /// <param name="mesh"></param>
    /// <param name="count"></param>
    /// <param name="spacing"></param>
    /// <param name="randRot01"></param>
    /// <param name="randScale01"></param>
    /// <param name="jitter"></param>
    /// <returns></returns>
    public static bool MakePile(ref Mesh result, Mesh mesh, int count, float spacing, float randRot01 = 0f, float randScale01 = 0f, float jitter = 0f)
    {
        if (count < 0) return false;
        //UnityEngine.Debug.Log($"[MeshPileUtility] Total generation time is {totalGenerationTime}s");
        PilePreset currentPreset = new(mesh, count, spacing, randRot01, randScale01, jitter);
        if (AllowCaching)
        {
            if (_meshCache.ContainsKey(currentPreset) && _meshCache[currentPreset] != null)
            {
                //UnityEngine.Debug.Log($"[MeshPileUtility] A mesh request has been fulfilled with a cached mesh (count is {count}) :\nPreset :\n\t{currentPreset.Serialize().ToString()}\n\nMesh :\n\t{_meshCache[currentPreset]}");
                result = UnityEngine.Object.Instantiate(_meshCache[currentPreset]);
                return true;
            }
        }


        System.Random _rng = new(Seed);

        var timer = Stopwatch.StartNew();

        Mesh[] toCombine = new Mesh[count];

        // Apply modifications on the mesh for each iteration in the pile, and store these meshes in an array for further combination.
        // They are not combined immediately for performance reasons
        for (int i = 0; i < count; i++)
        {
            toCombine[i] = UnityEngine.Object.Instantiate(mesh);

            Vector3 offset;
            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one;

            // SCALE
            // rand is systematically computed even if it is not used, so that the random sequence stays consistent anyway
            float rand = (float)_rng.NextDouble();
            if (randScale01 > 0f)
                scale *= 1 + (float)(rand * 2 - 1) * randScale01;

            // ROTATION
            rand = (float)_rng.NextDouble();
            if (randRot01 > 0f)
                rotation = Quaternion.Euler(new Vector3(0, (float)(rand * 2 - 1) * randRot01 * 180, 0));
            
            // OFFSET
            offset = new Vector3(0, spacing * i, 0) +
                Quaternion.Euler(new Vector3(0, (float)_rng.NextDouble() * 360, 0)) * Vector3.forward * (float)_rng.NextDouble() * jitter;

            // APPLY CHANGES
            Matrix4x4 mat = Matrix4x4.identity;
            mat = mat * Matrix4x4.Translate(offset);
            mat = mat * Matrix4x4.Rotate(rotation);
            mat = mat * Matrix4x4.Scale(scale);
            ApplyMatrix(ref toCombine[i], mat);
        

            // Make sure that the time budget is not consumed
            if (timer.Elapsed.TotalSeconds > MaxGenerationDuration)
            {
                // UnityEngine.Debug.LogWarning("MeshPile generation has been aborted because it was longer than " + MaxGenerationDuration + "s. It generated " + mesh.vertices.Length * (i+1) + " vertices and copied " + (i+1) + " meshes.");
                return false;
            }
        }
        result = Combine(toCombine);
        result.name = mesh.name + " (pile of " + count + ")";

        if (AllowCaching)
        {
            // We don't cache the same mesh that is passed on so that it doesn't get modified by error. So we instantiate a new one.
            _meshCache.Add(currentPreset, UnityEngine.Object.Instantiate(result));
            UnityEngine.Debug.Log($"[MeshPileUtility] A mesh has been added to cache (cache contains {_meshCache.Count} meshes) (pile of {count} elements) :\nPreset :\n\t{currentPreset.Serialize().ToString().Replace("\n", "\n\t| ")}\n\nMesh :\n\t{result.name}");
        }

        timer.Stop();
        totalGenerationTime += timer.Elapsed.TotalSeconds;

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
        //mesh.RecalculateBounds();
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
        //mesh.RecalculateBounds();
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
        //mesh.RecalculateBounds();
    }
    /// <summary>
    /// Applies a transformation matrix on the mesh
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="mat"></param>
    public static void ApplyMatrix(ref Mesh mesh, Matrix4x4 mat)
    {
        Vector3[] p = new Vector3[mesh.vertices.Length];
        Vector3[] n = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector4 vert = mesh.vertices[i];
            vert.w = 1; // Make sure that positions are offset
            p[i] = mat * vert;
            n[i] = mat * mesh.normals[i]; // Normals keep w with a value of 0 because they shouldn't be offset
        }
        mesh.SetVertices(p);
        mesh.SetNormals(n);
        mesh.RecalculateBounds();
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
