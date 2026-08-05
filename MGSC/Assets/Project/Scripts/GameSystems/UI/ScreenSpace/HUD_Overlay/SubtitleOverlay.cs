
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
            }

        public void WaitAndDo(float duration, Action OnEnd) => Sub.WaitAndDo(duration, OnEnd);
        public void EndSubtitle(float duration = 0.5f, Action OnEnd = null)
        {
            if (Sub == null) return;
            Sub.FadeOut(duration, OnEnd);
            //Debug.Log("Fade out");
        }
        public Subtitle PlaySubtitle(string text, float charactersPerSecond=16, float fadeInDuration=0.5f, Action OnEnd=null)
        {
            Sub.DisplayText("");
            Sub.FadeIn(
                fadeInDuration,
                () => Sub.DisplayText(
                    text, 
                    charactersPerSecond,
                    OnEnd
                    )
                );
            return Sub;
        }
        public void FadeIn(float duration = 0.5f, Action OnEnd = null) => Sub.FadeIn(duration, OnEnd);
        public void FadeOut(float duration = 0.5f, Action onEnd = null) => Sub.FadeOut(duration, onEnd);
    } 
}