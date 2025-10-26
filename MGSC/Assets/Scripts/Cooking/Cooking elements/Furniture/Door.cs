using Controller;
using System.Collections;
using System.Data;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// Opens and closes on interaction
    /// </summary>
    public class Door : Interactable
    {
        #region Properties
        public bool IsOpen => _open;
        public UnityEvent OnOpen => _onOpen;
        public UnityEvent OnClose => _onClose;
        #endregion

        #region Fields
        [Header("Door parameters")]
        [Tooltip("Choose on what axis the opening animation will occur")]
        [SerializeField] private EAxis _axis;
        [Tooltip("How much will the door open, in degrees")]
        [Range(0.0f, 180.0f)]
        [SerializeField] private int _angle;
        [Tooltip("How long will the door take to open/close, in seconds")]
        [SerializeField] private float _animationDuration = 0.2f;
        [Space]
        [Header("Door events")]
        [Tooltip("Invoked when the door opens")]
        [SerializeField] private UnityEvent _onOpen;
        [Tooltip("Invoked when the door closes")]
        [SerializeField] private UnityEvent _onClose;

        private bool _open;
        private float _defaultAngle;
        private float _endAngle;
        private Coroutine _coroutine;
        private Vector3 _offset;
        #endregion

        #region Methods

        private void Start()
        {
            _defaultAngle = GetAxis(transform.rotation.eulerAngles).magnitude;
            _offset = new Vector3(_angle, _angle, _angle);
        }
        public override void Interact(PlayerInteractor interactor)
        {
            base.Interact(interactor);
            Animate();
            _open = !_open;
            if (_open)
                _onOpen?.Invoke();
            else
                _onClose?.Invoke();

        }

        private void Animate()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            SetTargetAngles();
            _coroutine = StartCoroutine(Animate(_animationDuration));
        }

        private void SetTargetAngles()
        {
            _endAngle = _defaultAngle;
            if (!_open)
                _endAngle += GetAxis(_offset).magnitude;
        }

        private IEnumerator Animate(float duration = 0.2f)
        {
            AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            Quaternion startRot = transform.rotation;
            Quaternion endRot = GetTargetRotation();

            for (float timeElapsed = 0.0f; timeElapsed <= duration; timeElapsed += Time.deltaTime)
            {
                float t = (duration == 0) ? 1 : curve.Evaluate(timeElapsed / duration);
                transform.rotation = Quaternion.Lerp(startRot, endRot, t);
                yield return default(IEnumerator);
            }
        }

        private Vector3 GetAxis(Vector3 vec)
        {
            switch (_axis)
            {
                case EAxis.X:
                    vec.Scale(Vector3.right);
                    break;
                case EAxis.Xminus:
                    vec.Scale(Vector3.left);
                    break;
                case EAxis.Y:
                default:
                    vec.Scale(Vector3.up);
                    break;
                case EAxis.Yminus:
                    vec.Scale(Vector3.down);
                    break;
                case EAxis.Z:
                    vec.Scale(Vector3.forward);
                    break;
                case EAxis.Zminus:
                    vec.Scale(Vector3.back);
                    break;
            }
            return vec;
        }
        private Quaternion GetTargetRotation()
        {
            Quaternion outValue = transform.rotation;
            outValue *= Quaternion.AngleAxis((_open ? -_angle : _angle), GetAxis(Vector3.one));
            return outValue;
        }

        #endregion

        private enum EAxis
        {
            X,
            Y,
            Z,
            Xminus,
            Yminus,
            Zminus
        }
    }

}