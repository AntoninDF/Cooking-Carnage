using Orders;

namespace Features
{
    public class ServingEvaluator : Feature
    {
        public ServingEvaluator(IFeatureCollection owner) : base(owner)
        {
        }

        public ServingInfo Evaluate(Servable servable)
        {
            return new();
        }
        public ServingInfo Evaluate(Servable servable, Order order)
        {
            return new();
        }
    }

}