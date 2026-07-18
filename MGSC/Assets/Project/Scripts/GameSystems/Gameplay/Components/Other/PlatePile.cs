using System.Collections;
using Features;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(MeshPile))]
public class PlatePile : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();
    public F_LookAtTarget LookAtTarget { get; set; }
    public F_Interactable Interactable { get; set; }

    public Item ItemPrefab;

    private Transform _target;
    private MeshFilter _mf;
    private MeshPile _mp;

    int _defaultPileCount;

    private void Awake()
    {
        TryGetComponent(out _mp);
        _defaultPileCount = _mp.count;

        LookAtTarget = new(this, transform);
        Interactable = new(this);
    }

    private void OnEnable()
    {
        GameLoop.OnAfterReset += ResetThis;
    }
    private void OnDisable()
    {
        GameLoop.OnAfterReset -= ResetThis;
    }

    void Start()
    {
        TryGetComponent(out _mf);

        LookAtTarget.OnStartLookAt += () => _mp.SetOutline(true);
        LookAtTarget.OnStopLookAt  += () => _mp.SetOutline(false);
        Interactable.OnInteraction += TrySpawnIntoHand;

        Collection.LateSetup();
    }

    void ResetThis()
    {
        _mp.count = _defaultPileCount;
        _mp.Regenerate();
    }

    private void TrySpawnIntoHand(InteractionContext context)
    {
        StartCoroutine(GrabSpawnedItem(context));
    }

    IEnumerator GrabSpawnedItem(InteractionContext context)
    {
        if (_mp.count == 0) yield break;
        if (!context.TryGetFeature(out F_Container playerContainer)) yield break;
        if (playerContainer.HeldItem != null) yield break;
        if (playerContainer.CanGrab(ItemPrefab.Grabbable)) yield break;

        SetupTarget();

        GameObject go = Instantiate(ItemPrefab.gameObject, _target.position, _target.rotation);
        var item = go.GetComponent<Item>();
        item.Anim.Enabled = true;
        

        yield return null; //The plate need a frame to setup the features
        if (!playerContainer.RequestGrab(item.Grabbable))
        {
            Destroy(go);
            yield break;
        }
        
        item.Grabbable.OnPlayerGrabSFX?.Invoke();

        _mp.RemoveOneElement();
    }

    void SetupTarget()
    {
        if (_target == null)
        {
            if (transform.childCount > 0 && transform.GetChild(0).name == "Slot target")
                _target = transform.GetChild(0);
            else
            {
                _target = new GameObject("Slot target").transform;
                _target.SetParent(transform);
            }
        }

        _target.localPosition = Vector3.zero;
        if (_mf != null && _mf.sharedMesh != null)
        {
            float y = _mf.sharedMesh.bounds.max.y;
            if (_mp != null && _mp.count != 0)
                y -= _mp.mesh.bounds.size.y;
            _target.localPosition = new Vector3(0, y, 0);
        }
    }
}