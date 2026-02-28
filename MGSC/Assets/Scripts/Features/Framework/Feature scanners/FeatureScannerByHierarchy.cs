using System.Collections.Generic;
using UnityEngine;

namespace Features
{
    public class FeatureScannerByHierarchy : FeatureScanner
    {
        public GameObject GameObject { get; set; }




        public FeatureScannerByHierarchy(GameObject gameObject)
        {
            GameObject = gameObject;
        }
        public override List<Feature> Get()
        {
            List<Feature> result = new();
            var hierarchy = GetChildren(GameObject);
            var components = GetComponentsFromGameObjects(hierarchy);
            foreach (var comp in components)
            {
                if (comp is IFeatureCollection collection)
                {
                    result.AddRange(collection.AllFeatures);
                }
            }
            return result;
        }

        private List<GameObject> GetChildren(GameObject go, bool addBaseGameObject = true)
        {
            List<GameObject> children = new List<GameObject>();

            if (addBaseGameObject)
                children.Add(go);

            for (int i = 0; i < go.transform.childCount; i++)
            {
                children.AddRange(GetChildren(go.transform.GetChild(i).gameObject, true));
            }
            return children;
        }
        private List<Component> GetComponentsFromGameObjects(List<GameObject> gameObjects)
        {
            var components = new List<Component>();
            foreach (GameObject go in gameObjects)
            {
                components.AddRange(go.GetComponents<Component>());
            }
            return components;
        }
    }
}