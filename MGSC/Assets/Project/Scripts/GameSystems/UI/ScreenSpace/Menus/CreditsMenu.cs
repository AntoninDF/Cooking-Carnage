using UnityEngine.UI;

namespace UI
{
    public class CreditsMenu : UIMenu
    {
        public Button B_Back;

        public override void OnLoad()
        {
            BindAll();
        }

        private void BindAll()
        {
            if (B_Back != null)
            {
                B_Back.onClick.AddListener(OnBack);
            }
        }

        void OnBack()
        {
            GameLoop.RequestTransition(StateTransitions.OpenMainMenu);
        }
    }
}