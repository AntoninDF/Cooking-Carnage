#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Upgrades
{
	[InitializeOnLoad]
	public class UpgradeBindingIndexSynchronizer
	{
		static UpgradeBindingIndexSynchronizer()
		{
			EditorApplication.delayCall += Synchronize;
		}

		private static void Synchronize()
		{
			UpgradeBindingIndex  index = LoadOrCreateIndex();
			if (index == null) return;

			var  upgradeType = GetConcreteUpgradeTypes();
			bool dirty       = false;

			dirty |= RemoveObsoleteEntries(index, upgradeType);
			dirty |= AddMissingEntries(index, upgradeType);

			if (dirty)
			{
				EditorUtility.SetDirty(index);
				AssetDatabase.SaveAssets();
				Debug.Log("[UpgradeBindingIndex] Synchronized successfully");
			}
		}

		private static UpgradeBindingIndex LoadOrCreateIndex()
		{
			var index = Resources.Load<UpgradeBindingIndex>("Upgrades/UpgradeBindingIndex");
            
			if (index != null) return index;
			
			if (!AssetDatabase.IsValidFolder("Assets/Resources"))
				AssetDatabase.CreateFolder("Assets", "Resources");
			if (!AssetDatabase.IsValidFolder("Assets/Resources/Upgrades"))
				AssetDatabase.CreateFolder("Assets/Resources", "Upgrades");

			index = ScriptableObject.CreateInstance<UpgradeBindingIndex>();
			AssetDatabase.CreateAsset(index, "Assets/Resources/Upgrades/UpgradeBindingIndex.asset");
			AssetDatabase.SaveAssets();
			Debug.Log("[UpgradeBindingIndex] Asset created at Resources/Upgrades/UpgradeBindingIndex");
            
			return index;
		}
		
		private static List<Type> GetConcreteUpgradeTypes()
		{
			return AppDomain.CurrentDomain.GetAssemblies()
			                .SelectMany(a => a.GetTypes())
			                .Where(t => t.IsSubclassOf(typeof(Upgrade)) && !t.IsAbstract)
			                .ToList();
		}

		private static bool RemoveObsoleteEntries(UpgradeBindingIndex index, List<Type> validTypes)
		{
			var validNames = new HashSet<string>(validTypes.Select(t => t.Name));
			var toRemove   = index.Bindings.Where(e => !validNames.Contains(e.UpgradeTypeName)).ToList();

			if (toRemove.Count == 0) return false;

			foreach (var entry in toRemove)
			{
				index.Bindings.Remove(entry);
				Debug.Log($"[UpgradeBindingIndex] Removed obsolete entry: {entry.UpgradeTypeName}");
			}
			return true;
		}

		private static bool AddMissingEntries(UpgradeBindingIndex index, List<Type> types)
		{
			var  existingNames = new HashSet<string>(index.Bindings.Select(e => e.UpgradeTypeName));
			bool dirty         = false;

			foreach (var type in types)
			{
				if (existingNames.Contains(type.Name)) continue;

				index.Bindings.Add(new UpgradeBindingEntry { UpgradeTypeName = type.Name, Config = null });
				Debug.Log($"[UpgradeBindingIndex] Added missing entry: {type.Name}");
				dirty = true;
			}
			return dirty;
		}
	}
}
#endif