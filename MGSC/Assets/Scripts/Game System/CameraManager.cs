using Controller;
using GameSystem;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : IManager
{
         #region Properties
        
        #endregion

        #region Fields

        private bool fpvMode = true;
        private CinemachineCamera cam;
    
        #endregion

        #region Methods
   
        public CameraManager()
        {
            Register();
        }
        
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
        public void Unregister()
        {
            ReferenceManager.Instance.RemoveManager(this);
        }

        public void Init()
        {
            var player = ReferenceManager.Instance.GetElementOfType<PlayerInterface>();
            cam = player.GetComponentInChildren<CinemachineCamera>();
            
            if (cam == null)
                this.Log(LogType.Error, "No Camera found");
        }

        void LaunchShift(float duration)
        {
            //ReferenceManager.Instance.GetManager<TimerManager>().StartTimer("shiftDuration", duration, () => EndShift(), false);
        }

        

        
    
        #endregion
}
