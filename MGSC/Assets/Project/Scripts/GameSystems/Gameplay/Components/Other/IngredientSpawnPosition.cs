using Cooking;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IngredientSpawnPosition : MonoBehaviour
{
    public static List<IngredientSpawnPosition> Positions = new();

    public SO_Ingredient IngredientData;
    [Range(0f, 1f)]
    [SerializeField] private float _gizmoSize = 0.1f;

    private GameObject _lastSpawnedObject;
    private void Awake()
    {
        if (IngredientData == null)
            enabled = false;
    }
    private void OnEnable()
    {
        Positions.Add(this);
    }
    private void OnDisable()
    {
        Positions.Remove(this);
    }

    public static void SpawnIngredients(int ingredientCount, Func<Ingredient, bool> filter = null)
    {
        int i = 0; // i represents the amount of spawned ingredients, not the loop count
        foreach (var pos in Positions)
        {
            if (i == ingredientCount)
                break;

            Ingredient ing = pos.IngredientData?.Prefab?.GetComponent<Ingredient>();
            if (ing == null)
                continue;

            if (filter != null && !filter.Invoke(ing))
                continue;

            pos.Spawn();
            i++;
        }
    }
    public GameObject Spawn()
    {
        var prefab = IngredientData.Prefab;
        var instance = GameObject.Instantiate(prefab);
        instance.transform.SetParent(transform);
        instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _lastSpawnedObject = instance;
        return instance;
    }

#if UNITY_EDITOR

    float Size => transform.localScale.magnitude * 0.57f * _gizmoSize;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.crimson;
        if (IngredientData != null && IngredientData.name.Contains("Eye", StringComparison.CurrentCultureIgnoreCase))
            Gizmos.color = Color.skyBlue;
        if (IngredientData != null && IngredientData.name.Contains("Fiak", StringComparison.CurrentCultureIgnoreCase))
            Gizmos.color = Color.lightSalmon;
        if (IngredientData != null && IngredientData.name.Contains("Tooth", StringComparison.CurrentCultureIgnoreCase))
            Gizmos.color = Color.seaGreen;
        // 0.57f corresponds approximately to 1/sqrt(3), to compensate the magnitude of the scale
        Gizmos.DrawSphere(transform.position, Size);
    }
    private void OnDrawGizmosSelected()
    {
        string text = "Ingredient spawn position\nValue : ";
        text += IngredientData == null ? "null" : IngredientData.name;
        Handles.Label(transform.position + Vector3.up * (Size + 0.10f), text);
    }

#endif
}
