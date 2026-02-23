using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; private set; } = new();

    public PlayerActor PlayerActor { get; private set; }
    public SoloContainer Container { get; private set; }
    public BasePlayerInputs PlayerInputs { get; private set; }
    public PlayerMovement PlayerMovement { get; private set; }


    [SerializeField] private Transform head;
    [SerializeField] private Transform hand;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerActor = new(this);
        Container = new (this, hand);
        PlayerInputs = new PlayerInputReader(this);
        PlayerMovement = new(this, transform, head, GetComponent<CharacterController>());
        Collection.Init();

        BindPlayerActions();
        AddInteractions();
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
        PlayerInputs.OnLook += () => PlayerMovement.LookDirection = PlayerInputs.LookOffset;
    }

    void AddInteractions()
    {
        PlayerActor.AddPlayerAction(typeof(Interactable),
            (feature) =>
            {
                var obj = feature as Interactable;
                return obj.TryInteract();
            }
            );

        PlayerActor.AddPlayerAction(typeof(Grabbable),
            (feature) =>
            {
                var obj = feature as Grabbable;
                return Container.RequestGrab(obj);
            }
            );

        PlayerActor.AddPlayerAction(typeof(Container),
            (feature) =>
            {
                var obj = feature as Container;
                if (!obj.Passthrough)
                    return obj.RequestGrab(Container.HeldItem);
                else
                {
                    if (Container.RequestGrab(obj.HeldItem)) return true;
                    return obj.RequestGrab(Container.HeldItem);
                }
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
