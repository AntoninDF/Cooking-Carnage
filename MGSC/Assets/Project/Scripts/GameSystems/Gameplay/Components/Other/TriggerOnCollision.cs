using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class TriggerOnCollision : MonoBehaviour
{
    public UnityEvent OnCollisionEnter;
    public UnityEvent OnCollisionExit;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnTriggerEnter invoked on " + name);
        OnCollisionEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("OnTriggerExit invoked on " + name);
        OnCollisionExit?.Invoke();
    }

}
