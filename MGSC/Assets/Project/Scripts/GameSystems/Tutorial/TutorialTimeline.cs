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

    private TutorialStep _currentPlayingTutorial;


    [NonSerialized] SubtitleOverlay Subs;
    public static void StartTutorial() => Instance.Start();
    void Start()
    {
        Debug.Log("[TutorialTimeline] Starting the introduction sequence");
        BindTutorialStepsToEvents(TutorialStepsToBindToEvents);
        Play(DefaultTutorialStep);
        Globals.Statistics.TutorialHasBeenPlayed.Value = true;
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

        // Play the given tutorial step (if not null)
        step.Play(onEnd);
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
            }
        }
    }
    /*
    private void PlayTutorialSequence(List<TutorialStep> steps)
    {
        Action builtSequence = BuildSequence(steps);
        builtSequence();
    }
    private Action BuildSequence(List<TutorialStep> steps, Action onEnd = null)
    {
        Debug.Log("[TutorialTimeline] Building sequence");
        if (steps == null || steps.Count == 0)
        {
            if (Reference.TryGetManager(out UISystem ui))
            {
                ui.GetWidget(out SubtitleOverlay subs);
                onEnd += () => subs?.EndSubtitle();
            }
            return onEnd;
        }
        List<TutorialStep> nextSequence = steps.GetRange(1, steps.Count - 1);
        Debug.Log("Current is " + steps[0].name + ", NextSequence is of length " + nextSequence.Count);
        return () => steps[0].Play(BuildSequence(nextSequence, onEnd));
    }
    */
    private void End()
    {
        Subs?.FadeOut(onEnd:Subs.Disable);
        Subs = null;
    }

    [Serializable]
    public struct BindTutorialStepToEvent
    {
        public EventVariable Event;
        public TutorialStep TutorialStep;
    }
}
