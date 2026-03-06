

using GameSystem;

namespace Features
{
    public abstract class BaseClientAction : Feature
    {
        public ClientBehaviour ClientOwner { get; set; }
        public ClientManager ClientManager => Reference.GetManager<ClientManager>();
        protected BaseClientAction(ClientBehaviour owner) : base(owner)
        {
            ClientOwner = owner;
        }

        public virtual void Perform()
        {

        }
    }
}