using Features;
using System;
using UnityEngine;

public class ServingArea : MonoBehaviour, IFeatureCollection
{
    #region Properties
    public FeatureCollection Collection { get; set; } = new();

    public ClientSeat ClientSeat { get; set; }
    public ServingCounter ServingCounter { get; set; }
    #endregion

    #region Fields

    public Transform SeatTransform;
    public SlotContainer MealSlot;

    #endregion

    private void Start()
    {
        ClientSeat = new(this, SeatTransform);
        ServingCounter = new(this);
        if (MealSlot == null) Debug.LogWarning("Please assign a meal slot on the serving area component of " + name);
        else
        {
            MealSlot.Container.OnGrab += _ => TryServe();
            MealSlot.Container.Filter += (obj) => obj is F_Grabbable grab && grab.TryGetSibling(out Servable temp, true);
        }
        BindEvents();
    }

    private void BindEvents()
    {
    }

    public bool TryServe()
    {
        Debug.Log("1");
        if (MealSlot.Container.HeldItem == null) return false;
        if (ClientSeat.SittingClient == null ) return false;

        Debug.Log("2");
        ServingCounter.TryServe(MealSlot.Container.HeldItem.GetSibling<Servable>(), ClientSeat.SittingClient);

        return true;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (ClientSeat.SittingClient != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(transform.position, transform.localScale * 1.01f);
            Gizmos.DrawSphere(ClientSeat.SittingClient.transform.position + Vector3.up * 1f, 0.1f);
        }
        else if (ClientSeat.RegisteredClient != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(transform.position, transform.localScale * 1.01f);
            Gizmos.DrawSphere(ClientSeat.RegisteredClient.transform.position + Vector3.up * 1f, 0.1f);
        }
    }
}