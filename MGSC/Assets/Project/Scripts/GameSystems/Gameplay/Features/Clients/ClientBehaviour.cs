using System;
using Unity.VisualScripting;
using UnityEngine.AI;

namespace Features
{
    public class ClientBehaviour : Feature, IFeatureCollection
    {
        public FeatureCollection Collection { get; set; } = new();

        public ClientMovement ClientMovement { get; set; }
        public NavMeshAgent Agent { get; set; }

        public BaseClientAction DefaultAction;
        public BaseClientAction LastPerformedAction;

        public Action OnActionPerformed { get; set; }

        public ClientBehaviour(IFeatureCollection owner, NavMeshAgent agent) : base(owner)
        {
            Agent = agent;
            ClientMovement = new(this, agent);

            DefaultAction = new GoToNearestClientSeat(this);
        }

        public override void Update()
        {
            Collection.Update();
        }

        public void Perform(BaseClientAction action)
        {
            action.Perform();
            OnActionPerformed?.Invoke();
            LastPerformedAction = action;
        }
        public void GoToOrderingCounter(Action<ClientSeat> onDestinationReached=null) => GoToCounter<OrderingCounter>(onDestinationReached);
        public void GoToServingCounter(Action<ClientSeat> onDestinationReached=null) => GoToCounter<ServingCounter>(onDestinationReached);
        // T is the type of a required sibling feature of the client seat
        public void GoToCounter<T>(Action<ClientSeat> onDestinationReached = null) where T : Feature
        {
            var action = new GoToNearestClientSeat(this);
            action.Filter += (obj) => obj is ClientSeat seat && seat.TryGetSibling<T>(out var temp);
            action.OnClientSeatReached += onDestinationReached;
            action.NearestCounter.TryRegister(Owner as Client);
            Perform(action);
        }

        public void GoToEmptySeat()
        {

        }
        public void WaitForMeal()
        {

        }
    }
}
