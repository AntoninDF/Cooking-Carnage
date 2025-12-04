using Controller;
using Cooking;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class LookAtTarget : MonoBehaviour, IWatchable
{

    public PlayerInterface Player { get; set; }
    public Collider Collider => _collider;
    public bool IsLookedAt => Player.LookTarget == gameObject;
    public UnityEvent OnLookAt => _onLookAt;
    public UnityEvent OnStopLookAt => _onStopLookAt;

    [SerializeField] private UnityEvent _onLookAt;
    [SerializeField] private UnityEvent _onStopLookAt;
    private Collider _collider;
    private bool _wasLookedAtThisFrame;

    void Start()
    {
        _collider = GetComponent<Collider>();
    }

    void Update()
    {
        CheckIfLookedAt();
    }
    private void CheckIfLookedAt()
    {
        bool lookedAt = (Player != null && Player.LookTarget == this.gameObject);
        if (lookedAt && !_wasLookedAtThisFrame)
            _onLookAt?.Invoke();
        else if (!lookedAt && _wasLookedAtThisFrame)
            _onStopLookAt?.Invoke();
        _wasLookedAtThisFrame = lookedAt;
    }

    /// <summary>
    /// Tell this IWatchable that it is being looked at
    /// </summary>
    /// <param name="interactor"></param>
    public void LookAt(PlayerInteractor interactor)
    {
        Player = interactor.Player;
    }
}
