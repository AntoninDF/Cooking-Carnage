

using GameSystem;
using System;

namespace Features
{
    public abstract class BaseClientAction : Feature
    {
        public Action OnActionStart { get; set; }
        public Action OnActionEnd { get; set; }



        public ClientBehaviour ClientOwner { get; set; }
        public ClientManager ClientManager => Reference.GetManager<ClientManager>();
        protected BaseClientAction(ClientBehaviour owner) : base(owner)
        {
            ClientOwner = owner;
        }

        public virtual void Perform()
        {
            OnActionStart?.Invoke();
        }
    }
}