using System;
using Clients;

namespace Features
{
    public class ServingCounter : Feature
    {
        public Action<Servable> OnServe { get; set; } 
        public ServingCounter(IFeatureCollection owner) : base(owner)
        {
        }

        public void TryServe(Servable servable, Client client)
        {
            client.CurrentOrder.Resolve(servable);
            OnServe?.Invoke(servable);
        }


    }

}