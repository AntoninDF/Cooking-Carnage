using System;
using UnityEngine;

namespace Client
{

    public class OrderingCounter : Counter
    {
        #region Properties
        
        #endregion

        #region Fields
      

        #endregion

        #region Methods


        public void TryAcceptOrder()
        {
            if (IsFree) return;
            AcceptOrder();
            return;
        }

        private void AcceptOrder()
        {
            _currentClient.AcceptOrder();
            Free();
        }

        #endregion

    }

}