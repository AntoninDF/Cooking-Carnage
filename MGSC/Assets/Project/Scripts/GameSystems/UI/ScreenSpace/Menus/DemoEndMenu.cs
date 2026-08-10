using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class DemoEndMenu : UIMenu
	{
        #region Fields
        public Button B_Next;


        #endregion


        #region Methods
        public override void OnLoad()
        {
            base.OnLoad();
            BindAll();
        }

        void BindAll()
        {
            if (B_Next != null)
                B_Next.onClick.AddListener(OnNext);
        }
        void OnNext()
        {
            GameLoop.RequestTransition(StateTransitions.OpenMainMenu);
        }
        #endregion
    }

}