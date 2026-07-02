using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;

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

        List<UIWidget> _widgetPrefabs = new();
        List<UIWidget> _loadedWidgets = new();

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
            if (_root != null) GameObject.Destroy(_root);
            if (_HUDFolder != null) GameObject.Destroy(_HUDFolder);
            if (_HUDOverlayFolder != null) GameObject.Destroy(_HUDOverlayFolder);
            if (_menusFolder != null) GameObject.Destroy(_menusFolder);
            if (_menusOverlayFolder != null) GameObject.Destroy(_menusOverlayFolder);
            if (_inactiveFolder != null) GameObject.Destroy(_inactiveFolder);

            // Create the hierarchy
            _root = new GameObject("UI_Root", typeof(InputSystemUIInputModule)).transform;
            _HUDFolder = new GameObject("HUD Folder").transform;
            _HUDOverlayFolder = new GameObject("HUD Overlay Folder").transform;
            _menusFolder = new GameObject("Menus Folder").transform;
            _menusOverlayFolder = new GameObject("Menus Overlay Folder").transform;
            _inactiveFolder = new GameObject("Inactive Folder").transform;

            _HUDFolder.SetParent(_root);
            _HUDOverlayFolder.SetParent(_root);
            _menusFolder.SetParent(_root);
            _menusOverlayFolder.SetParent(_root);
            _inactiveFolder.SetParent(_root);

            _inactiveFolder.gameObject.SetActive(false);

            Debug.Log("[UISystem] Hierarchy reset");
        }

        public void AddPrefabs(IEnumerable<UIWidget> widgetPrefabs) => _widgetPrefabs.AddRange(widgetPrefabs);
        public void StashWidget(UIWidget widget) => widget?.transform.SetParent(_inactiveFolder);
        public void ShowWidget(UIWidget widget)
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
            //Debug.Log($"[UISystem] Showing widget '{widget.name}'");
        }
        public bool HasLoadedWidget<T>()
        {
            foreach (var widget in _loadedWidgets)
                if (widget is T)
                    return true;
            return false;
        }
        public T GetWidget<T>() where T : UIWidget
        {
            T result;

            // Return an already loaded widget if there is one
            if (TryGetWidget(out result))
                return result;

            // Find a suitable widget prefab if there is one
            UIWidget widgetPrefab = null;
            foreach (var current in _widgetPrefabs)
            {
                if (current is T)
                {
                    widgetPrefab = current;
                    break;
                }
            }

            // Try to load it
            UIWidget temp = LoadWidget_Internal(widgetPrefab);
            if (temp == null) return null;

            return temp as T;
        }
        /// <summary>
        /// Looks for a same typed widget in the loaded widgets. If multiple are found, the first one will be provided.
        /// </summary>
        /// <typeparam name="T">The type of the requested widget</typeparam>
        /// <param name="widget">The variable on which to write the result. Will be null if the return is false</param>
        /// <returns>True if a widget of the right type has been found, false otherwise</returns>
        public bool TryGetWidget<T>(out T widget) where T : UIWidget
        {
            widget = null;
            foreach(var current in _loadedWidgets)
            {
                if (current is T result)
                {
                    widget = result;
                    return true;
                }
            }
            return false;
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
        UIWidget LoadWidget_Internal(UIWidget widget)
        {
            if (widget == null) return null; 
            GameObject prefab = widget.gameObject;

            var go = GameObject.Instantiate(prefab);
            go.transform.SetParent(_inactiveFolder.transform);
            var result = go.GetComponent<UIWidget>();
            _loadedWidgets.Add(result);
            result.Owner = this;
            //Debug.Log($"[UISystem] Widget '{result.name}' has been loaded");
            result.OnLoad();
            return result;
        }

        // Not interesting methods
        public void Register() => Reference.SetManager(this);
        public void Unregister() => Reference.RemoveManager(this);
        #endregion
    }

}