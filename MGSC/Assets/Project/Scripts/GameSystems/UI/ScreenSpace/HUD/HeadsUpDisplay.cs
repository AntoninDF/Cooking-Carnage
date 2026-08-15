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
        public bool PlaySoundWhenLookingAtInteractableObject = false;
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
                if (PlaySoundWhenLookingAtInteractableObject) AudioSystem.PlayAudio(SoundForInteractableObjects);
            }
            else if (target.TryGetSibling(out F_Interactable interact) && interact.CanInteract(new InteractionContext(PlayerSetup.Player)))
            {
                I_Crosshair.sprite = InteractableCrosshair;
                if (PlaySoundWhenLookingAtInteractableObject) AudioSystem.PlayAudio(SoundForInteractableObjects);
            }
            else
                I_Crosshair.sprite = DefaultCrosshair;

        }

        void OnGUI()
        {
            DisplayDebugInformations();
        }


        float lastFpsUpdate;
        float delayBetweenFpsUpdates = 0.2f;
        int currentFps;
        void DisplayDebugInformations()
        {
            int px18 = Screen.height / 60; // 18 pixels is the size we want for a reference FHD screen, but it should scale with higher rez
            int px15 = Screen.height / 72; // Same
            int defaultY = px18;
            int yIncrement = px18;
            int currentLine = 1;

            Rect GetPos() => new Rect(px18, Screen.height - (defaultY + yIncrement * currentLine++), Screen.width * 0.45f, yIncrement);

            if ((Time.time - lastFpsUpdate >= delayBetweenFpsUpdates))
            {
                currentFps = Mathf.RoundToInt(1f / (Time.smoothDeltaTime / Time.timeScale));
                lastFpsUpdate = Time.time;
            }

                if (Globals.Authorizations.DisplayFPS)
            {
                GUI.Label(GetPos(), "FPS : " + currentFps);
            }
        }
    } 
}
