using Controller;
using GameSystem;
using UI;
using Unity.Cinemachine;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class CameraManager : IManager
{
         #region Properties
        
        #endregion

        #region Fields

        
        private CinemachineVirtualCameraBase camFPV;
        private CinemachineVirtualCameraBase camFocus;

        private GameObject focusPivot;
        
        private CameraState currentState = CameraState.FirstPerson;
        
        private float blendTime = 0.1f;
    
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

            foreach (var cam in player.GetComponentsInChildren<CinemachineVirtualCameraBase>())
            {
                if (cam.Name == "FP Camera")
                {
                    camFPV = cam;
                }
                else if (cam.Name == "Focus Camera")
                {
                    camFocus = cam;
                }
            }
            
            if (camFocus == null || camFPV == null)
                this.Log(LogType.Error, "No Camera found, please fix this");
            
            focusPivot = new GameObject("Focus Pivot");
        }

        public void LookAt(Transform target, float duration = 0, bool showMouse = false)
        {
            Vector3 targetPos = target.transform.position + target.transform.forward * -(0.5f);
            focusPivot.transform.position = targetPos;
            
            camFocus.Follow = focusPivot.transform;
            camFocus.LookAt = target;
            
            ChangeState(CameraState.Focus);

            if (showMouse)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (duration > 0)
            {
                ReferenceManager.Instance.GetManager<TimerManager>().StartTimer("camFocus", duration, () => EnableFirstPersonView(), false);
                this.Log(LogType.Log, $"Camera will focus on {target.name} for {duration}s");
            }
            else
            {
                this.Log(LogType.Log, $"Camera will focus on {target.name}");
            }
            ReferenceManager.Instance.GetManager<UIManager>().PauseGame();
        }

        public void EnableFirstPersonView()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ChangeState(CameraState.FirstPerson);
            ReferenceManager.Instance.GetManager<UIManager>().ResumeGame();
        }

        void ChangeState(CameraState newState)
        {
            if (currentState != newState)
            {
                switch (newState)
                {
                    case CameraState.FirstPerson :
                        (camFocus.Priority,camFPV.Priority) = (10,20);
                        
                        break;
                    case CameraState.Focus :
                        (camFocus.Priority,camFPV.Priority) = (20,10);
                        break;
                }
                
                currentState = newState;
                this.Log(LogType.Log, $"Camera mode updated to {newState}");
            }
            else
            {
                this.Log(LogType.Warning, $"Camera was already in {currentState} mode");
            }
        }

        
    
        #endregion

        enum CameraState
        {
            FirstPerson,
            Focus,
        }
}
