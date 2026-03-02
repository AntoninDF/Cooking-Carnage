using Features;
using GameSystem;
using System;
using UnityEngine;

public class GoToAnimation : Feature
{
    public Transform ControlledTransform { get; set; }
    public Action OnAnimationStart { get; set; }
    public Action OnAnimationStop { get; set; }


    private Vector3 defaultScale;
    private Quaternion defaultRotation;
    private Vector3 startPos;
    private Vector3 endPos;
    private Transform targetTransform;
    private TimerManager timerManager  => Reference.GetManager<TimerManager>();

    public GoToAnimation(IFeatureCollection owner, Transform controlledTransform) : base(owner)
    {
        ControlledTransform = controlledTransform;
        defaultScale = ControlledTransform.lossyScale;
        defaultRotation = ControlledTransform.rotation;
    }

    public void GoTo(Vector3 endPos, float duration=0.1f)
    {
        if (!CanAnimate()) return;

        OnAnimationStart?.Invoke();

        startPos = ControlledTransform.position;
        this.endPos = endPos;

        targetTransform = null;

        if (duration == 0f)
            SetPos(1);
        else
            timerManager.StartTimer("GoToAnimation", duration, null, false, (elapsed) => SetPos(elapsed / duration));
    }
    public void GoTo(Transform dest, Vector3 worldPos, float duration=0.1f, bool setAsParent=true)
    {
        if (!CanAnimate())
        {
            return;
        }

        if (dest == null)
        {
            GoTo(worldPos, duration);
            return;
        }

        OnAnimationStart?.Invoke();

        if (setAsParent)
        {
            ControlledTransform.SetParent(dest);
            var a = defaultScale;
            var b = dest.lossyScale;
            ControlledTransform.localScale = new Vector3(a.x/(b.x==0?1:b.x), a.y / (b.y == 0 ? 1 : b.y), a.z / (b.z == 0 ? 1 : b.z));
            ControlledTransform.rotation = defaultRotation;
            startPos = ControlledTransform.localPosition;
            endPos = worldPos - dest.position;
            endPos = new Vector3(endPos.x / (b.x == 0 ? 1 : b.x), endPos.y / (b.y == 0 ? 1 : b.y), endPos.z / (b.z == 0 ? 1 : b.z));

        }
        else
        {
            startPos = ControlledTransform.position;
            endPos = worldPos;
        }

        targetTransform = dest;
        if (duration == 0f)
            SetPos(1);
        else
            timerManager.StartTimer("GoToAnimation", duration, EndAnimation, false, (elapsed) => SetPos(elapsed / duration));
    }
    public void GoTo(Transform dest, float duration=0.1f, bool setAsParent=true)
    {
        Vector3 pos = Vector3.zero;
        if (dest != null) pos = dest.position;
        GoTo(dest, dest.position, duration, setAsParent);
    }

    private void SetPos(float percentage)
    {
        percentage = Mathf.Clamp01(percentage);
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        if (ControlledTransform.parent == targetTransform)
        {
            ControlledTransform.localPosition = startPos + (endPos - startPos) * curve.Evaluate(percentage);
        }
        else
        {
            ControlledTransform.position = startPos + (endPos - startPos) * curve.Evaluate(percentage);
        }

    }

    private void EndAnimation()
    {
        OnAnimationStop?.Invoke();
    }

    public bool CanAnimate()
    {
        if (!Enabled) return false;
        if (ControlledTransform == null) return false;
        return true;
    }
}
