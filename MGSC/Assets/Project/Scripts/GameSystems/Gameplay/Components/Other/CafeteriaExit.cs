using Features;
using UnityEngine;

public class CafeteriaExit : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();
    public ClientSeat Seat { get; set; }
    public Exit Exit {  get; set; }


    public Transform SeatTransform;

    private void Awake()
    {
        //Seat = new(this, SeatTransform);
        Exit = new(this);
    }


}
