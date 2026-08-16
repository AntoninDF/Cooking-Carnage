using System;
using System.Collections.Generic;
using Clients;
using UnityEngine;
using System.Linq;
using Features;

namespace GameSystem
{
    /// <summary>
    /// Keep track of all current and previous clients (Order,Type...) and more (Waiting, Eating, Served, Killed...)
    /// </summary>
    public class ClientManager : IManager, IFeatureCollection
    {
        #region Properties

        // Public Properties

        public List<Client> CurrentClients => currentClients;
        public ClientWaitingQueue WaitingQueue { get; private set; }
        public int ClientsServedThisShift
        {
            get
            {
                if (_clientsServedPerShift.ContainsKey(Globals.Gameplay.ShiftNumber))
                    return _clientsServedPerShift[Globals.Gameplay.ShiftNumber];
                return 0;
            }
        }
        public int ClientsKilledThisShift
        {
            get
            {
                if (_clientsKilledPerShift.ContainsKey(Globals.Gameplay.ShiftNumber))
                    return _clientsKilledPerShift[Globals.Gameplay.ShiftNumber];
                return 0;
            }
        }
        public Dictionary<EClientType, float> ClientTypeAmount       => UpdateClientTypeData();

        public FeatureCollection Collection { get; set; } = new();
        public ClientSeatCollection ClientSeats { get; set; }
        #endregion

        #region Fields

        // Clients Data Fields
        List<Client> currentClients = new List<Client>(); // script linked to entities
        Dictionary<int, List<ClientData>> clientHistory =  new (); // script copy
        List<ClientData> currentClientHistory {
            get { 
                EnforceClientHistoryKey();
                return clientHistory[Globals.Gameplay.ShiftNumber];
                }}
        Dictionary<int, int> _clientsKilledPerShift = new();
        Dictionary<int, int> _clientsServedPerShift = new();
        
        Dictionary<EClientType, float> clientTypePart = new Dictionary<EClientType, float>();

        private float _timeBeforeGoSeating = 3.5f;
        #endregion
        
        #region Methods

        public ClientManager()
        {
            Register();
            ClientSeats = new(this);
            GameLoop.OnReset += DestroyEveryClient;
        }
        
        /// <summary>
        /// IManager Interface method to register itself to the ReferenceManager
        /// </summary>
        public void Register()
        {
            Reference.SetManager(this);
        }
        public void Unregister()
        {
            Reference.RemoveManager(this);
        }
        public void Init()
        {
            foreach (var client in Reference.AssetReferences.clientList)
            {
                clientTypePart[client.clientType] = 0f;
            }
            
        }

        public void PostInit()
        {
            WaitingQueue = new(this);

            foreach (var seat in ClientSeats.Seats)
            {
                seat.OnClientUnregister += (client => MakeSureClientEat(client, seat));
                seat.OnClientUnregister += (client => AddClientData(client, isDone: true, killed: client.IsCooked));
            }
        }
        
        private void MakeSureClientEat(Client client, ClientSeat seat)
        {
            
            TimerManager.StartGameplayTimer($"{client.UniqueID}Wait For Quit the seat", client.IsCooked ? 1f : _timeBeforeGoSeating, (() => WaitingQueue.OnSeatFreed(seat)));
        }

        private void Reset()
        {
            currentClients = new();
            clientHistory = new();
            _clientsKilledPerShift.Clear();
        }

        public void Register(ClientSeat seat) => ClientSeats.Register(seat);
        public List<ClientSeat> GetClientSeats(bool mustBeAvailable = true) => ClientSeats.GetClientSeats(_ => true, mustBeAvailable);
        public List<ClientSeat> GetClientSeats(Func<ClientSeat, bool> filter, bool mustBeAvailable = true) => ClientSeats.GetClientSeats(filter, mustBeAvailable); 


        /// <summary>
        /// Add client data to the lists (creates a copy in case gameobject gets destroyed)
        /// </summary>
        public void AddClientData(Client client, bool isDone, bool killed = false)
        {
            if (isDone)
            {
                // WIP : Pour l'instant un client doit forcément venir de currentClients pour aller vers History

                if (!currentClients.Contains(client))
                {
                    this.Log(LogType.Warning, "Client"+client.GetEntityId()+" is not a current client and cannot be archived");
                    return;
                }
                
                currentClients.Remove(client);
                AddToHistory(client,killed);
                UpdateClientTypeData();
                
                this.Log(LogType.Log, "Client"+client.GetEntityId()+" Data has been archived");
                
            }
            else
            {
                if (!currentClients.Contains(client))
                {
                    currentClients.Add(client);
                    this.Log(LogType.Log, "Client"+client.GetEntityId()+" is now a current client");
                }
                else
                {
                    this.Log(LogType.Warning, "Client"+client.GetEntityId()+" is already a current client");
                }
            }
        }
        
        
        /// <summary>
        /// Converts Client into ClientData and stores it
        /// </summary>
        void AddToHistory(Client client, bool killed)
        {
            if (HistoryContainsClient(client.GetEntityId()))
            {
                this.Log(LogType.Warning, "Data for Client" + client.GetEntityId() + " has already been archived");

                return;
            }

            int shiftCount = 0;
            if (Reference.TryGetManager(out ShiftManager sm))
                shiftCount = sm.ShiftNumber;

            float moneyGave = client.CurrentOrder?.OrderRating.MoneyGained ?? 0f;
            
            ClientData clientData = new ClientData(
                client.GetEntityId(),
                client.Type,
                killed,
                moneyGave,
                Time.time,
                client.CurrentOrder,
                clientHistory.Count,
                shiftCount
            );

            currentClientHistory.Add(clientData);
            if (killed)
            {
                if (!_clientsKilledPerShift.ContainsKey(Globals.Gameplay.ShiftNumber))
                {
                    _clientsKilledPerShift[Globals.Gameplay.ShiftNumber] = 0;
                }
                _clientsKilledPerShift[Globals.Gameplay.ShiftNumber]++;

                this.Log(LogType.Log, "Client" + client.GetEntityId() + " is the " + ClientsKilledThisShift + "th client killed during this shift");
            }
            else
            {
                if (!_clientsServedPerShift.ContainsKey(Globals.Gameplay.ShiftNumber))
                {
                    _clientsServedPerShift[Globals.Gameplay.ShiftNumber] = 0;
                }
                _clientsServedPerShift[Globals.Gameplay.ShiftNumber]++;

                this.Log(LogType.Log, "Client" + client.GetEntityId() + " is the " + ClientsServedThisShift + "th client served during this shift");
            }
        }

