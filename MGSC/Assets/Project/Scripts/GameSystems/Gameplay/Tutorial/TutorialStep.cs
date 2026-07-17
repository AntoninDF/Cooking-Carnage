using FMODUnity;
using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

[CreateAssetMenu(fileName = "Tutorial step", menuName = "New tutorial step")]
[HelpBox("You can customize your text's appearance using https://namestyler.app/ " +
    "- Variable text size is also supported using '<size=50>' and '</size> for example (which yields a 50px font size)")]
public class TutorialStep : ScriptableObject
{
    public List<SubStep> Steps;

    [NonSerialized] protected UISystem UI;
    [NonSerialized] protected TaskPanel Tasks;
    [NonSerialized] protected SubtitleOverlay Subtitles;
    [NonSerialized] protected int CurrentSubStep;
    protected bool LoadDependencies()
    {
        if (!Reference.TryGetManager(out UI))
            return false;

        UI.GetWidget(out Tasks);
        UI.GetWidget(out Subtitles);

        if (Tasks == null || Subtitles == null)
            return false;

        return true;
    }

    public virtual void Start()
    {
        if (!LoadDependencies())
        {
            Debug.LogWarning("[TutorialStep] Couldn't start a tutorial step because it's dependencies couldn't be loaded");
            return;
        }

        CurrentSubStep = 0;

    }

    protected void PlaySubStep(int index, Action onEnd=null)
    {
        if (index >= Steps.Count) return;
        SubStep current = Steps[index];
        
        if (!current.VoiceLine.IsNull)
            AudioSystem.PlayAudio(current.VoiceLine);
        if (!string.IsNullOrEmpty(current.SubtitleText))
            Subtitles.PlaySubtitle(current.SubtitleText);

        HandleRequestedAction(current.RequestedAction);
        DisplayArrowsFor(current.DisplayArrowsOn);

    }

    private void HandleRequestedAction(PlayerAction requestedAction)
    {
        // TODO
    }

    private void DisplayArrowsFor(HoveringIcon.IconCategory category)
    {
        HoveringIcon.HideAll();
        HoveringIcon.DisplayObjectsOfType(category);
    }



    #region Types
    [System.Serializable]
    public struct SubStep
    {
        public EventReference VoiceLine;
        [TextArea] public string SubtitleText;
        public PlayerAction RequestedAction;
        public HoveringIcon.IconCategory DisplayArrowsOn;

    }
    public enum PlayerAction
    {
        None,
        TakeIngredient,
        CutAnIngredient,
        PlaceACutIngredientInAPlate,
        PlaceAPlateInAnOven,
        StartCooking,
        FinishCooking,
        StartTheShift,
        ServeAClient,
        KillAClient
    }
    #endregion
}
