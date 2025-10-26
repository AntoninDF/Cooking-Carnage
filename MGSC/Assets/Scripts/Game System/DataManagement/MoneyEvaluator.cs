

using Orders;
using UnityEngine;

namespace GameSystem
{

    public static class MoneyEvaluator
    {
        public static float Evaluate(Order order)
        {
            if (order.ServingInfo == default(ServingInfo))
            {
                throw new System.Exception("The member 'ServingInfo' of Order was not set to a specific value.");
            }
            float output;
            if (order.Recipe != order.ServingInfo.Recipe) output = 2.5f;
            else output =  10f - Mathf.Lerp(0f, 5f, Mathf.Clamp01(((order.ServingInfo.Timestamp - order.Timestamp) - 20f) / 30f)); // 10€ if served in less than 20s. 5€ if served in 50s

            return output;
        }
    }
}