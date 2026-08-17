using FMODUnity;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UIElements;
using GameSystem;
using FMOD.Studio;

[CreateAssetMenu(fileName = "Tutorial step", menuName = "New tutorial step")]
[HelpBox("You can customize your text's appearance using https://namestyler.app/ " +
    "- Variable text size is also supported using '<size=50>' and '</size> for example (which yields a 50px font size)")]
public class TutorialStep : ScriptableObject
{
    public List<SubStep> Steps = new();
    public Timer Timer => _currentTimer;
    [NonSerialized] protected UISystem UI;
    [NonSerialized] protected TaskPanel Tasks;
    [NonSerialized] protected SubtitleOverlay Subtitles;
    [NonSerialized] protected int CurrentSubStep;

    [NonSerialized] private Action _playCallback;
    [NonSerialized] private Timer _currentTimer;
    [NonSerialized] private EventReference _currentAudioReference;
    [NonSerialized] private EventInstance _currentAudioInstance;

    #region Methods

    
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

    public virtual void PlayWhenConditionIsMet(EventVariable startCondition, Action onEnd = null)
    {
        if (startCondition == null)
        {
            Debug.Log("Play without condition");
            Play(onEnd);
        }
        else
        {
            Debug.Log("Play with condition '" + startCondition.name + "'");
            _playCallback = () =>
            {
                Play(onEnd);
                startCondition.RemoveListener(_playCallback);
            };
            startCondition.AddListener(_playCallback);
        }
    }
    //public virtual void PlayUnique() => Play(null);
    public virtual void Play(Action onEnd = null)
    {
        Debug.Log("[TutorialStep] Starting step '" + name + "' (substep count = " + Steps.Count + ")");
        if (!LoadDependencies())
        {
            Debug.LogWarning("[TutorialStep] Couldn't start a tutorial step because it's dependencies couldn't be loaded");
            return;
        }

        // onEnd += () => Debug.Log("onEnd invoked at the end of tutorial step");
        _playCallback = null;
        Action sequence = BuildSequence(0, onEnd);
        sequence();
    }

    private Action BuildSequence(int startIndex = 0, Action onEnd = null)
    {
        if (startIndex >= Steps.Count)
        {
            // Debug.Log("Return onEnd, index is " + startIndex);
            return onEnd;
        }
        return () => PlaySubStep(startIndex, BuildSequence(startIndex + 1, onEnd));
    }

    protected void PlaySubStep(int index, Action onEnd=null)
    {
        if (index >= Steps.Count) return;
        SubStep current = Steps[index];

        GameLoop.OnPause += PauseVoiceline;
        GameLoop.OnResume += ResumeVoiceline;

        // Try to play the voiceline
        if (!current.VoiceLine.IsNull)
        {
            _currentAudioInstance = AudioSystem.PlayAudio(current.VoiceLine);
            _currentAudioReference = current.VoiceLine;
        }

        // Try to display the subtitle
        if (!string.IsNullOrEmpty(current.SubtitleText))
            Subtitles.PlaySubtitle(current.SubtitleText);

        HandleActions(current.ActionsBeforeDialogue);

        // What to do when this substep is over
        Action onPreEnd = () =>
        {
            Subtitles.EndSubtitle();

            HandleActions(current.ActionsAfterDialogue);
            if (_currentAudioReference.Equals(current.VoiceLine))
                _currentAudioReference = default;

            GameLoop.OnPause  -= PauseVoiceline;
            GameLoop.OnResume -= ResumeVoiceline;
            _currentAudioInstance = new EventInstance();
        };

        // Launch the substep timer
        _currentTimer = TimerManager.StartGameplayTimer(
            "Playing tutorial : '" + current.SubtitleText + "' (" + Time.time + ")",
            GetMinimumDurationForSubstep(current),
            () => {
                onPreEnd?.Invoke();
                onEnd?.Invoke();
            }
            );
    }

