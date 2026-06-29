using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(SlotContainer), typeof(MeshPile))]
public class PlatePile : MonoBehaviour
{
    public SlotContainer Container;
    public Item ItemPrefab;

    private Transform _target;
    private MeshFilter _mf;
    private SlotContainer _sc;
    private MeshPile _mp;

    void Start()
    {
        TryGetComponent(out _mp);
        TrySpawn();
        Container.Container.OnDrop += _ => TrySpawn();
        Container.Container.OnDrop += _ => _mp.RemoveOneElement();
        Container.Container.OnDrop += _ => Debug.Log("DROP");
    }
    public void TrySpawn()
    {
        if (Container == null || ItemPrefab == null)
        {
            Debug.LogWarning("[SpawnItemInSlot] Couldn't spawn item because there were missing references on " + name + ".");
            return;
        }

        if (Container.Container.HeldItem != null) return;

        SetupTarget();

        GameObject go = Instantiate(ItemPrefab.gameObject);
        Container.Container.RequestGrab(go.GetComponent<Item>().Grabbable);
    }

    void SetupTarget()
    {
        if (_target == null)
        {
            if (transform.childCount > 0 && transform.GetChild(0).name == "Slot target")
            {
                _target = transform.GetChild(0);
            }
            else
            {
                _target = new GameObject("Slot target").transform;
                _target.SetParent(transform);
            }
        }

        _target.localPosition = Vector3.zero;
        if (TryGetComponent(out _mf))
        {
            float y = _mf.sharedMesh.bounds.max.y;
            _target.localPosition = new Vector3(0, y, 0);
        }

        if (TryGetComponent(out _sc))
        {
            _sc.Container.transform = _target;
        }
    }

    void Reset()
    {
        Container = GetComponent<SlotContainer>();
    }
}
