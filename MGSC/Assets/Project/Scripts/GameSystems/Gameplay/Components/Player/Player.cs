using Features;
using System;
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
            OnNewTargetLookedAt?.Invoke(value);
        }
    }
    public Action<F_LookAtTarget> OnNewTargetLookedAt;
    private F_LookAtTarget _lookedAtObject;

    [SerializeField] private Transform head;
    [SerializeField] private SlotContainer hand;
    [SerializeField] private int heldLayer;


    void Awake()
    {
        PlayerActor = new(this);
        PlayerActor.SortFeaturesByDistance = true;
        PlayerActor.GetAllFeaturesInSight = false;
        PlayerInputs = new PlayerInputReader(this);
        PlayerMovement = new(this, transform, head, GetComponent<CharacterController>());

        BindPlayerActions();
        GameLoop.OnPause += UnbindPlayerActions;
        GameLoop.OnResume += BindPlayerActions;
        GameLoop.OnBeforeReset += ResetThis;
        AddInteractions();
    }
    void Start()
    {
        Collection.LateSetup();
        if (hand != null)
        {
            Container = hand.Container;
            hand.Container.SetOwner(this);
            Collection.Add(Container);
        }
        else
            Container = new(this, transform);


        Container.OnGrab += SetHeldLayer;
        Container.OnDrop += ResetHeldLayer;
        Container.Filter.AddCondition(() => Globals.Gameplay.Player.AllowPlayerGrab);

    }

    void ResetThis()
    {
        Container.IsLocked = false;
        if (Container.HeldItem != null) Container.HeldItem.IsLocked = false;
        Container.RequestDrop();
    }
    void SetHeldLayer(F_Grabbable item)
    {
        if (item == null) return;
        if (item.Owner is not Item itemComponent) return;
        itemComponent.ActiveModel.Outline.CanShowOutline = false;
        itemComponent.SetHeld(true);
        itemComponent.StoreLayerRecursive();
        itemComponent.SetLayerRecursive(heldLayer);
    }

    void ResetHeldLayer(F_Grabbable item)
    {
        if (item == null) return;
        if (item.Owner is not Item itemComponent) return;
        itemComponent.ActiveModel.Outline.CanShowOutline = true;
        itemComponent.SetHeld(false);
        itemComponent.RestoreLayerRecursive();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameLoop.IsRunning) return;
        TryResetLookedAtObject();

        Collection.Update();

    }

    private void TryResetLookedAtObject()
    {
        var cam = Camera.main;
        if (LookedAtObject != null && !LookedAtObject.CheckIfLookedAt(cam.transform.position, cam.transform.forward))
            LookedAtObject = null;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit))
        {
            var features = FeatureScannerByHierarchy.Get<F_LookAtTarget>(hit.transform.gameObject);
            LookedAtObject = features.Count > 0 ? features[0] : null;
        }
    }

    void BindPlayerActions()
    {
        PlayerInputs.OnPrimaryAction += TryAnyPlayerAction;
        PlayerInputs.OnMovement += UpdateMovement;
        PlayerInputs.OnLook += UpdateCamera;
    }
    void UnbindPlayerActions()
    {
        PlayerInputs.OnPrimaryAction -= TryAnyPlayerAction;
        PlayerInputs.OnMovement -= UpdateMovement;
        PlayerInputs.OnLook -= UpdateCamera;
    }
    void TryAnyPlayerAction() => PlayerActor.TryAnyPlayerAction();
    void UpdateMovement() => PlayerMovement.MovementDirection = PlayerInputs.MovementDirection;
    void UpdateCamera()
    {
        // The player shouldn't be ablee to look around if the cursor is being shown. It's one or the other
        if (CursorUtility.CursorIsFree) return;
        PlayerMovement.LookOffset = PlayerInputs.LookOffset;
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
