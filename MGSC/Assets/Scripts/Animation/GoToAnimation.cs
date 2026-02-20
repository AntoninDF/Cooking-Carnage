using GameSystem;
using UnityEngine;

public class GoToAnimation : FeatureComponent
{
    public Transform ControlledTransform { get; set; }


    private Vector3 startPos;
    private Vector3 endPos;
    private Transform targetTransform;
    private TimerManager timerManager;

    public GoToAnimation(IFeatureCollection owner, Transform controlledTransform) : base(owner)
    {
        Reference.GetManager(out timerManager);
        ControlledTransform = controlledTransform;
    }

    public void GoTo(Vector3 endPos, float duration=0.1f)
    {
        if (!CanAnimate()) return;
        startPos = ControlledTransform.position;
        this.endPos = endPos;

        targetTransform = null;

        if (duration == 0f)
            SetPos(1);
        else
            timerManager.StartTimer("GoToAnimation", duration, null, false, (elapsed) => SetPos(elapsed / duration));
    }
    public void GoTo(Transform dest, float duration=0.1f, bool setAsParent=true)
    {
        if (!CanAnimate())
        {
            return;
        }

        if (dest == null)
        {
            GoTo(Vector3.zero, duration);
            return;
        }


        if (setAsParent)
        {
            ControlledTransform.SetParent(dest);
            startPos = ControlledTransform.localPosition;
            endPos = Vector3.zero;

        }
        else
        {
            startPos = ControlledTransform.position;
            endPos = dest.position;
        }

            targetTransform = dest;
        if (duration == 0f)
            SetPos(1);
        else
            timerManager.StartTimer("GoToAnimation", duration, null, false, (elapsed) => SetPos(elapsed / duration));
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

    public bool CanAnimate()
    {
        if (!Enabled) return false;
        if (ControlledTransform == null) return false;
        return true;
    }
}
