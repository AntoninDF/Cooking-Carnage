using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Experimental.GraphView.GraphView;

public class LookAtSphere : MonoBehaviour
{
    public bool LookedAt => _wasLookedAtThisFrame;

    private bool _wasLookedAtThisFrame;

    [Header("Parameters")]
    [SerializeField] private float _radius;

    [Header("Events")]
    [SerializeField] private UnityEvent _onLookAt;
    [SerializeField] private UnityEvent _onStopLookAt;

    // Update is called once per frame
    void Update()
    {
        CheckLookAt();
    }
    private void CheckLookAt()
    {
        bool viewIntersects = RayIntersectsSphere(Camera.main.transform.position, Camera.main.ScreenPointToRay(new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2)).direction.normalized, transform.position, _radius / 2);
        Debug.Log("View intesects sphere is " + viewIntersects);
        if ((viewIntersects && !_wasLookedAtThisFrame))
        {
            _onLookAt?.Invoke();
        }
        else if (!viewIntersects && _wasLookedAtThisFrame)
            _onStopLookAt?.Invoke();
        _wasLookedAtThisFrame = viewIntersects;
    }
    private bool RayIntersectsSphere(Vector3 rayPos, Vector3 rayDir, Vector3 spherePos, float sphereRadius)
    {
        // Get the components of the vector coming from the point to the camera 
        float x = spherePos.x - rayPos.x;
        float y = spherePos.y - rayPos.y;
        float z = spherePos.z - rayPos.z;
        //rayDir = -rayDir;

        float t = (x * rayDir.x + y * rayDir.y + z * rayDir.z) /
                            (rayDir.x * rayDir.x + rayDir.y * rayDir.y + rayDir.z * rayDir.z);

        float D1 = (rayDir.x * rayDir.x + rayDir.y * rayDir.y + rayDir.z * rayDir.z) * (t * t);
        float D2 = (x * rayDir.x + y * rayDir.y + z * rayDir.z) * 2 * t;
        float D3 = (x * x + y * y + z * z);

        // Solve quadratic formula
        float D = D1 - D2 + D3;

        //Debug.Log(D);
        // Check if the ray is within the radius
        return (D < sphereRadius);
    }

    private void OnDrawGizmos()
    {
        if (Selection.Contains(this.gameObject))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
