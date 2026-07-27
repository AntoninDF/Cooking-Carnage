using Controller;
using Features;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Refacto TODO : Separation of concerns, put audio outside of here
    /// </summary>
    public class HeadsUpDisplay : UIHUD
    {
        public override bool ForceLoadOnUIInitialization => true;
        public EventReference SoundForInteractableObjects;
        public Image I_Crosshair;
        public Sprite DefaultCrosshair; 
        public Sprite GrabbableCrosshair; 
        public Sprite InteractableCrosshair; 

        public override void OnLoad()
        {
            base.OnLoad();
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
            else if (target.TryGetSibling(out F_Grabbable grab) && !grab.IsLocked && (grab.Container == null ? true : grab.Container.CanDrop(grab)))
            {
                I_Crosshair.sprite = GrabbableCrosshair;
                AudioSystem.PlayAudio(SoundForInteractableObjects);
            }
            else if (target.TryGetSibling(out F_Interactable interact) && interact.CanInteract(new InteractionContext(PlayerSetup.Player)))
            {
                I_Crosshair.sprite = InteractableCrosshair;
                AudioSystem.PlayAudio(SoundForInteractableObjects);
            }
            else
                I_Crosshair.sprite = DefaultCrosshair;

        }
    } 
}
