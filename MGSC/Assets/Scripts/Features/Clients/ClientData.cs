
using Clients;
using Orders;

public class ClientData
{
    public int id;
    public EClientType ClientType;
    public bool killed;
    public float moneyGave;
    public float timeWhenDone;
    public Order order;
    public int serviceIndex;
    // Graphic data ? idk if needed, maybe a face icon for UI

    public ClientData(
        int id,
        EClientType clientType = EClientType.None,
        bool killed = false,
        float moneyGave = 0f,
        float timeWhenDone = 0f,
        Order order = null,
        int serviceIndex = 0
    )
    {
        this.id = id;
        ClientType = clientType;
        this.killed = killed;
        this.moneyGave = moneyGave;
        this.timeWhenDone = timeWhenDone;
        this.order = order;
        this.serviceIndex = serviceIndex;
    }

}