    void PauseVoiceline()
    {
        if (_currentAudioInstance.isValid())
            _currentAudioInstance.setPaused(true);
    }
    void ResumeVoiceline()
    {
        if (_currentAudioInstance.isValid())
            _currentAudioInstance.setPaused(false);
    }
    void HandleActions(SubStepInstantActions actions)
    {
        HandleFeatureLock(actions.LockFeatures, actions.UnlockFeatures);
        DisplayArrowsFor(actions.DisplayArrows);
        if (actions.RaiseEvent != null) actions.RaiseEvent.Raise();
        HandleTaskRequests(actions.AddTasks);
    }
    private void HandleFeatureLock(LockableFeature lockFeatures, LockableFeature unlockFeatures)
    {
        // Priority is given to unlocking a feature if it is set as both lock and unlock
        // If it's neither specified to lock or unlock, the parameter should stay untouched

        void LockOrUnlock(LockableFeature feature, Param<bool> param)
        {
            bool _lock = 0 != (lockFeatures & feature);
            bool _unlock = 0 != (unlockFeatures & feature);
            if (_lock || _unlock)
                param.Value = _unlock || !_lock;
        }

        LockOrUnlock(LockableFeature.Grab, Globals.Authorizations.Player.AllowPlayerGrab);
        LockOrUnlock(LockableFeature.Cut, Globals.Authorizations.AllowCutting);
        LockOrUnlock(LockableFeature.Cook, Globals.Authorizations.AllowCooking);
        LockOrUnlock(LockableFeature.Kill, Globals.Authorizations.AllowClientKilling);
        LockOrUnlock(LockableFeature.StartShift, Globals.Authorizations.AllowShiftStart);
        LockOrUnlock(LockableFeature.PutIngredientsInPlates, Globals.Authorizations.AllowPuttingIngredientsInPlates);
        LockOrUnlock(LockableFeature.PutPlatesInOvens, Globals.Authorizations.AllowPuttingPlatesInOvens);
        LockOrUnlock(LockableFeature.Serve, Globals.Authorizations.AllowClientServing);
        LockOrUnlock(LockableFeature.GloobyTrash, Globals.Authorizations.AllowTrashingStuff);
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

        /*
        Debug.Log("Duration for '" + step.SubtitleText + "' : audio is " + audioDuration + "s ; text is " + textDuration + "s ; delay is " +
            step.DelayBeforeNextSubStep + "s ; total is " + result + "s, using " + (Mathf.Max(audioDuration, textDuration) == audioDuration ? "audio" : "text") + " as max duration");
        */

        return result;
    }

    private void HandleTaskRequests(List<PlayerTask> tasks)
    {
        foreach (PlayerTask task in tasks)
        {
            if (string.IsNullOrEmpty(task.Name) || task.ResolutionEvent == null)
                return;

            Tasks.BindTask(
                task.Name,
                e => task.ResolutionEvent.AddListener(e),
                e => task.ResolutionEvent.RemoveListener(e)
                );
        }
    }

    private void DisplayArrowsFor(HoveringIcon.IconCategory category)
    {
        HoveringIcon.HideAll();
        foreach (var enumFlag in Enum.GetValues(typeof(HoveringIcon.IconCategory)))
        {
            bool isValid = ((HoveringIcon.IconCategory)enumFlag & category) != 0;
            if (isValid)
                HoveringIcon.DisplayObjectsOfType(category);
        }
    }

    internal void Stop()
    {
        TimerManager.RemoveTimer(_currentTimer?.Name);
        AudioSystem.StopAudio(_currentAudioReference);
    }
    #endregion


    #region Types
    [System.Serializable]
    public struct SubStep
    {
        public SubStepInstantActions ActionsBeforeDialogue;
        public EventReference VoiceLine;
        [ResizableTextArea] public string SubtitleText;
        public SubStepInstantActions ActionsAfterDialogue;
        public float DelayBeforeNextSubStep;
    }
    [System.Serializable]
    public struct SubStepInstantActions
    {
        [EnumFlags] public LockableFeature LockFeatures;
        [EnumFlags] public LockableFeature UnlockFeatures;
        [EnumFlags] public HoveringIcon.IconCategory DisplayArrows;
        public EventVariable RaiseEvent;
        public List<PlayerTask> AddTasks;

    }
    [Serializable]
    public struct PlayerTask
    {
        public string Name;
        public EventVariable ResolutionEvent;
    }
    public enum LockableFeature
    {
        Grab =                      1 << 0,
        Cook =                      1 << 1,
        Cut =                       1 << 2,
        Kill =                      1 << 3,
        StartShift =                1 << 4,
        PutIngredientsInPlates =    1 << 5,
        PutPlatesInOvens =          1 << 6,
        Serve =                     1 << 7,
        GloobyTrash =               1 << 8,
    }

    #endregion
}