        private void EnforceClientHistoryKey()
        {
            if (!clientHistory.ContainsKey(Globals.Gameplay.ShiftNumber) || clientHistory[Globals.Gameplay.ShiftNumber] == null)
                clientHistory[Globals.Gameplay.ShiftNumber] = new();
        }

        /// <summary>
        /// Checks if the history contains a client data corresponding to the id
        /// </summary>
        bool HistoryContainsClient(int entityId)
        {
            return clientHistory.Any(kvp => kvp.Value.Any(data => data.id == entityId));
        }


        /// <summary>
        /// Returns a sized-copy of the Clients History 
        /// </summary>
        public List<ClientData> GetClientsHistory(int amount=-1)
        {
            //if(clientHistory.Count == 0) { return null; }

            amount = amount == -1 ? currentClientHistory.Count : amount;
            
            List<ClientData> list = new List<ClientData>();
            
            int possibleAmount = Mathf.Min(amount, currentClientHistory.Count);
            
            for (int i = currentClientHistory.Count-1; i >= currentClientHistory.Count - possibleAmount; i--)
                list.Add(currentClientHistory[i]);

            return list;
        }
        public int GetCorrectOrdersCount() 
            => GetClientsHistory().Count(c => !c.killed && c.wasCorrectRecipe && c.shiftNumber == Globals.Gameplay.ShiftNumber);
        public int GetWrongOrdersCount()
            => GetClientsHistory().Count(c => !c.killed && !c.wasCorrectRecipe && c.served && c.shiftNumber == Globals.Gameplay.ShiftNumber);
        public int GetMissedOrdersCount()
            => GetClientsHistory().Count(c => !c.killed && !c.wasCorrectRecipe && !c.served && c.shiftNumber == Globals.Gameplay.ShiftNumber);
        public int GetBurntOrdersCount()
            => GetClientsHistory().Count(c => !c.killed && !c.wasCorrectRecipe && c.served && c.servedRecipe.name.Contains("Failed") && c.shiftNumber == Globals.Gameplay.ShiftNumber);

        public Dictionary<EClientType, int> GetSeenClientTypesCount(int amount = -1)
        {
            List<EClientType> seenTypes = currentClients.Where((client) => true).Select((client) => EClientType.None).ToList();
            seenTypes.InsertRange(0, currentClientHistory.Select((data) => data.clientType).ToList());
            if (amount < -1) throw new ArgumentException("[ClientManager] : You cannot ask for a negative amount of client types. Only use -1 to get all of the seen types.");
            if (amount != -1)
            {
                int startIndex = Mathf.Max(0, seenTypes.Count - 1 - amount);
                int count = seenTypes.Count - startIndex;
                seenTypes = seenTypes.GetRange(startIndex, count);
            }
            Dictionary<EClientType, int> output = new();
            foreach (EClientType type in seenTypes)
            {
                if (!output.ContainsKey(type)) output[type] = 0;
                output[type]++;
            }
            return output;
        }

       /// <summary>
       /// Updates the percentages of clients types received
       /// </summary>
        Dictionary<EClientType, float> UpdateClientTypeData()
        {
            int total = currentClients.Count + currentClientHistory.Count;
           
           if (total == 0) { return clientTypePart; }

           foreach (var type in clientTypePart.Keys.ToList())
           {
               clientTypePart[type] = 0;
           }

           foreach (var t in currentClientHistory)
           {
               clientTypePart[t.clientType]++;
           }

           foreach (var t in currentClients)
           {
                //clientTypePart[t.Type]++;
           }
           
           this.Log(LogType.Log, "Client type Data has been updated : " + clientTypePart);
            
            return clientTypePart;
        }

        /// <summary>
        /// Returns the clientdata wanted if it's a previous client
        /// </summary>
        public ClientData GetClientDataByID(int entityId)
        {
            return currentClientHistory.FirstOrDefault(data => data.id == entityId);
        }

        internal void DestroyEveryClient()
        {
            for (int i = currentClients.Count-1; i >= 0; i--)
            {
                var client = currentClients[i];
                currentClients.RemoveAt(i);
                client.Despawn();
            }
            var clients = GameObject.FindObjectsByType<Client>(FindObjectsSortMode.None);
            foreach (var client in clients)
                client.Despawn();
            WaitingQueue?.Clear();
        }

        #endregion

    }
}
