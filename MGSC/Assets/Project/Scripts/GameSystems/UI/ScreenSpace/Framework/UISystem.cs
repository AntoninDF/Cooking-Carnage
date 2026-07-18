using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace UI
{
    public class UISystem : IManager
    {
        #region Properties



        #endregion


        #region Fields


        // PRIVATE FIELDS

        // Hierarchy folders
        Transform _root;
        Transform _HUDFolder;
        Transform _HUDOverlayFolder;
        Transform _menusFolder;
        Transform _menusOverlayFolder;
        Transform _inactiveFolder;

        List<FullscreenWidget> _widgetPrefabs = new();
        List<FullscreenWidget> _loadedWidgets = new();
        List<FullscreenWidget> _shownWidgets = new();

        #endregion


        #region Methods

        public UISystem()
        {
            //Debug.Log("UISystem constructor called");
            Register();
        }
        public void Init()
        {
            //Debug.Log("UISystem Init");
            AddPrefabs(Reference.AssetReferences.UIPrefabs);
            ResetSceneHierarchy();
            LoadNecessaryWidgets();
        }

        /// <summary>
        /// Deletes any existing (referenced) UI-related gameobjects, and recreates an empty folder list for UI menus instantiated in the future.
        /// </summary>
        void ResetSceneHierarchy()
        {
            // Destroy any existing hierarchy
            if (_root != null) GameObject.Destroy(_root.gameObject);
            if (_HUDFolder != null) GameObject.Destroy(_HUDFolder.gameObject);
            if (_HUDOverlayFolder != null) GameObject.Destroy(_HUDOverlayFolder.gameObject);
            if (_menusFolder != null) GameObject.Destroy(_menusFolder.gameObject);
            if (_menusOverlayFolder != null) GameObject.Destroy(_menusOverlayFolder.gameObject);
            if (_inactiveFolder != null) GameObject.Destroy(_inactiveFolder.gameObject);

            // Create the hierarchy
            _root = new GameObject(
                "UI_Root",
                typeof(InputSystemUIInputModule)
                ).transform;


            _HUDFolder =            MakeMenuFolder("HUD Folder");
            _HUDOverlayFolder =     MakeMenuFolder("HUD Overlay Folder");
            _menusFolder =          MakeMenuFolder("Menus Folder");
            _menusOverlayFolder =   MakeMenuFolder("Menus Overlay Folder");
            _inactiveFolder =       MakeMenuFolder("Inactive Folder");

            _HUDFolder.SetParent(_root);
            _HUDOverlayFolder.SetParent(_root);
            _menusFolder.SetParent(_root);
            _menusOverlayFolder.SetParent(_root);
            _inactiveFolder.SetParent(_root);

            _inactiveFolder.gameObject.SetActive(false);
        }
        Transform MakeMenuFolder(string name)
        {
            var go = new GameObject(name); //, typeof(RectTransform)

            /*
            RectTransform rt = go.GetComponent<RectTransform>();
            var res = new Vector2(1920, 1080);
            rt.localScale = Vector3.one;
            rt.localPosition = new Vector2(res.x/2, res.y/2);
            rt.sizeDelta = res;
            */

            return go.transform;
        }
        void InitPrefabs(IEnumerable<FullscreenWidget> widgetPrefabs)
        {
            foreach (FullscreenWidget widget in widgetPrefabs)
            {
                widget.Init();
            }
        }
        public void AddPrefabs(IEnumerable<FullscreenWidget> widgetPrefabs)
        {
            foreach (var prefab in widgetPrefabs)
            {
                if (prefab == null)
                    Debug.LogWarning("[UISystem] Null prefabs were tried to be added to the UI system. " +
                        "Please replace or remove null references. (Probably in the game profile)");
                if (!_widgetPrefabs.Contains(prefab))
                    _widgetPrefabs.Add(prefab);
            }
            InitPrefabs(widgetPrefabs);
        }
        public List<T> GetShownWidgets<T>() where T : FullscreenWidget
        {
            List<T> result = new();
            foreach (var widget in _shownWidgets)
                if (widget is T validWidget)
                    result.Add(validWidget);
            return result;
        }
        public void StashWidget(FullscreenWidget widget)
        {
            if (widget == null) return;
            widget.transform.SetParent(_inactiveFolder);
            _shownWidgets.Remove(widget);
        }
        public void ShowWidget(FullscreenWidget widget)
        {
            if (widget == null) return;
            Transform parent = null;
            switch (widget)
            {
                case UIHUD:
                    parent = _HUDFolder;
                    break;
                case UIHUDOverlay:
                    parent = _HUDOverlayFolder;
                    break;
                case UIMenu:
                    parent = _menusFolder;
                    break;
                case UIMenuOverlay:
                    parent = _menusOverlayFolder;
                    break;
            }
            if (parent == null)
            {
                Debug.LogWarning("[UISystem] Couldn't show UI widget '" + widget.name + "' because it does not inherit from a valid subtype (UIHUD, UIHUDOverlay, UIMenu, UIMenuOverlay).");
                return;
            }
            widget.transform.SetParent(parent);
            if (!_shownWidgets.Contains(widget))
                _shownWidgets.Add(widget);
            //Debug.Log($"[UISystem] Showing widget '{widget.name}'");
        }
        public bool HasLoadedWidget<T>()
        {
            foreach (var widget in _loadedWidgets)
                if (widget is T)
                    return true;
            return false;
        }
        public void GetWidget<T>(out T widget) where T : FullscreenWidget => widget = GetWidget<T>();
        public T GetWidget<T>() where T : FullscreenWidget
        {
            return GetWidget(typeof(T)) as T;
        }
        public FullscreenWidget GetWidget(Type widgetType)
        {
            FullscreenWidget result;

            
        
            // Return an already loaded widget if there is one
            if (TryGetWidget(widgetType, out result))
                return result;

            // Find a suitable widget prefab if there is one
            FullscreenWidget widgetPrefab = null;
            foreach (var current in _widgetPrefabs)
            {
                if (IsSameOrSubclass(widgetType , current.GetType()))
                {
                    widgetPrefab = current;
                    break;
                }
            }

        // Try to load it
        FullscreenWidget temp = LoadWidget_Internal(widgetPrefab);
            if (temp == null) return null;

            return temp;
        }
        public bool TryGetWidget(Type widgetType, out FullscreenWidget widget)
        {
            widget = null;
            foreach (var current in _loadedWidgets)
            {
                if (IsSameOrSubclass(widgetType, current.GetType()))
                {
                    widget = current;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Looks for a same typed widget in the loaded widgets. If multiple are found, the first one will be provided.
        /// </summary>
        /// <typeparam name="T">The type of the requested widget</typeparam>
        /// <param name="widget">The variable on which to write the result. Will be null if the return is false</param>
        /// <returns>True if a widget of the right type has been found, false otherwise</returns>
        public bool TryGetWidget<T>(out T widget) where T : FullscreenWidget
        {
            bool success = TryGetWidget(typeof(T), out FullscreenWidget result);
            widget = result as T;
            return success;
        }
        public void LoadAllWidgets()
        {
            foreach(var widgetPrefab in _widgetPrefabs)
            {
                LoadWidget_Internal(widgetPrefab);
            }
        }
        public void LoadNecessaryWidgets()
        {
            foreach(var widgetPrefab in _widgetPrefabs)
            {
                if (widgetPrefab.ForceLoadOnUIInitialization)
                    LoadWidget_Internal(widgetPrefab);
            }
        }
        /// <summary>
        /// Instantiates a UI widget prefab and stores it in the corresponding folder. Does not check if it has already been loaded
        /// </summary>
        /// <param name="widget"></param>
        FullscreenWidget LoadWidget_Internal(FullscreenWidget widget)
        {
            if (widget == null) return null; 
            GameObject prefab = widget.gameObject;

            var go = GameObject.Instantiate(prefab);
            go.SetActive(true); // Just in case the prefab was disabled
            go.transform.SetParent(_inactiveFolder.transform);
            /*
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.position = new();
            rt.sizeDelta = new();
            rt.anchoredPosition = new();
            rt.localScale = Vector3.one;
            */
            var result = go.GetComponent<FullscreenWidget>();
            _loadedWidgets.Add(result);
            result.Owner = this;
            //Debug.Log($"[UISystem] Widget '{result.name}' has been loaded");
            result.OnLoad();
            return result;
        }

        // Not interesting methods
        // Utility method to compare types
        bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant) =>
            potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
        public void Register() => Reference.SetManager(this);
        public void Unregister() => Reference.RemoveManager(this);
        #endregion
    }

}