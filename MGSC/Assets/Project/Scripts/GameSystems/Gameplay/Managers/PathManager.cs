using System.Collections;
using Features.Path;
using Orders;
using UnityEngine;
using UnityEngine.Splines;

namespace GameSystem
{
    public class PathManager : MonoBehaviour, IManager
    {
        public Spline MainSpline => _splineComponent.Spline;
        public SplineContainer SplineComponent => _splineComponent;
        
        public SO_RecipeUnlockConfig _recipeUnlockCongig;

        public SplineTrafficManager SplineTrafficManager = new ();
        
        [SerializeField] private SplineContainer _splineComponent;

        void Awake()
        {
            if (_splineComponent == null)
            {
                this.Log(LogType.Error, "Please reference a spline component in the path manager's inspector :)");
            }
            Register();

        }

        public void Register()
        {
            Reference.SetManager(this);
        }
        public void Unregister()
        {
            Reference.RemoveManager(this);
        }
        public void Init() { }
    }
}