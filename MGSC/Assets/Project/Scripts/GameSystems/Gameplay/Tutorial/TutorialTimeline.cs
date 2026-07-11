using System;
using UI;
using UnityEngine;

public class TutorialTimeline
{
    static TutorialTimeline Instance
    {
        get
        {
            if (_instance == null)
                _instance = new();
            return _instance;
        }
    }
    static TutorialTimeline _instance;
    SubtitleOverlay Subs;
    public static void StartTutorial() => Instance.Start();
    void Start()
    {
        if (Reference.TryGetManager(out UISystem ui))
        {
            Subs = ui.GetWidget<SubtitleOverlay>();
            Subs.Enable(); ;
            Introduction();
        }
    }

    private void Introduction()
    {
        if (Reference.TryGetManager(out UISystem ui))
        {
            var sub = ui.GetWidget<SubtitleOverlay>();
            if (sub != null)
            {
                sub.PlaySubtitle(
                    Markup.ToSized("Hey !", 60) + Markup.breakLine +
                    " How's it going ?",

                OnEnd:
                () => sub.WaitAndDo(2f,

                OnEnd:
                () => sub.PlaySubtitle(
                    "I recorded this tape for you because I know" +
                    " it's your first shift here..." + Markup.breakLine
                    + "I'm sure you may be a little lost, but I'm " + Markup.ToBold("here to help"),

                OnEnd:
                () => sub.WaitAndDo(4f,

                OnEnd:
                () => sub.PlaySubtitle(
                    "So it's quite easy. First, the " + Markup.breakLine +
                    Markup.ToSized(Markup.ToColor("CUSTOMERS !", Color.slateBlue), 50),

                OnEnd:
                () => sub.WaitAndDo(5f,

                OnEnd:
                () =>sub.EndSubtitle(

                OnEnd:
                        End

                )
                )
                )
                )
                )
                )
                );
            }
        }
    }

    private void End()
    {
        Subs.Disable();
        Subs = null;
    }
}
