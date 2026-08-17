using FMODUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class TutorialTimeline : ScriptableObject
{
    static TutorialTimeline Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.LoadAll<TutorialTimeline>("Tutorials/").FirstOrDefault();
            return _instance;
        }
    }
    static TutorialTimeline _instance;


    public TutorialStep DefaultTutorialStep;
    public List<BindTutorialStepToEvent> TutorialStepsToBindToEvents = new();
    public EventVariable EventRaisedUponEnd;


    private TutorialStep _currentPlayingTutorial;
    List<Action> _tutorialStepBoundCallbacks = new();

    [NonSerialized] SubtitleOverlay Subs;
    public static void StartTutorial() => Instance.Start();
    void Start()
    {
        Debug.Log("[TutorialTimeline] Starting the introduction sequence");
        BindTutorialStepsToEvents(TutorialStepsToBindToEvents);
        Play(DefaultTutorialStep);

        // You have to finish the tutorial to validate it, it is considered as played as soon as you finish the last voiceline
        EventRaisedUponEnd.AddListener(ValidateTutorial);
        EventRaisedUponEnd.AddListener(StopTutorial);
    }
    void ValidateTutorial()
    {
        Globals.Statistics.TutorialHasBeenPlayed.Value = true;
        Achievements.Unlock("ACH_A03");
    }
    void Play(TutorialStep step, Action onEnd = null)
    {
        // Stop the previous tutorial step
        if (_currentPlayingTutorial != null)
        {
            _currentPlayingTutorial.Stop();
        }
        _currentPlayingTutorial = step;

        // Get the subtitle UI widget
        if (Reference.TryGetManager(out UISystem ui))
        {
            Subs = ui.GetWidget<SubtitleOverlay>();
            Subs?.Enable();
        }

        if (step == null)
            return;

        Action callback = () =>
        {
            // Subs?.EndSubtitle();
            onEnd?.Invoke();
        };

        // Play the given tutorial step (if not null)
        step.Play(callback);
    }
    void BindTutorialStepsToEvents(List<BindTutorialStepToEvent> items)
    {
        foreach (var item in items)
        {
            if (item.Event != null && item.TutorialStep != null)
            {
                Action callback = null;
                callback = () =>
                    {
                        Play(item.TutorialStep);
                        item.Event.RemoveListener(callback);
                    };
                item.Event.AddListener(callback);
                _tutorialStepBoundCallbacks.Add(callback);
            }
        }
    }
    public static void StopTutorial()
    {
        ClearBoundTutorialSteps(Instance.TutorialStepsToBindToEvents.Select(item => item.Event));
        StopCurrentStep();
    }

    static void StopCurrentStep()
    {
        // End subtitles, remove tasks
        if (Reference.TryGetManager(out UISystem ui))
        {
            Instance.Subs = ui.GetWidget<SubtitleOverlay>();
            Instance.Subs.EndSubtitle();

            var Tasks = ui.GetWidget<TaskPanel>();
            Tasks.RemoveCurrentTasks();
        }
        AudioSystem.StopAudioForBus_Dialog();
        Instance?._currentPlayingTutorial?.Timer?.Pause();
    }
    static void ClearBoundTutorialSteps(IEnumerable<EventVariable> events)
    {
        foreach (var item in events)
            if (item != null)
                foreach (var callback in Instance._tutorialStepBoundCallbacks)
                    item.RemoveListener(callback);

        Instance._tutorialStepBoundCallbacks.Clear();
    }


    [Serializable]
    public struct BindTutorialStepToEvent
    {
        public EventVariable Event;
        public TutorialStep TutorialStep;
    }
}
