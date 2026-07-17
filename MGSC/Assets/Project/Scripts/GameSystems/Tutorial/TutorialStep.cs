using FMODUnity;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UIElements;
using GameSystem;

[CreateAssetMenu(fileName = "Tutorial step", menuName = "New tutorial step")]
[HelpBox("You can customize your text's appearance using https://namestyler.app/ " +
    "- Variable text size is also supported using '<size=50>' and '</size> for example (which yields a 50px font size)")]
public class TutorialStep : ScriptableObject
{
    public EventVariable StartCondition;
    public List<SubStep> Steps = new();

    [NonSerialized] protected UISystem UI;
    [NonSerialized] protected TaskPanel Tasks;
    [NonSerialized] protected SubtitleOverlay Subtitles;
    [NonSerialized] protected int CurrentSubStep;

    private Action _playCallback;
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

    public virtual void PlayWhenConditionIsMet(Action onEnd = null)
    {
        if (StartCondition == null)
            Play(onEnd);
        else
        {
            _playCallback = () => Play(onEnd);
            StartCondition.Subscribe(_playCallback);
        }
    }
    public virtual void Play(Action onEnd = null, bool fadeOut = true)
    {
        Debug.Log("[TutorialStep] Starting step '" + name + "' (substep count = " + Steps.Count + ")");
        if (!LoadDependencies())
        {
            Debug.LogWarning("[TutorialStep] Couldn't start a tutorial step because it's dependencies couldn't be loaded");
            return;
        }

        if (fadeOut)
            onEnd += () => Subtitles.EndSubtitle();
        StartCondition?.Unsubscribe(_playCallback);
        _playCallback = null;
        Action sequence = BuildSequence(0, onEnd);
        sequence();
    }

    private Action BuildSequence(int startIndex = 0, Action onEnd = null)
    {
        if (startIndex >= Steps.Count) return onEnd;
        return () => PlaySubStep(startIndex, BuildSequence(startIndex + 1));
    }

    protected void PlaySubStep(int index, Action onEnd=null)
    {
        if (index >= Steps.Count) return;
        SubStep current = Steps[index];

        Debug.Log("[TutorialStep] Playing substep of '" + name +
            "' with a duration of " + GetMinimumDurationForSubstep(current) + "s.");
        
        if (!current.VoiceLine.IsNull)
            AudioSystem.PlayAudio(current.VoiceLine);

        if (!string.IsNullOrEmpty(current.SubtitleText))
            Subtitles.PlaySubtitle(current.SubtitleText);

        HandleTaskRequest(current.AddTask);
        DisplayArrowsFor(current.DisplayArrowsOn);
        HandleFeatureLock(current.LockFeatures, current.UnlockFeatures);

        TimerManager.StartTimer(
            "Playing tutorial : '" + current.SubtitleText + "' (" + Time.time + ")",
            GetMinimumDurationForSubstep(current),
            () => {
                // TODO : Don't invoke 'onEnd' directly, but check if the requested action was performed before
                onEnd?.Invoke();
            }
            );
    }

    private void HandleFeatureLock(LockableFeature lockFeatures, LockableFeature unlockFeatures)
    {
        LockableFeature feature;
        // Priority is given to unlocking a feature if it is set as both lock and unlock

        feature = LockableFeature.Grab;
        Globals.Gameplay.Player.AllowPlayerGrab.Value =
            0 ==   (lockFeatures & feature) ||
            0 != (unlockFeatures & feature);

        feature = LockableFeature.Cut;
        Globals.Gameplay.AllowCutting.Value =
            0 ==   (lockFeatures & feature) ||
            0 != (unlockFeatures & feature);

        feature = LockableFeature.Cook;
        Globals.Gameplay.AllowCooking.Value =
            0 ==   (lockFeatures & feature) ||
            0 != (unlockFeatures & feature);

        feature = LockableFeature.Kill;
        Globals.Gameplay.AllowClientKilling.Value =
            0 ==   (lockFeatures & feature) ||
            0 != (unlockFeatures & feature);

        feature = LockableFeature.StartShift;
        Globals.Gameplay.AllowShiftStart.Value =
            0 ==   (lockFeatures & feature) ||
            0 != (unlockFeatures & feature);
    }

    float GetMinimumDurationForSubstep(SubStep step)
    {
        // Get audio duration
        float audioDuration = 0;
        if (!step.VoiceLine.IsNull)
        {
            FMOD.RESULT res = RuntimeManager.GetEventDescription(step.VoiceLine).getLength(out int eventDurationInMs);
            if (res == FMOD.RESULT.OK)
                audioDuration = eventDurationInMs / 1000f;
        }

        // Get text display duration
        float textDuration = 0;
        float cps = 16f; // Characters per second
        textDuration = step.SubtitleText.Length / cps;

        // Get maximum value
        float result = Mathf.Max(audioDuration, textDuration);
        result += step.DelayBeforeNextSubStep;

        return result;
    }

    private void HandleTaskRequest(PlayerTask requestedAction)
    {
        if (!string.IsNullOrEmpty(requestedAction.Name) && requestedAction.ResolutionEvent != null)
        Tasks.BindTask(
            requestedAction.Name,
            e => requestedAction.ResolutionEvent.Subscribe(e),
            e => requestedAction.ResolutionEvent.Unsubscribe(e)
            );
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
        [ResizableTextArea] public string SubtitleText;
        public PlayerTask AddTask;
        public HoveringIcon.IconCategory DisplayArrowsOn;
        [EnumFlags] public LockableFeature LockFeatures;
        [EnumFlags] public LockableFeature UnlockFeatures;
        public float DelayBeforeNextSubStep;
    }
    [Serializable]
    public struct PlayerTask
    {
        public string Name;
        public EventVariable ResolutionEvent;
    }
    public enum LockableFeature
    {
        Grab =          1 << 0,
        Cook =          1 << 1,
        Cut =           1 << 2,
        Kill =          1 << 3,
        StartShift =    1 << 4
    }

    #endregion
}
