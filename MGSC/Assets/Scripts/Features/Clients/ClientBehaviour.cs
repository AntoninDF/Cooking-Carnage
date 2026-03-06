using System;
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

            DefaultAction = new GoToNearestOrderingCounter(this);
        }

        public void Perform(BaseClientAction action)
        {
            action.Perform();
            OnActionPerformed?.Invoke();
        }
        public void GoToOrderingCounter()
        {

        }

        public void GoToServingCounter()
        {

        }

        public void GoToEmptySeat()
        {

        }
        public void WaitForMeal()
        {

        }
    }
}
