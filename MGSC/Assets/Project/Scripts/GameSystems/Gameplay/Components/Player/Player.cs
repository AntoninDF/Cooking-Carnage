using Features;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; private set; } = new();

    public PlayerInteractor PlayerActor { get; private set; }
    public F_SoloContainer Container { get; private set; }
    public BasePlayerInputs PlayerInputs { get; private set; }
    public PlayerMovement PlayerMovement { get; private set; }
    public F_LookAtTarget LookedAtObject
    {
        get => _lookedAtObject;
        set
        {
            if (value == _lookedAtObject) return;
            if (_lookedAtObject != null)
                _lookedAtObject.IsLookedAt = false;
            _lookedAtObject = value;
            if (_lookedAtObject != null)
                _lookedAtObject.IsLookedAt = true;
        }
    }
    private F_LookAtTarget _lookedAtObject;

    [SerializeField] private Transform head;
    [SerializeField] private SlotContainer hand;


    void Awake()
    {
        PlayerActor = new(this);
        PlayerActor.SortFeaturesByDistance = true;
        PlayerActor.GetAllFeaturesInSight = false;
        PlayerInputs = new PlayerInputReader(this);

        BindPlayerActions();
        AddInteractions();
    }
    void Start()
    {
        Collection.LateSetup();
        if (hand != null)
        {
            Container = hand.Container;
            Collection.Add(Container);
        }
        else
            Container = new(this, transform);
        PlayerMovement = new(this, transform, head, GetComponent<CharacterController>());
        

    }

    // Update is called once per frame
    void Update()
    {
        Collection.Update();

        var cam = Camera.main;
        if (LookedAtObject != null && !LookedAtObject.CheckIfLookedAt(cam.transform.position, cam.transform.forward))
            LookedAtObject = null;
    }

    void BindPlayerActions()
    {
        PlayerInputs.OnPrimaryAction += () => PlayerActor.TryAnyPlayerAction();
        PlayerInputs.OnMovement += () => PlayerMovement.MovementDirection = PlayerInputs.MovementDirection;
        PlayerInputs.OnLook += () => PlayerMovement.LookOffset = PlayerInputs.LookOffset;
    }

    void AddInteractions()
    {
        PlayerActor.AddPlayerAction(typeof(F_Interactable), PlayerActionType.Primary,
            (feature) =>
            {
                var obj = feature as F_Interactable;
                return obj.TryInteract(new InteractionContext(this));
            }
            );

        PlayerActor.AddConstantAction(typeof(F_LookAtTarget),
            (feature) =>
            {
                // The look at target is always looked at by default, since it is only triggered if in line of sight.
                // So there is no need to check with CheckIfLookedAt().
                var obj = feature as F_LookAtTarget;
                obj.IsLookedAt = true;
                obj.LookPosition = PlayerActor.LookPosition;
                LookedAtObject = obj;
                return true;
            }
            );
    }

    public Transform GetCameraTransform()
    {
        return Camera.main.transform;
    }
}
