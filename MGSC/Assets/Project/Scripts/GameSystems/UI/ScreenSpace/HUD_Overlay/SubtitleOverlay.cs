
using System;
using UnityEngine;

namespace UI
{

    public class SubtitleOverlay : UIHUDOverlay
    {
        public override bool ForceLoadOnUIInitialization => true;

        public UIWidget SubtitlePrefab;
        private Subtitle Sub;
        public override void OnLoad()
        {
            base.OnLoad();
            OnEnable += WhenEnabled;
        }

        void WhenEnabled()
        {
            if (SubtitlePrefab == null) return;
            GameObject go = GameObject.Instantiate(SubtitlePrefab.gameObject);
            go.transform.SetParent(transform);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.sizeDelta = Vector3.zero;
            rt.anchoredPosition = Vector3.zero;

            Sub = go.GetComponent<Subtitle>();
            Sub.SetVisibility(0f);
            Sub.DisplayText("");

            PlaySubtitle(
                "Lorem ipsum dolor" + Markup.breakLine +
                Markup.ToBold("Lorem ipsum dolor bold") +
                " Lorem ipsum dolor");
            }

        public void PlaySubtitle(string text, float charactersPerSecond=16, float fadeInDuration=0.5f)
        {
            Sub.FadeIn(
                fadeInDuration,
                () => Sub.DisplayText(
                    text, 
                    charactersPerSecond,
                    () => Sub.WaitAndDo(2f, () => Sub.DisplayText(
                        Markup.ToColorLerped("Hello there..." + Markup.breakLine + "  General Kenobi  ", Color.skyBlue, Color.deepPink),
                        10,
                        () => Sub.FadeOut())))
                );
            
        }
    } 
}