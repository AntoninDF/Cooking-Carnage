using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

namespace GameSystem
{
    public class PathManager : MonoBehaviour, IManager
    {
        public Spline MainSpline => _splineComponent.Spline;
        public SplineContainer SplineComponent => _splineComponent;
        
        [SerializeField] private SplineContainer _splineComponent;

        void Start()
        {
            if (_splineComponent == null)
            {
                Debug.LogError("Please reference a spline component in the path manager's inspector :)");
            }
            Register();

        }

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
    }
}