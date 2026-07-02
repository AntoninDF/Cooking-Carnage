using Controller;
using Features;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HeadsUpDisplay : UIHUD
    {
        public override bool ForceLoadOnUIInitialization => true;
        public Image I_Crosshair;
        public Sprite DefaultCrosshair; 
        public Sprite GrabbableCrosshair; 
        public Sprite InteractableCrosshair; 

        public override void OnLoad()
        {
            GameLoop.OnResume += Enable;
            GameLoop.OnPause += Disable;
            UpdateCrosshair(null);
            if (PlayerSetup.Player != null)
            {
                PlayerSetup.Player.OnNewTargetLookedAt += UpdateCrosshair;
            }
        }

        void UpdateCrosshair(F_LookAtTarget target)
        {
            if (I_Crosshair == null)
                return;
            if (target == null)
                I_Crosshair.sprite = DefaultCrosshair;
            else if (target.TryGetSibling(out F_Grabbable grab))
                I_Crosshair.sprite = GrabbableCrosshair;
            else if (target.TryGetSibling(out F_Interactable interact))
                I_Crosshair.sprite = InteractableCrosshair;
            else
                I_Crosshair.sprite = DefaultCrosshair;

        }
    } 
}
