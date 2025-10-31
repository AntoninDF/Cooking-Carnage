using System;
using System.Collections.Generic;
using Clients;
using Orders;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.HighDefinition;

namespace GameSystem
{
    /// <summary>
    /// Keep track of all current and previous clients (Order,Type...) and more (Waiting, Eating, Served, Killed...)
    /// </summary>
    public class ClientManager : IManager
    {
        #region Properties
        
        // Public Properties
        public List<Client> CurrentClients => currentClients;
        public Dictionary<EClientType, float> ClientTypeAmount => UpdateClientTypeData();

        #endregion
        
        #region Fields
        
        // Clients Data Fields
        List<Client> currentClients = new List<Client>(); // script linked to entities
        List<ClientDataWIP> clientHistory =  new List<ClientDataWIP>(); // script copy
        
        Dictionary<EClientType, float> clientTypePart = new Dictionary<EClientType, float>(){
            { EClientType.A, 0f },
            { EClientType.B, 0f },
            { EClientType.C, 0f }
        };
        
        
        #endregion
        
        #region Methods

        public ClientManager()
        {
            Register();
        }
        
        /// <summary>
        /// IManager Interface method to register itself to the ReferenceManager
        /// </summary>
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }

        
        /// <summary>
        /// Add client data to the lists (creates a copy in case gameobject gets destroyed)
        /// </summary>
        public void AddClientData(Client client, bool isDone, bool killed = false)
        {
            if (isDone)
            {
                // WIP : Pour l'instant un client doit forcément venir de currentClients pour aller vers History
                
                if (!currentClients.Contains(client)) return;
                
                currentClients.Remove(client);
                AddToHistory(client,killed);
                UpdateClientTypeData();
                //Debug.Log("Client"+client.GetEntityId()+" has been added to the history");
            }
            else
            {
                if (!currentClients.Contains(client))
                {
                    currentClients.Add(client);
                    //Debug.Log("Client"+client.GetEntityId()+" is a current client");
                }
            }
        }
        
        
        /// <summary>
        /// Converts Client into ClientData and stores it
        /// </summary>
        void AddToHistory(Client client, bool killed)
        {
            if (HistoryContainsClient(client.GetEntityId())) { return; }
            
            // WIP : Clearly incomplete
            ClientDataWIP clientData = new ClientDataWIP(client.GetEntityId(),client.Type,killed,-1,Time.time,client.Order,ReferenceManager.Instance.GetManager<ShiftManager>().ShiftCount);
            
            clientHistory.Add(clientData);

            if (client.gameObject)
            {
                client.AutoDestruction();
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
        public List<ClientDataWIP> GetClientsHistory(int amount=-1)
        {
            //if(clientHistory.Count == 0) { return null; }
            
            amount = amount == -1 ? clientHistory.Count : amount;
            
            List<ClientDataWIP> list = new List<ClientDataWIP>();
            
            int possibleAmount = Mathf.Min(amount, clientHistory.Count);
            
            for (int i = clientHistory.Count-1; i >= clientHistory.Count - possibleAmount; i--)
            {
                list.Add(clientHistory[i]);
            }
            return list;
        }

        public Dictionary<EClientType, int> GetSeenClientTypesCount(int amount = -1)
        {
            List<EClientType> seenTypes = currentClients.Where((client) => client.HasOrdered).Select((client) => client.Type).ToList();
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
               switch (t.ClientType)
               {
                   case EClientType.A:
                       clientTypePart[EClientType.A]++;
                       break;
                   case EClientType.B:
                       clientTypePart[EClientType.B]++;
                       break;
                   case EClientType.C:
                       clientTypePart[EClientType.C]++;
                       break;
               }
           }

           foreach (var t in currentClients)
           {
               switch (t.Type)
               {
                   case EClientType.A:
                       clientTypePart[EClientType.A]++;
                       break;
                   case EClientType.B:
                       clientTypePart[EClientType.B]++;
                       break;
                   case EClientType.C:
                       clientTypePart[EClientType.C]++;
                       break;
               }
           }
           
           /* Percentage Conversion
            
            foreach (var type in clientTypePart.Keys.ToList())
           {
               clientTypePart[type] = (float)clientTypePart[type] / total;
           }*/
           
           return clientTypePart;
       }


       /// <summary>
       /// Returns the clientdata wanted if it's a previous client
       /// </summary>
       public ClientDataWIP GetClientDataByID(int entityId)
       {
           return clientHistory.FirstOrDefault(data => data.id == entityId);
       }
        
        #endregion

        public class ClientDataWIP
        {
            public int id;
            public EClientType ClientType;
            public bool killed;
            public float moneyGave;
            public float timeWhenDone;
            public Order order;
            public int serviceIndex;
            // Graphic data ? idk if needed, maybe a face icon for UI
            
            public ClientDataWIP(
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
        
    }
}
