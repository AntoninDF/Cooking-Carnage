using System.Collections.Generic;
using UnityEngine;

public class VersionSpecificObject : MonoBehaviour
{
    [Tooltip("These objects will be disabled unless we're in the release version")]
    [SerializeField] List<GameObject> _releaseSpecificObjects;
    [Tooltip("These objects will be disabled unless we're in the demo version")]
    [SerializeField] List<GameObject> _demoSpecificObjects;
    [Tooltip("These objects will be disabled unless we're in a development build")]
    [SerializeField] List<GameObject> _debugSpecificObjects;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LoadSpecificObjects();
    }
    private void LoadSpecificObjects()
    {
        bool allowReleaseObj = true;
        bool allowDemoObj = false;
        bool allowDebugObj = false;
#if DEMO
        allowDemoObj = true;
        allowReleaseObj = false;
#endif
#if DEVELOPMENT_BUILD
            allowDebugObj = true;
#endif
        // Apply the settings to the objects
        foreach (GameObject obj in _releaseSpecificObjects)
            obj.SetActive(allowReleaseObj);
        foreach (GameObject obj in _demoSpecificObjects)
            obj.SetActive(allowDemoObj);
        foreach (GameObject obj in _debugSpecificObjects)
            obj.SetActive(allowDebugObj);

    }

}
