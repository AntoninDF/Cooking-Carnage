using Clients;
using Cooking;
using Orders;
using UnityEngine;

public class ClientData
{
    public int id;
    public EClientType clientType;
    
    public bool killed;
    public bool served =>order != null && order.IsResolved;
    public bool timedOut => order != null && order.IsCancelled;
    
    public Order order;
    public SO_Recipe orderedRecipe => order?.Recipe;
    public SO_Recipe servedRecipe => order?.ServingInfo.Recipe;
    public bool wasCorrectRecipe => orderedRecipe != null && servedRecipe != null && orderedRecipe == servedRecipe;
    
    public int stars => order?.OrderRating.Stars ?? 0;
    public float moneyGave;
    
    public float orderTimestamp => order?.Timestamp ?? 0f;
    public float seatedTimestamp => order?.SeatedTimestamp ?? 0f;
    public float timeWhenDone;
    public float timeTakenToServe => order != null ? timeWhenDone - seatedTimestamp : 0f;
    
    public int serviceIndex;
    public int shiftNumber;
    // Graphic data ? idk if needed, maybe a face icon for UI
    

    public ClientData(
        int id,
        EClientType clientType,
        bool killed,
        float moneyGave,
        float timeWhenDone,
        Order order,
        int serviceIndex,
        int shiftNumber
    )
    {
        this.id = id;
        this.clientType = clientType;
        this.killed = killed;
        this.moneyGave = moneyGave;
        this.timeWhenDone = timeWhenDone;
        this.order = order;
        this.serviceIndex = serviceIndex;
        this.shiftNumber = shiftNumber;
    }

}
