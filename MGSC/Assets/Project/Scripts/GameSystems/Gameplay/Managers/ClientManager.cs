using System;
using System.Collections.Generic;
using Clients;
using UnityEngine;
using System.Linq;
using Features;
using Unity.AI.Navigation;

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
        public int ClientsServedThisShift {  get; private set; }
        public int ClientsKilledThisShift {  get; private set; }
        public Dictionary<EClientType, float> ClientTypeAmount => UpdateClientTypeData();

        public FeatureCollection Collection { get; set; } = new();
        public ClientSeatCollection ClientSeats { get; set; }
        #endregion

        #region Fields

        // Clients Data Fields
        List<Client> currentClients = new List<Client>(); // script linked to entities
        List<ClientData> clientHistory =  new List<ClientData>(); // script copy
        
        Dictionary<EClientType, float> clientTypePart = new Dictionary<EClientType, float>();
        #endregion
        
        #region Methods

        public ClientManager()
        {
            Register();
            ClientSeats = new(this);
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
        }

        private void Reset()
        {
            ClientsServedThisShift = 0;
            ClientsKilledThisShift = 0;
            currentClients = new();
            clientHistory = new();
        }

        public void Register(ClientSeat seat) => ClientSeats.Register(seat);
        public List<ClientSeat> GetClientSeats(bool mustBeAvailable = true)
            => ClientSeats.GetClientSeats(_ => true, mustBeAvailable);
        public List<ClientSeat> GetClientSeats(Func<ClientSeat, bool> filter, bool mustBeAvailable = true)
            => ClientSeats.GetClientSeats(filter, mustBeAvailable); 


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
                this.Log(LogType.Warning, "Data for Client"+client.GetEntityId()+" has already been archived");
                
                return;
            }

            int shiftCount = 0;
            if (Reference.TryGetManager(out ShiftManager sm))
                shiftCount = sm.ShiftCount;

            //ClientData clientData = new ClientData(client.GetEntityId(), client.Type, killed, -1, Time.time, client.Order, shiftCount);
            
            //clientHistory.Add(clientData);
            if (killed)
            {
                ClientsKilledThisShift++;
                
                this.Log(LogType.Log, "Client"+client.GetEntityId()+" is the "+ ClientsKilledThisShift+"th client killed during this shift");
            }
            else
            {
                ClientsServedThisShift++;
                
                this.Log(LogType.Log, "Client"+client.GetEntityId()+" is the "+ ClientsServedThisShift+"th client served during this shift");
            }
        }
        
        /// <summary>
        /// Checks if the history contains a client data corresponding to the id
        /// </summary>
        bool HistoryContainsClient(int entityId)
        {
            return clientHistory.Any(data => data.id == entityId);
        }


        /// <summary>
        /// Returns a sized-copy of the Clients History 
        /// </summary>
        public List<ClientData> GetClientsHistory(int amount=-1)
        {
            //if(clientHistory.Count == 0) { return null; }
            
            amount = amount == -1 ? clientHistory.Count : amount;
            
            List<ClientData> list = new List<ClientData>();
            
            int possibleAmount = Mathf.Min(amount, clientHistory.Count);
            
            for (int i = clientHistory.Count-1; i >= clientHistory.Count - possibleAmount; i--)
            {
                list.Add(clientHistory[i]);
            }
            return list;
        }

        public Dictionary<EClientType, int> GetSeenClientTypesCount(int amount = -1)
        {
            List<EClientType> seenTypes = currentClients.Where((client) => true).Select((client) => EClientType.None).ToList();
            seenTypes.InsertRange(0, clientHistory.Select((data) => data.ClientType).ToList());
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
           int total = currentClients.Count + clientHistory.Count;
           
           if (total == 0) { return clientTypePart; }

           foreach (var type in clientTypePart.Keys.ToList())
           {
               clientTypePart[type] = 0;
           }
           
           foreach (var t in clientHistory)
           {
               clientTypePart[t.ClientType]++;
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
           return clientHistory.FirstOrDefault(data => data.id == entityId);
        }
        
        #endregion

    }
}
