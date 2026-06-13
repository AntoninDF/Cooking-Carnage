using System;
using System.Collections.Generic;

namespace Features
{
    public class Interactable : Feature
    {

        public Action<InteractionContext> OnInteraction { get; set; }
        protected List<Func<InteractionContext, bool>> _conditions = new();

        public Interactable(IFeatureCollection owner) : base(owner)
        {
        }

        public virtual bool TryInteract(InteractionContext context = null)
        {
            if (!CanInteract(context)) return false;

            OnInteraction?.Invoke(context);

            return true;
        }

        public virtual bool CanInteract(InteractionContext context = null)
        {
            if (!Enabled) return false;
            if (!CheckConditions(context)) return false;

            return true;
        }
        public void AddCondition(Func<InteractionContext, bool> condition)
        {
            if (!_conditions.Contains(condition))
                _conditions.Add(condition);
        }
        protected bool CheckConditions(InteractionContext context = null)
        {
            if (context == null) return true;

            foreach (var condition in _conditions)
            {
                if (!condition(context)) return false;
            }
            return true;
        }
    }
}