using Features;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; private set; } = new();

    public PlayerActor PlayerActor { get; private set; }
    public F_SoloContainer Container { get; private set; }
    public BasePlayerInputs PlayerInputs { get; private set; }
    public PlayerMovement PlayerMovement { get; private set; }

    [SerializeField] private Transform head;
    [SerializeField] private SlotContainer hand;


    void Awake()
    {
        PlayerActor = new(this);
        PlayerActor.SortFeaturesByDistance = true;
        PlayerActor.GetAllFeaturesInSight = false;
        PlayerInputs = new PlayerInputReader(this); Collection.LateSetup();

        BindPlayerActions();
        AddInteractions();
    }
    void Start()
    {
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
    }

    void BindPlayerActions()
    {
        PlayerInputs.OnPrimaryAction += () => PlayerActor.TryAnyPlayerAction();
        PlayerInputs.OnMovement += () => PlayerMovement.MovementDirection = PlayerInputs.MovementDirection;
        PlayerInputs.OnLook += () => PlayerMovement.LookOffset = PlayerInputs.LookOffset;
    }

    void AddInteractions()
    {
        PlayerActor.AddPlayerAction(typeof(Interactable), PlayerActionType.Primary,
            (feature) =>
            {
                var obj = feature as Interactable;
                return obj.TryInteract(new InteractionContext(this));
            }
            );

        PlayerActor.AddConstantAction(typeof(LookAtTarget),
            (feature) =>
            {
                // The look at target is always looked at by default, since it is only triggered if in line of sight.
                // So there is no need to check with CheckIfLookedAt().
                var obj = feature as LookAtTarget;
                obj.IsLookedAt = true;
                obj.LookPosition = PlayerActor.LookPosition;
                return true;
            }
            );

    }

    public Transform GetCameraTransform()
    {
        return Camera.main.transform;
    }
}
