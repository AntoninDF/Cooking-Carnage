

using System.Threading;

namespace GameSystem
{

    public class DataManager : IManager
    {
        #region Properties

        public MoneyInfo MoneyInfo { get { EnforceReferences(); return _moneyManager.MoneyInfo; } }


        #endregion

        #region Fields
        MoneyManager _moneyManager;
        #endregion

        #region Methods

        public DataManager()
        {
            Register();
        }

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }

        private void EnforceReferences()
        {
            _moneyManager = ReferenceManager.Instance.GetManager<MoneyManager>();
        }
        #endregion
    }
}