using GameSystem;
using System;
using UnityEngine;

namespace Features
{

    public class F_GoToAnimation : Feature
    {
        public Transform ControlledTransform { get; set; }
        public Action OnAnimationStart { get; set; }
        public Action OnAnimationStop { get; set; }


        private Vector3 defaultScale;
        private Quaternion defaultRotation;
        private Vector3 startPos;
        private Vector3 endPos;
        private Transform targetTransform; 

        public F_GoToAnimation(IFeatureCollection owner, Transform controlledTransform) : base(owner)
        {
            ControlledTransform = controlledTransform;
            defaultScale = ControlledTransform.lossyScale;
            defaultRotation = ControlledTransform.rotation;
        }

        public void GoTo(Vector3 endPos, float duration=0.1f)
        {
            if (!CanAnimate()) return;

            OnAnimationStart?.Invoke();

            ControlledTransform.localRotation = Quaternion.identity;
            startPos = ControlledTransform.position;
            this.endPos = endPos;

            targetTransform = null;

            if (duration == 0f)
                SetPos(1);
            else
                TimerManager.StartTimer("GoToAnimation " + endPos + Time.time.ToString(), duration, null, false, (elapsed) => SetPos(elapsed / duration));
        }
        public void GoTo(Transform dest, Vector3 worldPos, float duration=0.1f, bool setAsParent=true)
        {
            if (!CanAnimate())
            {
                return;
            }

            if (dest == null)
            {
                Debug.LogWarning("[GoToAnimation] GoTo : Given destination transform is null");
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
                // Default Rotation is not used because when it's not 0, if the parent transform has non uniform scale, the controlled transform can get skewed or sheared
                ControlledTransform.localRotation = Quaternion.identity; 
                startPos = ControlledTransform.localPosition;
                endPos = worldPos - dest.position;
                endPos = Quaternion.Inverse(dest.rotation) * endPos;
                // Compensate the local offset by the local scale so that 1m is 1m regardless of the parent's scale.
                endPos = new Vector3(endPos.x / (b.x == 0 ? 1 : b.x), endPos.y / (b.y == 0 ? 1 : b.y), endPos.z / (b.z == 0 ? 1 : b.z));
            }
            else
            {
                startPos = ControlledTransform.position;
                endPos = worldPos;
            }

            targetTransform = dest;
            if (duration == 0f)
                SetPos(1f);
            else
            {
                TimerManager.StartTimer("GoToAnimation " + dest.name + Time.time.ToString(), duration, EndAnimation, false, (elapsed) => SetPos(elapsed / duration));
            }
        }
        public void GoTo(Transform dest, float duration=0.1f, bool setAsParent=true)
        {
            Vector3 pos = Vector3.zero;
            if (dest != null)
            {
                pos = dest.position;
                GoTo(dest, dest.position, duration, setAsParent);
            }
            else
                Debug.LogWarning("[GoToAnimation] GoTo : Given destination transform is null");
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

}