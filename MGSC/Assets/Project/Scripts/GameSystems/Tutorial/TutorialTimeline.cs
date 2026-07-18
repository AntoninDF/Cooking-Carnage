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
                _instance = Resources.Load<TutorialTimeline>("Tutorials/TutorialTimeline");
            return _instance;
        }
    }
    static TutorialTimeline _instance;


    public List<TutorialStep> Introduction = new();
    public List<TutorialStep> CookingLesson = new();
    public List<TutorialStep> ShiftStartingLesson = new();
    public List<TutorialStep> ServingLesson = new();
    public List<TutorialStep> KillingLesson = new();




    [NonSerialized] SubtitleOverlay Subs;
    public static void StartTutorial() => Instance.Start();
    void Start()
    {
        Debug.Log("[TutorialTimeline] Starting the introduction sequence");
        if (Reference.TryGetManager(out UISystem ui))
        {
            Subs = ui.GetWidget<SubtitleOverlay>();
            Subs.Enable(); ;
            PlayTutorialSequence(Introduction);
        }
    }

    private void PlayTutorialSequence(List<TutorialStep> steps)
    {
        Action builtSequence = BuildSequence(steps);
        builtSequence();
    }
    private Action BuildSequence(List<TutorialStep> steps, Action onEnd = null)
    {
        Debug.Log("[TutorialTimeline] Building sequence");
        if (steps == null || steps.Count == 0) return onEnd;
        List<TutorialStep> nextSequence = steps.GetRange(1, steps.Count - 1);
        Debug.Log("NextSequence " + nextSequence.Count);
        return () => steps[0].PlayWhenConditionIsMet(BuildSequence(nextSequence));
    }

    private void End()
    {
        Subs.Disable();
        Subs = null;
    }
}
