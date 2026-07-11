
using System;
using UnityEngine;

namespace UI
{

    public class SubtitleOverlay : UIHUDOverlay
    {
        public override bool ForceLoadOnUIInitialization => true;

        public UIWidget SubtitlePrefab;
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

            Subtitle sub = go.GetComponent<Subtitle>();
            sub.DisplayText("Lorem ipsum dolor sit amet sed constectur (1) Lorem ipsum dolor sit amet sed constectur (2) Lorem ipsum dolor sit amet sed constectur (3)", 16);
        }
    } 
}